using Microsoft.Extensions.Configuration;
using PaymentService.Clients;
using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Enums;
using PaymentService.Repositories;
using PaymentService.Services;

namespace PaymentService.Tests;

public class PaymentServiceTests
{
    [Test]
    public void ProcessPaymentAsync_RejectsEmptyBookingPnr()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ProcessPaymentAsync(new ProcessPaymentRequest(" ", 100m, "payment-key")));
    }

    [Test]
    public void ProcessPaymentAsync_RejectsNonPositiveAmount()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 0m, "payment-key")));
    }

    [Test]
    public void ProcessPaymentAsync_RejectsMissingIdempotencyKey()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 100m, " ")));
    }

    [Test]
    public async Task ProcessPaymentAsync_CreatesSuccessfulPaymentWithProviderReference()
    {
        var repository = new FakePaymentRepository();
        var gateway = new FakePaymentGateway
        {
            ProcessResult = new PaymentGatewayResult(true, "dummy_payment_1", "Payment succeeded.")
        };
        var service = CreateService(repository, gateway);

        var result = await service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 250m, "payment-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(PaymentStatus.Successful));
        Assert.That(result.TransactionReference, Is.EqualTo("dummy_payment_1"));
        Assert.That(repository.AddedPayments, Has.Count.EqualTo(1));
        Assert.That(repository.AddedPayments[0].TransactionReference, Is.EqualTo("dummy_payment_1"));
        Assert.That(repository.AddedPayments[0].CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(gateway.LastPaymentReference, Is.EqualTo("PNR10"));
        Assert.That(gateway.LastSimulateSuccess, Is.True);
    }

    [Test]
    public async Task ProcessPaymentAsync_CreatesFailedPaymentWithEmptyTransactionReference()
    {
        var repository = new FakePaymentRepository();
        var gateway = new FakePaymentGateway
        {
            ProcessResult = new PaymentGatewayResult(false, null, "Payment failed.")
        };
        var service = CreateService(repository, gateway);

        var result = await service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 250m, "payment-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(PaymentStatus.Failed));
        Assert.That(result.TransactionReference, Is.Null);
        Assert.That(repository.AddedPayments[0].TransactionReference, Is.Empty);
    }

    [TestCase(PaymentStatus.Successful)]
    [TestCase(PaymentStatus.Failed)]
    public async Task ProcessPaymentAsync_ReturnsExistingPaymentWithoutCallingProvider(PaymentStatus status)
    {
        var existingPayment = CreatePayment(status, "payment-key", "dummy_payment_1");
        var repository = new FakePaymentRepository { PaymentByIdempotencyKey = existingPayment };
        var gateway = new FakePaymentGateway();
        var service = CreateService(repository, gateway);

        var result = await service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 250m, "payment-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(status));
        Assert.That(gateway.ProcessCallCount, Is.Zero);
        Assert.That(repository.AddedPayments, Is.Empty);
    }

    [Test]
    public void ProcessPaymentAsync_PropagatesProviderHttpFailure()
    {
        var gateway = new FakePaymentGateway { ProcessException = new HttpRequestException("Provider unavailable.") };
        var service = CreateService(gateway: gateway);

        Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ProcessPaymentAsync(new ProcessPaymentRequest("PNR10", 250m, "payment-key")));
    }

    [TestCase("", 100, "refund-key")]
    [TestCase("PNR10", 0, "refund-key")]
    [TestCase("PNR10", 100, " ")]
    public void RefundAsync_RejectsInvalidInput(string bookingPnr, decimal amount, string idempotencyKey)
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.RefundAsync(new RefundPaymentRequest(bookingPnr, amount, idempotencyKey)));
    }

    [Test]
    public async Task RefundAsync_RefundsSuccessfulPaymentAndPreservesPaymentIdempotencyKey()
    {
        var payment = CreatePayment(PaymentStatus.Successful, "payment-key", "dummy_payment_1");
        var repository = new FakePaymentRepository { PaymentByBookingPnr = payment };
        var gateway = new FakePaymentGateway
        {
            RefundResult = new RefundGatewayResult(true, "dummy_refund_1", "Refund succeeded.")
        };
        var service = CreateService(repository, gateway);

        var result = await service.RefundAsync(new RefundPaymentRequest("PNR10", 250m, "refund-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(payment.RefundIdempotencyKey, Is.EqualTo("refund-key"));
        Assert.That(payment.IdempotencyKey, Is.EqualTo("payment-key"));
        Assert.That(payment.RefundStatus, Is.EqualTo(RefundStatus.Successful));
        Assert.That(repository.UpdatedPayments, Has.Count.EqualTo(1));
        Assert.That(gateway.RefundCallCount, Is.EqualTo(1));
        Assert.That(gateway.LastRefundTransactionReference, Is.EqualTo("dummy_payment_1"));
    }

    [Test]
    public async Task RefundAsync_ReturnsExistingRefundWithoutCallingProvider()
    {
        var payment = CreatePayment(PaymentStatus.Refunded, "payment-key", "dummy_payment_1");
        payment.RefundIdempotencyKey = "refund-key";
        var repository = new FakePaymentRepository { PaymentByRefundIdempotencyKey = payment };
        var gateway = new FakePaymentGateway();
        var service = CreateService(repository, gateway);

        var result = await service.RefundAsync(new RefundPaymentRequest("PNR10", 250m, "refund-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(gateway.RefundCallCount, Is.Zero);
        Assert.That(repository.UpdatedPayments, Is.Empty);
    }

    [Test]
    public async Task RefundAsync_DoesNotMarkPaymentRefundedWhenProviderFails()
    {
        var payment = CreatePayment(PaymentStatus.Successful, "payment-key", "dummy_payment_1");
        var repository = new FakePaymentRepository { PaymentByBookingPnr = payment };
        var gateway = new FakePaymentGateway
        {
            RefundResult = new RefundGatewayResult(false, null, "Refund failed.")
        };
        var service = CreateService(repository, gateway);

        var result = await service.RefundAsync(new RefundPaymentRequest("PNR10", 250m, "refund-key"));

        Assert.That(result.PaymentStatus, Is.EqualTo(PaymentStatus.Successful));
        Assert.That(payment.PaymentStatus, Is.EqualTo(PaymentStatus.Successful));
        Assert.That(payment.RefundIdempotencyKey, Is.EqualTo("refund-key"));
        Assert.That(payment.RefundStatus, Is.EqualTo(RefundStatus.Failed));
        Assert.That(repository.UpdatedPayments, Has.Count.EqualTo(1));
    }

    [TestCase(null)]
    [TestCase(PaymentStatus.Failed)]
    [TestCase(PaymentStatus.Refunded)]
    public void RefundAsync_RejectsMissingOrNonRefundablePayment(PaymentStatus? paymentStatus)
    {
        var repository = new FakePaymentRepository
        {
            PaymentByBookingPnr = paymentStatus is null
                ? null
                : CreatePayment(paymentStatus.Value, "payment-key", "dummy_payment_1")
        };
        var service = CreateService(repository);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RefundAsync(new RefundPaymentRequest("PNR10", 250m, "refund-key")));
    }

    [Test]
    public void RefundAsync_RejectsAnAmountDifferentFromThePaymentAmount()
    {
        var payment = CreatePayment(PaymentStatus.Successful, "payment-key", "dummy_payment_1");
        var repository = new FakePaymentRepository { PaymentByBookingPnr = payment };
        var service = CreateService(repository);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.RefundAsync(new RefundPaymentRequest("PNR10", 100m, "refund-key")));
    }

    private static PaymentService.Services.PaymentService CreateService(
        FakePaymentRepository? repository = null,
        FakePaymentGateway? gateway = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DummyRazorpay:SimulateSuccess"] = "true"
            })
            .Build();

        return new PaymentService.Services.PaymentService(
            repository ?? new FakePaymentRepository(),
            gateway ?? new FakePaymentGateway(),
            configuration);
    }

    private static Payment CreatePayment(PaymentStatus status, string idempotencyKey, string transactionReference)
    {
        return new Payment
        {
            BookingPnr = "PNR10",
            Amount = 250m,
            PaymentStatus = status,
            IdempotencyKey = idempotencyKey,
            TransactionReference = transactionReference,
            CreatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        public Payment? PaymentByIdempotencyKey { get; init; }
        public Payment? PaymentByRefundIdempotencyKey { get; init; }
        public Payment? PaymentByBookingPnr { get; init; }
        public List<Payment> AddedPayments { get; } = [];
        public List<Payment> UpdatedPayments { get; } = [];

        public Task<Payment?> GetByIdAsync(int id) => Task.FromResult<Payment?>(null);

        public Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey) =>
            Task.FromResult(PaymentByIdempotencyKey);

        public Task<Payment?> GetByRefundIdempotencyKeyAsync(string refundIdempotencyKey) =>
            Task.FromResult(PaymentByRefundIdempotencyKey);

        public Task<Payment?> GetByBookingPnrAsync(string bookingPnr) =>
            Task.FromResult(PaymentByBookingPnr);

        public Task<Payment?> GetByTransactionReferenceAsync(string transactionReference) =>
            Task.FromResult<Payment?>(null);

        public Task<bool> TryClaimRefundAsync(int paymentId, string refundIdempotencyKey) =>
            Task.FromResult(true);

        public Task AddAsync(Payment payment)
        {
            AddedPayments.Add(payment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Payment payment)
        {
            UpdatedPayments.Add(payment);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public PaymentGatewayResult ProcessResult { get; init; } = new(true, "dummy_payment_default", "Payment succeeded.");
        public RefundGatewayResult RefundResult { get; init; } = new(true, "dummy_refund_default", "Refund succeeded.");
        public Exception? ProcessException { get; init; }
        public int ProcessCallCount { get; private set; }
        public int RefundCallCount { get; private set; }
        public string? LastPaymentReference { get; private set; }
        public string? LastRefundTransactionReference { get; private set; }
        public bool LastSimulateSuccess { get; private set; }

        public Task<PaymentGatewayResult> ProcessPaymentAsync(string paymentReference, decimal amount, bool simulateSuccess)
        {
            ProcessCallCount++;
            LastPaymentReference = paymentReference;
            LastSimulateSuccess = simulateSuccess;

            return ProcessException is null
                ? Task.FromResult(ProcessResult)
                : Task.FromException<PaymentGatewayResult>(ProcessException);
        }

        public Task<RefundGatewayResult> RefundAsync(string providerTransactionReference, decimal amount)
        {
            RefundCallCount++;
            LastRefundTransactionReference = providerTransactionReference;
            return Task.FromResult(RefundResult);
        }
    }
}
