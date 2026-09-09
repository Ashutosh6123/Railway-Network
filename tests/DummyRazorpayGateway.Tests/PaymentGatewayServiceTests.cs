using DummyRazorpayGateway.DTOs;
using DummyRazorpayGateway.Services;

namespace DummyRazorpayGateway.Tests;

public class PaymentGatewayServiceTests
{
    private readonly PaymentGatewayService _service = new();

    [Test]
    public async Task ProcessPaymentAsync_ReturnsProviderReferenceForSuccessfulPayment()
    {
        var response = await _service.ProcessPaymentAsync(new(
            "payment-reference-1",
            250m,
            true));

        Assert.That(response.Success, Is.True);
        Assert.That(response.ProviderTransactionReference, Is.Not.Null.And.StartWith("dummy_payment_"));
    }

    [Test]
    public async Task ProcessPaymentAsync_ReturnsFailureWithoutProviderReference()
    {
        var response = await _service.ProcessPaymentAsync(new(
            "payment-reference-2",
            250m,
            false));

        Assert.That(response.Success, Is.False);
        Assert.That(response.ProviderTransactionReference, Is.Null);
    }

    [Test]
    public void ProcessPaymentAsync_ThrowsForEmptyPaymentReference()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(new("", 250m, true)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ProcessPaymentAsync_ThrowsForNonPositiveAmount(decimal amount)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(new("payment-reference", amount, true)));
    }

    [Test]
    public async Task RefundAsync_ReturnsProviderReferenceForSuccessfulRefund()
    {
        var response = await _service.RefundAsync(new(
            "dummy_payment_reference",
            250m));

        Assert.That(response.Success, Is.True);
        Assert.That(response.ProviderRefundReference, Is.Not.Null.And.StartWith("dummy_refund_"));
    }

    [Test]
    public void RefundAsync_ThrowsForEmptyProviderTransactionReference()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.RefundAsync(new("", 250m)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void RefundAsync_ThrowsForNonPositiveAmount(decimal amount)
    {
        Assert.ThrowsAsync<ArgumentException>(() => _service.RefundAsync(new("dummy_payment_reference", amount)));
    }
}
