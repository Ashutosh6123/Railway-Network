using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ReservationService.Clients;
using ReservationService.Enums;

namespace ReservationService.Tests;

public class ReservationHttpClientTests
{
    [Test]
    public async Task UserClient_ReturnsUserAndSendsInternalServiceKey()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new UserClientDto(1, "Rahul", "rahul@example.com", "9876543210", "Passenger")));
        var client = new UserClient(CreateHttpClient(handler), CreateConfiguration());

        var user = await client.GetUserAsync(1);

        Assert.That(user?.Email, Is.EqualTo("rahul@example.com"));
        Assert.That(handler.Requests[0].Headers["X-Internal-Service-Key"], Is.EqualTo("internal-key"));
    }

    [Test]
    public async Task UserClient_ReturnsNullWhenUserIsNotFound()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new UserClient(CreateHttpClient(handler), CreateConfiguration());

        var user = await client.GetUserAsync(999);

        Assert.That(user, Is.Null);
    }

    [Test]
    public void UserClient_ThrowsWhenUserServiceReturnsAnError()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var client = new UserClient(CreateHttpClient(handler), CreateConfiguration());

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetUserAsync(1));
    }

    [Test]
    public async Task TrainClient_ReturnsRouteFareAndSeatInventory()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/route"))
            {
                return JsonResponse(new List<RouteStopClientDto>
                {
                    new(1, 1, "DEL", "Delhi", TimeSpan.Zero, TimeSpan.FromHours(6))
                });
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/fare"))
            {
                return JsonResponse(new FareClientDto(1, 1, 2, CoachType.Sleeper, 450m));
            }

            return JsonResponse(new List<SeatInventoryClientDto>
            {
                new(2, "S1", 5, "1")
            });
        });
        var client = new TrainClient(CreateHttpClient(handler), CreateConfiguration());

        var route = await client.GetRouteAsync(1);
        var fare = await client.GetFareAsync(1, 1, 2, CoachType.Sleeper);
        var seats = await client.GetSeatInventoryAsync(1, CoachType.Sleeper);

        Assert.That(route[0].StopOrder, Is.EqualTo(1));
        Assert.That(fare.Amount, Is.EqualTo(450m));
        Assert.That(seats[0].SeatNumber, Is.EqualTo("1"));
        Assert.That(handler.Requests.Last().Headers["X-Internal-Service-Key"], Is.EqualTo("internal-key"));
    }

    [Test]
    public void TrainClient_ThrowsWhenTrainServiceReturnsAnError()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new TrainClient(CreateHttpClient(handler), CreateConfiguration());

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetRouteAsync(1));
    }

    [Test]
    public async Task PaymentClient_ProcessesPaymentAndSendsRequiredHeaders()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new PaymentClientResult("PNR123", 450m, 1, "payment_1", null)));
        var client = new PaymentClient(CreateHttpClient(handler), CreateConfiguration());

        var result = await client.ProcessPaymentAsync("PNR123", 450m, "payment-key-1");

        Assert.That(result.TransactionReference, Is.EqualTo("payment_1"));
        Assert.That(handler.Requests[0].Headers["X-Internal-Service-Key"], Is.EqualTo("internal-key"));
        Assert.That(handler.Requests[0].Headers["Idempotency-Key"], Is.EqualTo("payment-key-1"));
        Assert.That(handler.Requests[0].Body, Does.Not.Contain("IdempotencyKey"));
    }

    [Test]
    public async Task PaymentClient_ReturnsFailedPaymentResult()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new PaymentClientResult("PNR123", 450m, 2, null, null)));
        var client = new PaymentClient(CreateHttpClient(handler), CreateConfiguration());

        var result = await client.ProcessPaymentAsync("PNR123", 450m, "payment-key-1");

        Assert.That(result.PaymentStatus, Is.EqualTo(2));
        Assert.That(result.TransactionReference, Is.Null);
    }

    [Test]
    public async Task PaymentClient_ProcessesRefund()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new PaymentClientResult("PNR123", 450m, 3, "payment_1", 1)));
        var client = new PaymentClient(CreateHttpClient(handler), CreateConfiguration());

        var result = await client.RefundAsync("PNR123", 450m, "refund-key-1");

        Assert.That(result.RefundStatus, Is.EqualTo(1));
        Assert.That(handler.Requests[0].Headers["Idempotency-Key"], Is.EqualTo("refund-key-1"));
        Assert.That(handler.Requests[0].Body, Does.Contain("PNR123"));
        Assert.That(handler.Requests[0].Body, Does.Not.Contain("TransactionReference"));
    }

    [Test]
    public async Task MailClient_SendsNotificationWithInternalServiceKey()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new MailClientResult(true, "Notification sent successfully.")));
        var client = new MailClient(CreateHttpClient(handler), CreateConfiguration());

        var result = await client.SendAsync("rahul@example.com", "BookingConfirmed", new Dictionary<string, string> { ["pnr"] = "PNR123" });

        Assert.That(result.Success, Is.True);
        Assert.That(handler.Requests[0].Headers["X-Internal-Service-Key"], Is.EqualTo("internal-key"));
    }

    [Test]
    public void MailClient_ThrowsWhenMailServiceReturnsAnError()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var client = new MailClient(CreateHttpClient(handler), CreateConfiguration());

        Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync("rahul@example.com", "BookingConfirmed", new Dictionary<string, string>()));
    }

    private static HttpClient CreateHttpClient(FakeHttpMessageHandler handler) => new(handler)
    {
        BaseAddress = new Uri("http://localhost/")
    };

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["InternalService:ApiKey"] = "internal-key"
        })
        .Build();

    private static HttpResponseMessage JsonResponse<T>(T value) => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(value)
    };

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<RequestInfo> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value));
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RequestInfo(headers, body));
            return responseFactory(request);
        }
    }

    private record RequestInfo(Dictionary<string, string> Headers, string Body);
}
