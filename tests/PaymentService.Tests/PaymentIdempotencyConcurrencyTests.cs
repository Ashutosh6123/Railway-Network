using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentService.Clients;
using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Enums;
using PaymentService.Repositories;
using PaymentService.Services;

namespace PaymentService.Tests;

public class PaymentIdempotencyConcurrencyTests
{
    [Test]
    public async Task RepeatedPaymentRequests_CreateOnePaymentAndCallProviderOnce()
    {
        var repository = new InMemoryPaymentRepository();
        var gateway = new CountingPaymentGateway();
        var service = CreateService(repository, gateway);
        var request = new ProcessPaymentRequest("PNR1", 1000m, "payment-key-1");

        await service.ProcessPaymentAsync(request);
        await service.ProcessPaymentAsync(request);

        Assert.That(repository.StoredPaymentCount, Is.EqualTo(1));
        Assert.That(repository.AddCallCount, Is.EqualTo(1));
        Assert.That(gateway.ProcessCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RepeatedRefundRequests_UpdateOnePaymentAndCallProviderOnce()
    {
        var payment = CreateSuccessfulPayment();
        var repository = new InMemoryPaymentRepository(payment);
        var gateway = new CountingPaymentGateway();
        var service = CreateService(repository, gateway);
        var request = new RefundPaymentRequest(payment.TransactionReference, payment.Amount, "refund-key-1");

        await service.RefundAsync(request);
        await service.RefundAsync(request);

        Assert.That(repository.UpdateCallCount, Is.EqualTo(1));
        Assert.That(gateway.RefundCallCount, Is.EqualTo(1));
        Assert.That(repository.LatestUpdatedPayment!.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(repository.LatestUpdatedPayment.RefundIdempotencyKey, Is.EqualTo("refund-key-1"));
    }

    [Test]
    public async Task PaymentAndRefundUseIndependentIdempotencyKeys()
    {
        var repository = new InMemoryPaymentRepository();
        var gateway = new CountingPaymentGateway();
        var service = CreateService(repository, gateway);

        await service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR1", 1000m, "payment-key-1"));
        var payment = repository.GetStoredPayment("payment-key-1")!;

        await service.RefundAsync(new RefundPaymentRequest(
            payment.TransactionReference,
            payment.Amount,
            "refund-key-1"));

        Assert.That(repository.LatestUpdatedPayment!.IdempotencyKey, Is.EqualTo("payment-key-1"));
        Assert.That(repository.LatestUpdatedPayment.RefundIdempotencyKey, Is.EqualTo("refund-key-1"));
        Assert.That(repository.LatestUpdatedPayment.IdempotencyKey,
            Is.Not.EqualTo(repository.LatestUpdatedPayment.RefundIdempotencyKey));
    }

    [Test]
    public async Task ConcurrentPaymentRequests_CallProviderOnlyOnce()
    {
        var repository = new InMemoryPaymentRepository(waitForPaymentKeyLookups: true);
        var gateway = new CountingPaymentGateway();
        var service = CreateService(repository, gateway);
        var request = new ProcessPaymentRequest("PNR1", 1000m, "payment-key-1");

        await Task.WhenAll(
            service.ProcessPaymentAsync(request),
            service.ProcessPaymentAsync(request));

        Assert.That(repository.StoredPaymentCount, Is.EqualTo(1));
        Assert.That(gateway.ProcessCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ConcurrentRefundRequests_CallProviderOnlyOnce()
    {
        var payment = CreateSuccessfulPayment();
        var repository = new InMemoryPaymentRepository(payment, waitForRefundKeyLookups: true);
        var gateway = new CountingPaymentGateway();
        var service = CreateService(repository, gateway);
        var request = new RefundPaymentRequest(payment.TransactionReference, payment.Amount, "refund-key-1");

        await Task.WhenAll(
            service.RefundAsync(request),
            service.RefundAsync(request));

        Assert.That(gateway.RefundCallCount, Is.EqualTo(1));
        Assert.That(repository.UpdateCallCount, Is.EqualTo(1));
        Assert.That(repository.LatestUpdatedPayment!.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(repository.LatestUpdatedPayment.RefundIdempotencyKey, Is.EqualTo("refund-key-1"));
    }

    private static PaymentService.Services.PaymentService CreateService(
        InMemoryPaymentRepository repository,
        CountingPaymentGateway gateway)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DummyRazorpay:SimulateSuccess"] = "true"
            })
            .Build();

        return new PaymentService.Services.PaymentService(repository, gateway, configuration);
    }

    private static Payment CreateSuccessfulPayment()
    {
        return new Payment
        {
            Id = 1,
            BookingPnr = "PNR1",
            Amount = 1000m,
            PaymentStatus = PaymentStatus.Successful,
            IdempotencyKey = "payment-key-1",
            TransactionReference = "dummy_payment_1",
            CreatedAt = DateTime.UtcNow
        };
    }

    private sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly ConcurrentDictionary<string, Payment> _paymentsByIdempotencyKey = new();
        private readonly ConcurrentDictionary<string, Payment> _paymentsByRefundIdempotencyKey = new();
        private Payment? _paymentByTransactionReference;
        private readonly bool _waitForPaymentKeyLookups;
        private readonly bool _waitForRefundKeyLookups;
        private readonly TaskCompletionSource _paymentLookupBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _refundLookupBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _paymentLookupCount;
        private int _refundLookupCount;
        private int _addCallCount;
        private int _updateCallCount;
        private int _refundClaimed;

        public InMemoryPaymentRepository(
            Payment? payment = null,
            bool waitForPaymentKeyLookups = false,
            bool waitForRefundKeyLookups = false)
        {
            _paymentByTransactionReference = payment;
            _waitForPaymentKeyLookups = waitForPaymentKeyLookups;
            _waitForRefundKeyLookups = waitForRefundKeyLookups;

            if (payment is not null)
            {
                _paymentsByIdempotencyKey.TryAdd(payment.IdempotencyKey, payment);
            }
        }

        public int AddCallCount => _addCallCount;
        public int UpdateCallCount => _updateCallCount;
        public int StoredPaymentCount => _paymentsByIdempotencyKey.Count;
        public Payment? LatestUpdatedPayment { get; private set; }

        public Task<Payment?> GetByIdAsync(int id) => Task.FromResult<Payment?>(null);

        public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey)
        {
            if (_waitForPaymentKeyLookups)
            {
                if (Interlocked.Increment(ref _paymentLookupCount) == 2)
                {
                    _paymentLookupBarrier.TrySetResult();
                }

                await _paymentLookupBarrier.Task;
            }

            _paymentsByIdempotencyKey.TryGetValue(idempotencyKey, out var payment);
            return payment;
        }

        public async Task<Payment?> GetByRefundIdempotencyKeyAsync(string refundIdempotencyKey)
        {
            if (_waitForRefundKeyLookups)
            {
                if (Interlocked.Increment(ref _refundLookupCount) == 2)
                {
                    _refundLookupBarrier.TrySetResult();
                }

                await _refundLookupBarrier.Task;
            }

            _paymentsByRefundIdempotencyKey.TryGetValue(refundIdempotencyKey, out var payment);
            return payment;
        }

        public Task<Payment?> GetByTransactionReferenceAsync(string transactionReference)
        {
            return Task.FromResult(_paymentByTransactionReference?.TransactionReference == transactionReference
                ? CreateCopy(_paymentByTransactionReference)
                : null);
        }

        public Task<bool> TryClaimRefundAsync(int paymentId, string refundIdempotencyKey)
        {
            if (Interlocked.CompareExchange(ref _refundClaimed, 1, 0) != 0)
            {
                return Task.FromResult(false);
            }

            if (_paymentByTransactionReference is null ||
                _paymentByTransactionReference.Id != paymentId ||
                _paymentByTransactionReference.PaymentStatus != PaymentStatus.Successful ||
                _paymentByTransactionReference.RefundIdempotencyKey is not null)
            {
                return Task.FromResult(false);
            }

            _paymentByTransactionReference.RefundIdempotencyKey = refundIdempotencyKey;
            _paymentByTransactionReference.RefundStatus = RefundStatus.Pending;
            _paymentsByRefundIdempotencyKey.TryAdd(refundIdempotencyKey, _paymentByTransactionReference);
            return Task.FromResult(true);
        }

        public Task AddAsync(Payment payment)
        {
            Interlocked.Increment(ref _addCallCount);

            if (!_paymentsByIdempotencyKey.TryAdd(payment.IdempotencyKey, payment))
            {
                throw new DbUpdateException("A payment with this idempotency key already exists.");
            }

            _paymentByTransactionReference = payment;

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Payment payment)
        {
            Interlocked.Increment(ref _updateCallCount);
            LatestUpdatedPayment = payment;
            _paymentByTransactionReference = payment;

            if (!string.IsNullOrWhiteSpace(payment.RefundIdempotencyKey))
            {
                _paymentsByRefundIdempotencyKey.TryAdd(payment.RefundIdempotencyKey, payment);
            }

            return Task.CompletedTask;
        }

        public Payment? GetStoredPayment(string idempotencyKey)
        {
            _paymentsByIdempotencyKey.TryGetValue(idempotencyKey, out var payment);
            return payment;
        }

        private static Payment CreateCopy(Payment payment)
        {
            return new Payment
            {
                Id = payment.Id,
                BookingPnr = payment.BookingPnr,
                Amount = payment.Amount,
                PaymentStatus = payment.PaymentStatus,
                TransactionReference = payment.TransactionReference,
                IdempotencyKey = payment.IdempotencyKey,
                RefundIdempotencyKey = payment.RefundIdempotencyKey,
                RefundStatus = payment.RefundStatus,
                CreatedAt = payment.CreatedAt
            };
        }
    }

    private sealed class CountingPaymentGateway : IPaymentGateway
    {
        private int _processCallCount;
        private int _refundCallCount;
        private readonly bool _waitForRefundCalls;
        private readonly TaskCompletionSource _refundCallBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CountingPaymentGateway(bool waitForRefundCalls = false)
        {
            _waitForRefundCalls = waitForRefundCalls;
        }

        public int ProcessCallCount => _processCallCount;
        public int RefundCallCount => _refundCallCount;

        public Task<PaymentGatewayResult> ProcessPaymentAsync(string paymentReference, decimal amount, bool simulateSuccess)
        {
            Interlocked.Increment(ref _processCallCount);
            return Task.FromResult(new PaymentGatewayResult(true, "dummy_payment_1", "Payment succeeded."));
        }

        public async Task<RefundGatewayResult> RefundAsync(string providerTransactionReference, decimal amount)
        {
            if (_waitForRefundCalls && Interlocked.Increment(ref _refundCallCount) == 2)
            {
                _refundCallBarrier.TrySetResult();
            }
            else if (!_waitForRefundCalls)
            {
                Interlocked.Increment(ref _refundCallCount);
            }

            if (_waitForRefundCalls)
            {
                await _refundCallBarrier.Task;
            }

            return new RefundGatewayResult(true, "dummy_refund_1", "Refund succeeded.");
        }
    }
}
