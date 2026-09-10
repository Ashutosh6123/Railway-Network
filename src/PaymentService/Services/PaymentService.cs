using PaymentService.Clients;
using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Enums;
using PaymentService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    IConfiguration configuration) : IPaymentService
{
    public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        ValidatePaymentRequest(request);

        var existingPayment = await paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);

        if (existingPayment is not null)
        {
            return ToResultDto(existingPayment);
        }

        var payment = new Payment
        {
            BookingPnr = request.BookingPnr,
            Amount = request.Amount,
            PaymentStatus = PaymentStatus.Pending,
            IdempotencyKey = request.IdempotencyKey,
            TransactionReference = string.Empty,
            RefundIdempotencyKey = null,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Persist Pending first so the unique idempotency key is claimed before the provider call.
            await paymentRepository.AddAsync(payment);
        }
        catch (DbUpdateException)
        {
            var concurrentPayment = await paymentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);

            if (concurrentPayment is not null)
            {
                return ToResultDto(concurrentPayment);
            }

            throw;
        }

        var simulateSuccess = configuration.GetValue("DummyRazorpay:SimulateSuccess", true);
        var gatewayResult = await paymentGateway.ProcessPaymentAsync(
            request.BookingPnr,
            request.Amount,
            simulateSuccess);

        if (gatewayResult.Success)
        {
            if (string.IsNullOrWhiteSpace(gatewayResult.ProviderTransactionReference))
            {
                throw new InvalidOperationException("Dummy Razorpay did not return a transaction reference.");
            }

            payment.PaymentStatus = PaymentStatus.Successful;
            payment.TransactionReference = gatewayResult.ProviderTransactionReference;
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.Failed;
            payment.TransactionReference = string.Empty;
        }

        await paymentRepository.UpdateAsync(payment);

        return ToResultDto(payment);
    }

    public async Task<PaymentResultDto> RefundAsync(RefundPaymentRequest request)
    {
        ValidateRefundRequest(request);

        var existingRefund = await paymentRepository.GetByRefundIdempotencyKeyAsync(request.IdempotencyKey);

        if (existingRefund is not null)
        {
            return ToResultDto(existingRefund);
        }

        var payment = await paymentRepository.GetByBookingPnrAsync(request.BookingPnr);

        if (payment is null)
        {
            throw new InvalidOperationException("Payment was not found.");
        }

        if (payment.PaymentStatus != PaymentStatus.Successful)
        {
            throw new InvalidOperationException("Only successful payments can be refunded.");
        }

        if (request.Amount != payment.Amount)
        {
            throw new ArgumentException("Refund amount must equal the original payment amount.");
        }

        var refundClaimed = await paymentRepository.TryClaimRefundAsync(payment.Id, request.IdempotencyKey);

        if (!refundClaimed)
        {
            var concurrentRefund = await paymentRepository.GetByRefundIdempotencyKeyAsync(request.IdempotencyKey);

            if (concurrentRefund is not null)
            {
                return ToResultDto(concurrentRefund);
            }

            var currentPayment = await paymentRepository.GetByBookingPnrAsync(request.BookingPnr);

            if (currentPayment?.RefundIdempotencyKey == request.IdempotencyKey)
            {
                return ToResultDto(currentPayment);
            }

            throw new InvalidOperationException("Payment is already being refunded or has been refunded.");
        }

        payment.RefundIdempotencyKey = request.IdempotencyKey;
        payment.RefundStatus = RefundStatus.Pending;

        // Reservation Service sends only the booking PNR. The provider receives the stored reference.
        var gatewayResult = await paymentGateway.RefundAsync(payment.TransactionReference, request.Amount);

        if (gatewayResult.Success)
        {
            payment.RefundStatus = RefundStatus.Successful;
            payment.PaymentStatus = PaymentStatus.Refunded;
        }
        else
        {
            payment.RefundStatus = RefundStatus.Failed;
        }

        await paymentRepository.UpdateAsync(payment);

        return ToResultDto(payment);
    }

    private static void ValidatePaymentRequest(ProcessPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BookingPnr))
        {
            throw new ArgumentException("Booking PNR is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.");
        }
    }

    private static void ValidateRefundRequest(RefundPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BookingPnr))
        {
            throw new ArgumentException("Booking PNR is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Refund amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.");
        }
    }

    private static PaymentResultDto ToResultDto(Payment payment)
    {
        return new PaymentResultDto(
            payment.BookingPnr,
            payment.Amount,
            payment.PaymentStatus,
            string.IsNullOrEmpty(payment.TransactionReference) ? null : payment.TransactionReference,
            payment.RefundStatus);
    }
}
