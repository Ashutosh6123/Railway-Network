using DummyRazorpayGateway.DTOs;
using DummyRazorpayGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace DummyRazorpayGateway.Controllers;

[ApiController]
[Route("api/dummy-razorpay")]
public class DummyRazorpayController(IPaymentGatewayService paymentGatewayService) : ControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<ProcessPaymentResponse>> ProcessPayment(ProcessPaymentRequest request)
    {
        var response = await paymentGatewayService.ProcessPaymentAsync(request);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("refund")]
    public async Task<ActionResult<RefundResponse>> Refund(RefundRequest request)
    {
        var response = await paymentGatewayService.RefundAsync(request);

        return Ok(response);
    }
}
