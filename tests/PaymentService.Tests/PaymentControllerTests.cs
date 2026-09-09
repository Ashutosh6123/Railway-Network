using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PaymentService.Controllers;
using PaymentService.DTOs;
using PaymentService.Enums;
using PaymentService.Services;

namespace PaymentService.Tests;

public class PaymentControllerTests
{
    [Test]
    public async Task ProcessPayment_ReturnsOkAndPassesHeaderIdempotencyKeyToService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.ProcessPayment(
            new ProcessPaymentHttpRequest(1, 1000m),
            "valid-internal-key",
            "payment-key-1");

        var result = response.Result as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.That(paymentService.PaymentRequests, Has.Count.EqualTo(1));
        Assert.That(paymentService.PaymentRequests[0].IdempotencyKey, Is.EqualTo("payment-key-1"));
    }

    [Test]
    public async Task ProcessPayment_ReturnsUnauthorizedWhenInternalKeyIsMissing()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.ProcessPayment(
            new ProcessPaymentHttpRequest(1, 1000m),
            null,
            "payment-key-1");

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(paymentService.PaymentRequests, Is.Empty);
    }

    [Test]
    public async Task ProcessPayment_ReturnsUnauthorizedWhenInternalKeyIsInvalid()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.ProcessPayment(
            new ProcessPaymentHttpRequest(1, 1000m),
            "incorrect-key",
            "payment-key-1");

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(paymentService.PaymentRequests, Is.Empty);
    }

    [Test]
    public void ProcessPayment_RejectsMissingIdempotencyKeyBeforeCallingService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        Assert.ThrowsAsync<ArgumentException>(() => controller.ProcessPayment(
            new ProcessPaymentHttpRequest(1, 1000m),
            "valid-internal-key",
            " "));
        Assert.That(paymentService.PaymentRequests, Is.Empty);
    }

    [Test]
    public async Task Refund_ReturnsOkAndPassesHeaderIdempotencyKeyToService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.Refund(
            new RefundPaymentHttpRequest("dummy_payment_1", 1000m),
            "valid-internal-key",
            "refund-key-1");

        var result = response.Result as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.That(paymentService.RefundRequests, Has.Count.EqualTo(1));
        Assert.That(paymentService.RefundRequests[0].IdempotencyKey, Is.EqualTo("refund-key-1"));
    }

    [Test]
    public async Task Refund_ReturnsUnauthorizedWhenInternalKeyIsMissing()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.Refund(
            new RefundPaymentHttpRequest("dummy_payment_1", 1000m),
            null,
            "refund-key-1");

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(paymentService.RefundRequests, Is.Empty);
    }

    [Test]
    public async Task Refund_ReturnsUnauthorizedWhenInternalKeyIsInvalid()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        var response = await controller.Refund(
            new RefundPaymentHttpRequest("dummy_payment_1", 1000m),
            "incorrect-key",
            "refund-key-1");

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(paymentService.RefundRequests, Is.Empty);
    }

    [Test]
    public void Refund_RejectsMissingIdempotencyKeyBeforeCallingService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        Assert.ThrowsAsync<ArgumentException>(() => controller.Refund(
            new RefundPaymentHttpRequest("dummy_payment_1", 1000m),
            "valid-internal-key",
            null));
        Assert.That(paymentService.RefundRequests, Is.Empty);
    }

    [Test]
    public async Task ProcessPayment_RepeatedRequestsPassTheSameIdempotencyKeyToApplicationService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        await controller.ProcessPayment(new ProcessPaymentHttpRequest(1, 1000m), "valid-internal-key", "payment-key-1");
        await controller.ProcessPayment(new ProcessPaymentHttpRequest(1, 1000m), "valid-internal-key", "payment-key-1");

        Assert.That(paymentService.PaymentRequests, Has.Count.EqualTo(2));
        Assert.That(paymentService.PaymentRequests.Select(request => request.IdempotencyKey),
            Is.All.EqualTo("payment-key-1"));
    }

    [Test]
    public async Task Refund_RepeatedRequestsPassTheSameIdempotencyKeyToApplicationService()
    {
        var paymentService = new FakePaymentService();
        var controller = CreateController(paymentService);

        await controller.Refund(new RefundPaymentHttpRequest("dummy_payment_1", 1000m), "valid-internal-key", "refund-key-1");
        await controller.Refund(new RefundPaymentHttpRequest("dummy_payment_1", 1000m), "valid-internal-key", "refund-key-1");

        Assert.That(paymentService.RefundRequests, Has.Count.EqualTo(2));
        Assert.That(paymentService.RefundRequests.Select(request => request.IdempotencyKey),
            Is.All.EqualTo("refund-key-1"));
    }

    [Test]
    public void HttpRequestDtos_DoNotContainIdempotencyKeyBodyProperties()
    {
        Assert.That(typeof(ProcessPaymentHttpRequest).GetProperty("IdempotencyKey"), Is.Null);
        Assert.That(typeof(RefundPaymentHttpRequest).GetProperty("IdempotencyKey"), Is.Null);
    }

    private static PaymentController CreateController(FakePaymentService paymentService)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["InternalService:ApiKey"] = "valid-internal-key"
            })
            .Build();

        return new PaymentController(paymentService, configuration);
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public List<ProcessPaymentRequest> PaymentRequests { get; } = [];
        public List<RefundPaymentRequest> RefundRequests { get; } = [];

        public Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            PaymentRequests.Add(request);
            return Task.FromResult(new PaymentResultDto(
                request.BookingId,
                request.Amount,
                PaymentStatus.Successful,
                "dummy_payment_1"));
        }

        public Task<PaymentResultDto> RefundAsync(RefundPaymentRequest request)
        {
            RefundRequests.Add(request);
            return Task.FromResult(new PaymentResultDto(
                1,
                request.Amount,
                PaymentStatus.Refunded,
                request.TransactionReference));
        }
    }
}
