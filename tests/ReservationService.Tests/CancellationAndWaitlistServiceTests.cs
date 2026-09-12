using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using ReservationService.Clients;
using ReservationService.Data;
using ReservationService.DTOs;
using ReservationService.Entities;
using ReservationService.Enums;
using ReservationService.Repositories;
using ReservationService.Services;

namespace ReservationService.Tests;

public class CancellationAndWaitlistServiceTests
{
    [Test]
    public async Task CreateBookingAsync_FirstWaitlistedBookingInQueueReturnsPositionOne()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(
            booking,
            availableSeats: []);

        var result = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest());

        Assert.That(result.Status, Is.EqualTo(BookingStatus.Waitlisted));
        Assert.That(result.WaitlistPosition, Is.EqualTo(1));
        Assert.That(fixture.WaitlistRepository.Entries.Last().Position, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateBookingAsync_ReturnsNullWaitlistPositionForConfirmedBooking()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking, availableSeats: [CreateSeat(1)]);

        var result = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest());

        Assert.That(result.Status, Is.EqualTo(BookingStatus.Confirmed));
        Assert.That(result.WaitlistPosition, Is.Null);
    }

    [Test]
    public async Task CreateBookingAsync_ConfirmedTwoPassengerBookingSendsBothPassengerSeats()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking, availableSeats: [CreateSeat(1), CreateSeat(2)]);
        var request = new BookingRequest(
            20,
            1,
            2,
            DateTime.UtcNow.Date.AddDays(1),
            CoachType.Sleeper,
            QuotaType.General,
            [
                new BookingPassengerRequest("Rahul", 30, Gender.Male, "Delhi"),
                new BookingPassengerRequest("Priya", 28, Gender.Female, "Delhi")
            ]);

        await fixture.Service.CreateBookingAsync(booking.UserId, request);

        Assert.That(fixture.MailClient.Data.Single()["passengerSeats"], Is.EqualTo(
            "Rahul — Coach: S1, Seat: 1" + Environment.NewLine +
            "Priya — Coach: S1, Seat: 2"));
    }

    [Test]
    public async Task GetReservationAsync_ReturnsStoredWaitlistPositionForWaitlistedBooking()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var fixture = CreateFixture(booking, entries: [new WaitlistEntry { BookingId = booking.Id, Position = 4 }]);

        var result = await fixture.Service.GetReservationAsync(booking.UserId, booking.Pnr);

        Assert.That(result.WaitlistPosition, Is.EqualTo(4));
    }

    [Test]
    public async Task GetReservationAsync_ReturnsNullWaitlistPositionForConfirmedBooking()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking, entries: [new WaitlistEntry { BookingId = booking.Id, Position = 4 }]);

        var result = await fixture.Service.GetReservationAsync(booking.UserId, booking.Pnr);

        Assert.That(result.WaitlistPosition, Is.Null);
    }

    [Test]
    public async Task CreateBookingAsync_SecondWaitlistedBookingInSameQueueReturnsPositionTwo()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking, availableSeats: []);

        await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest());
        var result = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest());

        Assert.That(result.WaitlistPosition, Is.EqualTo(2));
        Assert.That(fixture.WaitlistRepository.Entries.Select(entry => entry.Position), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task CreateBookingAsync_DifferentQueuesEachStartAtPositionOne()
    {
        var date = DateTime.UtcNow.Date.AddDays(1);
        var booking = CreateBooking(BookingStatus.Confirmed, date);
        var fixture = CreateFixture(booking, availableSeats: []);

        var first = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest(journeyDate: date));
        var differentDate = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest(journeyDate: date.AddDays(1)));
        var differentTrain = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest(trainId: 21, journeyDate: date));
        var differentCoach = await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest(journeyDate: date, coachType: CoachType.AC3Tier));

        Assert.That(first.WaitlistPosition, Is.EqualTo(1));
        Assert.That(differentDate.WaitlistPosition, Is.EqualTo(1));
        Assert.That(differentTrain.WaitlistPosition, Is.EqualTo(1));
        Assert.That(differentCoach.WaitlistPosition, Is.EqualTo(1));
    }

    [Test]
    public async Task CancelBookingAsync_RemovingFirstWaitlistedEntryRenumbersRemainingQueueOnly()
    {
        var first = CreateBooking(BookingStatus.Waitlisted, pnr: "FIRST", id: 1);
        var second = CreateBooking(BookingStatus.Waitlisted, pnr: "SECOND", id: 2);
        var otherQueue = CreateBooking(BookingStatus.Waitlisted, pnr: "OTHER", trainId: 21, id: 3);
        var fixture = CreateFixture(
            first,
            bookings: [first, second, otherQueue],
            entries:
            [
                new WaitlistEntry { BookingId = first.Id, Position = 1 },
                new WaitlistEntry { BookingId = second.Id, Position = 2 },
                new WaitlistEntry { BookingId = otherQueue.Id, Position = 1 }
            ]);

        await fixture.Service.CancelBookingAsync(first.UserId, first.Pnr);

        Assert.That(fixture.WaitlistRepository.Entries.Single(entry => entry.BookingId == second.Id).Position, Is.EqualTo(1));
        Assert.That(fixture.WaitlistRepository.Entries.Single(entry => entry.BookingId == otherQueue.Id).Position, Is.EqualTo(1));
    }

    [Test]
    public async Task CancelBookingAsync_RemovingMiddleWaitlistedEntryRenumbersLaterEntriesAndGetReturnsCurrentPosition()
    {
        var first = CreateBooking(BookingStatus.Waitlisted, pnr: "FIRST", id: 1);
        var middle = CreateBooking(BookingStatus.Waitlisted, pnr: "SECOND", id: 2);
        var last = CreateBooking(BookingStatus.Waitlisted, pnr: "LAST", id: 3);
        var fixture = CreateFixture(
            first,
            bookings: [first, middle, last],
            entries:
            [
                new WaitlistEntry { BookingId = first.Id, Position = 1 },
                new WaitlistEntry { BookingId = middle.Id, Position = 2 },
                new WaitlistEntry { BookingId = last.Id, Position = 3 }
            ]);

        await fixture.Service.CancelBookingAsync(middle.UserId, middle.Pnr);
        var reservation = await fixture.Service.GetReservationAsync(last.UserId, last.Pnr);

        Assert.That(fixture.WaitlistRepository.Entries.Select(entry => entry.Position), Is.EqualTo(new[] { 1, 2 }));
        Assert.That(reservation.WaitlistPosition, Is.EqualTo(2));
    }

    [Test]
    public void CancelBookingAsync_RejectsAnotherUsersBooking()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.CancelBookingAsync(99, booking.Pnr));
        Assert.That(fixture.PaymentClient.RefundRequests, Is.Empty);
    }

    [Test]
    public void CancelBookingAsync_RejectsCancellationAfterDeparture()
    {
        var booking = CreateBooking(BookingStatus.Confirmed, DateTime.UtcNow.Date);
        var fixture = CreateFixture(booking, departureTime: TimeSpan.Zero);

        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Confirmed));
    }

    [Test]
    public async Task CancelBookingAsync_CancelsConfirmedBookingReleasesSeatsAndRefundsByPnr()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var allocations = new List<SeatAllocation> { new() { Id = 1, BookingId = booking.Id, BookingPassengerId = 1, SeatId = 1 } };
        var fixture = CreateFixture(booking, allocations: allocations);

        var result = await fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr);

        Assert.That(result.Status, Is.EqualTo(BookingStatus.Cancelled));
        Assert.That(booking.CancelledAt, Is.Not.Null);
        Assert.That(allocations, Is.Empty);
        Assert.That(fixture.PaymentClient.RefundRequests, Has.Count.EqualTo(1));
        Assert.That(fixture.PaymentClient.RefundRequests[0].BookingPnr, Is.EqualTo(booking.Pnr));
        Assert.That(fixture.PaymentClient.RefundRequests[0].Amount, Is.EqualTo(booking.TotalFare));
        Assert.That(fixture.MailClient.Templates, Is.EqualTo(new[] { "Cancellation" }));
    }

    [Test]
    public async Task CancelBookingAsync_CancelsWaitlistedBookingAndRemovesWaitlistEntry()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var entry = new WaitlistEntry { Id = 1, BookingId = booking.Id, Position = 4, CreatedAt = DateTime.UtcNow };
        var fixture = CreateFixture(booking, entries: [entry]);

        await fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr);

        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
        Assert.That(fixture.WaitlistRepository.Entries, Is.Empty);
        Assert.That(fixture.PaymentClient.RefundRequests[0].BookingPnr, Is.EqualTo(booking.Pnr));
    }

    [Test]
    public void CancelBookingAsync_ReportsRefundFailureExplicitly()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking);
        fixture.PaymentClient.ThrowOnRefund = true;

        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
        Assert.That(fixture.PaymentClient.RefundRequests, Has.Count.EqualTo(1));
    }

    [Test]
    public void CancelBookingAsync_ReportsFailedRefundResponseExplicitly()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking);
        fixture.PaymentClient.ReturnFailedRefund = true;

        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
    }

    [Test]
    public async Task CancelBookingAsync_MailFailureDoesNotUndoCancellation()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking);
        fixture.MailClient.ThrowOnSend = true;

        await fixture.Service.CancelBookingAsync(booking.UserId, booking.Pnr);

        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_DoesNotSkipEarlierBookingThatDoesNotFit()
    {
        var first = CreateBooking(BookingStatus.Waitlisted, pnr: "FIRST");
        var second = CreateBooking(BookingStatus.Waitlisted, pnr: "SECOND");
        var fixture = CreateFixture(
            first,
            bookings: [first, second],
            entries:
            [
                new WaitlistEntry { BookingId = first.Id, Position = 1 },
                new WaitlistEntry { BookingId = second.Id, Position = 2 }
            ],
            passengers:
            [
                CreatePassenger(first.Id, 1), CreatePassenger(first.Id, 2), CreatePassenger(second.Id, 3)
            ],
            availableSeats: [CreateSeat(1)]);

        var promoted = await fixture.Service.PromoteEarliestWaitlistedBookingAsync(first.TrainId, first.JourneyDate, first.CoachType);

        Assert.That(promoted, Is.False);
        Assert.That(first.Status, Is.EqualTo(BookingStatus.Waitlisted));
        Assert.That(second.Status, Is.EqualTo(BookingStatus.Waitlisted));
        Assert.That(fixture.WaitlistRepository.Entries.Select(entry => entry.Position), Is.EqualTo(new[] { 1, 2 }));
        Assert.That(fixture.BookingRepository.RequestedIds, Is.EqualTo(new[] { first.Id }));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_PromotesWholeBookingWithoutAnotherPayment()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var fixture = CreateFixture(
            booking,
            entries: [new WaitlistEntry { BookingId = booking.Id, Position = 1 }],
            passengers: [CreatePassenger(booking.Id, 1), CreatePassenger(booking.Id, 2)],
            availableSeats: [CreateSeat(1), CreateSeat(2)]);

        var promoted = await fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType);

        Assert.That(promoted, Is.True);
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Confirmed));
        Assert.That(fixture.SeatAllocationRepository.Allocations, Has.Count.EqualTo(2));
        Assert.That(fixture.WaitlistRepository.Entries, Is.Empty);
        Assert.That(fixture.PaymentClient.ProcessRequests, Is.Empty);
        Assert.That(fixture.MailClient.Templates, Is.EqualTo(new[] { "WaitlistPromotion" }));
        Assert.That(fixture.MailClient.Data.Single()["passengerSeats"], Is.EqualTo(
            "Passenger 1 — Coach: 1, Seat: 1" + Environment.NewLine +
            "Passenger 2 — Coach: 1, Seat: 2"));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_RenumbersRemainingEntries()
    {
        var first = CreateBooking(BookingStatus.Waitlisted, pnr: "FIRST", id: 1);
        var second = CreateBooking(BookingStatus.Waitlisted, pnr: "SECOND", id: 2);
        var fixture = CreateFixture(
            first,
            bookings: [first, second],
            entries:
            [
                new WaitlistEntry { BookingId = first.Id, Position = 1 },
                new WaitlistEntry { BookingId = second.Id, Position = 2 }
            ],
            passengers: [CreatePassenger(first.Id, 1), CreatePassenger(second.Id, 2)],
            availableSeats: [CreateSeat(1)]);

        await fixture.Service.PromoteEarliestWaitlistedBookingAsync(first.TrainId, first.JourneyDate, first.CoachType);

        Assert.That(fixture.WaitlistRepository.Entries.Single().BookingId, Is.EqualTo(second.Id));
        Assert.That(fixture.WaitlistRepository.Entries.Single().Position, Is.EqualTo(1));
    }

    [Test]
    public async Task CancelBookingAsync_TwoSeatReleasePromotesTwoBookingsAndRenumbersThirdWithoutTrackingConflict()
    {
        var cancelled = CreateBooking(BookingStatus.Confirmed, pnr: "CANCELLED", id: 1);
        var first = CreateBooking(BookingStatus.Waitlisted, pnr: "FIRST", id: 2);
        var second = CreateBooking(BookingStatus.Waitlisted, pnr: "SECOND", id: 3);
        var third = CreateBooking(BookingStatus.Waitlisted, pnr: "THIRD", id: 4);
        var fixture = CreateFixture(
            cancelled,
            bookings: [cancelled, first, second, third],
            entries:
            [
                new WaitlistEntry { BookingId = first.Id, Position = 1 },
                new WaitlistEntry { BookingId = second.Id, Position = 2 },
                new WaitlistEntry { BookingId = third.Id, Position = 3 }
            ],
            passengers:
            [
                CreatePassenger(cancelled.Id, 1),
                CreatePassenger(first.Id, 2),
                CreatePassenger(second.Id, 3),
                CreatePassenger(third.Id, 4)
            ],
            allocations: [new SeatAllocation { BookingId = cancelled.Id, BookingPassengerId = 1, SeatId = 1 }],
            availableSeats: [CreateSeat(1), CreateSeat(2)]);

        await fixture.Service.CancelBookingAsync(cancelled.UserId, cancelled.Pnr);

        Assert.That(first.Status, Is.EqualTo(BookingStatus.Confirmed));
        Assert.That(second.Status, Is.EqualTo(BookingStatus.Confirmed));
        Assert.That(third.Status, Is.EqualTo(BookingStatus.Waitlisted));
        Assert.That(fixture.WaitlistRepository.Entries.Single().BookingId, Is.EqualTo(third.Id));
        Assert.That(fixture.WaitlistRepository.Entries.Single().Position, Is.EqualTo(1));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_UsesPersistedAllocationForPromotionEmail()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var fixture = CreateFixture(
            booking,
            entries: [new WaitlistEntry { BookingId = booking.Id, Position = 1 }],
            passengers: [CreatePassenger(booking.Id, 1)],
            availableSeats: [new AvailableSeat(7, "Ignored", 9, "Ignored")]);

        await fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType);
        var reservation = await fixture.Service.GetReservationAsync(booking.UserId, booking.Pnr);

        Assert.That(fixture.MailClient.Data.Single()["passengerSeats"], Is.EqualTo("Passenger 1 — Coach: 7, Seat: 9"));
        Assert.That(reservation.Passengers.Single().CoachNumber, Is.EqualTo("7"));
        Assert.That(reservation.Passengers.Single().SeatNumber, Is.EqualTo("9"));
    }

    [Test]
    public async Task CreateBookingAsync_WaitlistEmailContainsAssignedQueuePosition()
    {
        var booking = CreateBooking(BookingStatus.Confirmed);
        var fixture = CreateFixture(booking, availableSeats: []);

        await fixture.Service.CreateBookingAsync(booking.UserId, CreateBookingRequest());

        Assert.That(fixture.MailClient.Templates, Is.EqualTo(new[] { "BookingWaitlisted" }));
        Assert.That(fixture.MailClient.Data.Single()["waitlistPosition"], Is.EqualTo("1"));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_DoesNotPromoteAfterDeparture()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted, DateTime.UtcNow.Date);
        var fixture = CreateFixture(
            booking,
            departureTime: TimeSpan.Zero,
            entries: [new WaitlistEntry { BookingId = booking.Id, Position = 1 }],
            availableSeats: [CreateSeat(1)]);

        var promoted = await fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType);

        Assert.That(promoted, Is.False);
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Waitlisted));
    }

    [Test]
    public async Task PromoteEarliestWaitlistedBookingAsync_MailFailureDoesNotUndoPromotion()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var fixture = CreateFixture(
            booking,
            entries: [new WaitlistEntry { BookingId = booking.Id, Position = 1 }],
            availableSeats: [CreateSeat(1)]);
        fixture.MailClient.ThrowOnSend = true;

        var promoted = await fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType);

        Assert.That(promoted, Is.True);
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Confirmed));
    }

    [Test]
    public async Task ConcurrentPromotionAttempts_CreateOneSetOfSeatAllocations()
    {
        var booking = CreateBooking(BookingStatus.Waitlisted);
        var fixture = CreateFixture(
            booking,
            entries: [new WaitlistEntry { BookingId = booking.Id, Position = 1 }],
            availableSeats: [CreateSeat(1)]);

        var results = await Task.WhenAll(
            fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType),
            fixture.Service.PromoteEarliestWaitlistedBookingAsync(booking.TrainId, booking.JourneyDate, booking.CoachType));

        Assert.That(results.Count(result => result), Is.EqualTo(1));
        Assert.That(fixture.SeatAllocationRepository.Allocations, Has.Count.EqualTo(1));
    }

    private static Fixture CreateFixture(
        Booking booking,
        TimeSpan? departureTime = null,
        List<Booking>? bookings = null,
        List<WaitlistEntry>? entries = null,
        List<BookingPassenger>? passengers = null,
        List<SeatAllocation>? allocations = null,
        List<AvailableSeat>? availableSeats = null)
    {
        var context = new TestReservationDbContext();
        var bookingItems = bookings ?? [booking];
        var bookingRepository = new FakeBookingRepository(bookingItems);
        var passengerRepository = new FakePassengerRepository(passengers ?? [CreatePassenger(booking.Id, 1)]);
        var seatRepository = new FakeSeatAllocationRepository(allocations ?? []);
        var waitlistRepository = new FakeWaitlistRepository(entries ?? [], bookingItems);
        var userClient = new FakeUserClient();
        var trainClient = new FakeTrainClient(departureTime ?? TimeSpan.FromHours(6));
        var paymentClient = new FakePaymentClient();
        var mailClient = new FakeMailClient();
        var availabilityService = new FakeAvailabilityService(availableSeats ?? [CreateSeat(1)], seatRepository);
        var service = new BookingService(
            context,
            bookingRepository,
            passengerRepository,
            seatRepository,
            waitlistRepository,
            userClient,
            trainClient,
            paymentClient,
            mailClient,
            availabilityService,
            NullLogger<BookingService>.Instance);

        return new Fixture(service, bookingRepository, seatRepository, waitlistRepository, paymentClient, mailClient);
    }

    private static Booking CreateBooking(
        BookingStatus status,
        DateTime? journeyDate = null,
        string pnr = "PNR123",
        int trainId = 20,
        CoachType coachType = CoachType.Sleeper,
        int? id = null) => new()
    {
        Id = id ?? (pnr == "SECOND" ? 2 : 1),
        Pnr = pnr,
        UserId = 10,
        TrainId = trainId,
        FromStationId = 1,
        ToStationId = 2,
        JourneyDate = journeyDate ?? DateTime.UtcNow.Date.AddDays(1),
        CoachType = coachType,
        Quota = QuotaType.General,
        Status = status,
        TotalFare = 450m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static BookingPassenger CreatePassenger(int bookingId, int id) => new()
    {
        Id = id,
        BookingId = bookingId,
        Name = $"Passenger {id}",
        Age = 30,
        Gender = Gender.Male,
        Address = "Delhi"
    };

    private static AvailableSeat CreateSeat(int seatId) => new(1, "S1", seatId, seatId.ToString());

    private static BookingRequest CreateBookingRequest(
        int trainId = 20,
        DateTime? journeyDate = null,
        CoachType coachType = CoachType.Sleeper) => new(
        trainId,
        1,
        2,
        journeyDate ?? DateTime.UtcNow.Date.AddDays(1),
        coachType,
        QuotaType.General,
        [new BookingPassengerRequest("Passenger", 30, Gender.Male, "Delhi")]);

    private record Fixture(
        BookingService Service,
        FakeBookingRepository BookingRepository,
        FakeSeatAllocationRepository SeatAllocationRepository,
        FakeWaitlistRepository WaitlistRepository,
        FakePaymentClient PaymentClient,
        FakeMailClient MailClient);

    private sealed class TestReservationDbContext : ReservationDbContext
    {
        private readonly SemaphoreSlim transactionLock = new(1, 1);

        public TestReservationDbContext() : base(new DbContextOptionsBuilder<ReservationDbContext>().Options) { }

        public override async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await transactionLock.WaitAsync();
            return new TestTransaction(transactionLock);
        }
    }

    private sealed class TestTransaction(SemaphoreSlim transactionLock) : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();
        public void Commit() { }
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Rollback() { }
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() => transactionLock.Release();
        public ValueTask DisposeAsync()
        {
            transactionLock.Release();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeBookingRepository(List<Booking> bookings) : IBookingRepository
    {
        public List<int> RequestedIds { get; } = [];
        public Task<Booking?> GetByIdAsync(int id)
        {
            RequestedIds.Add(id);
            return Task.FromResult(bookings.FirstOrDefault(booking => booking.Id == id));
        }
        public Task<Booking?> GetByPnrAsync(string pnr) => Task.FromResult(bookings.FirstOrDefault(booking => booking.Pnr == pnr));
        public Task AddAsync(Booking booking)
        {
            if (booking.Id == 0) booking.Id = bookings.Select(item => item.Id).DefaultIfEmpty().Max() + 1;
            bookings.Add(booking);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(Booking booking) => Task.CompletedTask;
    }

    private sealed class FakePassengerRepository(List<BookingPassenger> passengers) : IBookingPassengerRepository
    {
        public Task<List<BookingPassenger>> GetByBookingIdAsync(int bookingId) =>
            Task.FromResult(passengers.Where(passenger => passenger.BookingId == bookingId).ToList());
        public Task AddRangeAsync(List<BookingPassenger> items) { passengers.AddRange(items); return Task.CompletedTask; }
    }

    private sealed class FakeSeatAllocationRepository(List<SeatAllocation> allocations) : ISeatAllocationRepository
    {
        public List<SeatAllocation> Allocations => allocations;
        public Task<List<SeatAllocation>> GetActiveByTrainAndJourneyDateAsync(int trainId, DateTime journeyDate) => Task.FromResult(allocations.ToList());
        public Task<List<SeatAllocation>> GetByBookingIdAsync(int bookingId) => Task.FromResult(allocations.Where(item => item.BookingId == bookingId).ToList());
        public Task AddRangeAsync(List<SeatAllocation> items) { allocations.AddRange(items); return Task.CompletedTask; }
        public Task RemoveRangeAsync(List<SeatAllocation> items) { foreach (var item in items) allocations.Remove(item); return Task.CompletedTask; }
    }

    private sealed class FakeWaitlistRepository(List<WaitlistEntry> entries, List<Booking> bookings) : IWaitlistRepository
    {
        public List<WaitlistEntry> Entries => entries;
        public Task<WaitlistEntry?> GetByBookingIdAsync(int bookingId) => Task.FromResult(entries.FirstOrDefault(entry => entry.BookingId == bookingId));
        public Task<List<WaitlistEntry>> GetOrderedByQueueAsync(int trainId, DateTime journeyDate, CoachType coachType) =>
            Task.FromResult(GetQueueEntries(trainId, journeyDate, coachType).OrderBy(entry => entry.Position).ToList());
        public Task<int> GetNextPositionAsync(int trainId, DateTime journeyDate, CoachType coachType) =>
            Task.FromResult(GetQueueEntries(trainId, journeyDate, coachType).Select(entry => entry.Position).DefaultIfEmpty().Max() + 1);
        public Task AddAsync(WaitlistEntry entry) { entries.Add(entry); return Task.CompletedTask; }
        public Task RemoveAndRenumberAsync(WaitlistEntry entry, int trainId, DateTime journeyDate, CoachType coachType)
        {
            entries.Remove(entry);
            var remainingEntries = GetQueueEntries(trainId, journeyDate, coachType)
                .OrderBy(item => item.Position)
                .ThenBy(item => item.CreatedAt)
                .ThenBy(item => item.Id)
                .ToList();
            for (var index = 0; index < remainingEntries.Count; index++) remainingEntries[index].Position = index + 1;
            return Task.CompletedTask;
        }

        private IEnumerable<WaitlistEntry> GetQueueEntries(int trainId, DateTime journeyDate, CoachType coachType) =>
            entries.Where(entry => bookings.Any(booking =>
                booking.Id == entry.BookingId &&
                booking.TrainId == trainId &&
                booking.JourneyDate == journeyDate.Date &&
                booking.CoachType == coachType));
    }

    private sealed class FakeUserClient : IUserClient
    {
        public Task<UserClientDto?> GetUserAsync(int userId) => Task.FromResult<UserClientDto?>(new(userId, "Rahul", "rahul@example.com", "9876543210", "Passenger"));
    }

    private sealed class FakeTrainClient(TimeSpan departureTime) : ITrainClient
    {
        public Task<TrainClientDto> GetTrainAsync(int trainId) => Task.FromResult(new TrainClientDto(trainId, "12001", "Express"));
        public Task<List<RouteStopClientDto>> GetRouteAsync(int trainId) => Task.FromResult(new List<RouteStopClientDto>
        {
            new(1, 1, "DEL", "Delhi", TimeSpan.Zero, departureTime),
            new(2, 2, "BPL", "Bhopal", TimeSpan.Zero, TimeSpan.Zero)
        });
        public Task<FareClientDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) => Task.FromResult(new FareClientDto(trainId, fromStationId, toStationId, coachType, 450m));
        public Task<List<SeatInventoryClientDto>> GetSeatInventoryAsync(int trainId, CoachType coachType) => Task.FromResult(new List<SeatInventoryClientDto>());
    }

    private sealed class FakePaymentClient : IPaymentClient
    {
        public List<(string BookingPnr, decimal Amount, string IdempotencyKey)> ProcessRequests { get; } = [];
        public List<(string BookingPnr, decimal Amount, string IdempotencyKey)> RefundRequests { get; } = [];
        public bool ThrowOnRefund { get; set; }
        public bool ReturnFailedRefund { get; set; }
        public Task<PaymentClientResult> ProcessPaymentAsync(string bookingPnr, decimal amount, string idempotencyKey)
        {
            ProcessRequests.Add((bookingPnr, amount, idempotencyKey));
            return Task.FromResult(new PaymentClientResult(bookingPnr, amount, 1, "payment_1", null));
        }
        public Task<PaymentClientResult> RefundAsync(string bookingPnr, decimal amount, string idempotencyKey)
        {
            RefundRequests.Add((bookingPnr, amount, idempotencyKey));
            if (ThrowOnRefund) throw new HttpRequestException("Refund failed.");
            return Task.FromResult(new PaymentClientResult(bookingPnr, amount, 3, "payment_1", ReturnFailedRefund ? 2 : 1));
        }
    }

    private sealed class FakeMailClient : IMailClient
    {
        public List<string> Templates { get; } = [];
        public List<Dictionary<string, string>> Data { get; } = [];
        public bool ThrowOnSend { get; set; }
        public Task<MailClientResult> SendAsync(string to, string template, Dictionary<string, string> data)
        {
            Templates.Add(template);
            Data.Add(data);
            if (ThrowOnSend) throw new HttpRequestException("Mail failed.");
            return Task.FromResult(new MailClientResult(true, "Sent"));
        }
    }

    private sealed class FakeAvailabilityService(
        List<AvailableSeat> seats,
        FakeSeatAllocationRepository seatAllocationRepository) : IAvailabilityService
    {
        public Task<AvailabilityResult> GetAvailabilityAsync(int trainId, int fromStationId, int toStationId, DateTime journeyDate, CoachType coachType) =>
            Task.FromResult(new AvailabilityResult(
                1,
                2,
                seats.Where(seat => seatAllocationRepository.Allocations.All(allocation => allocation.SeatId != seat.SeatId)).ToList()));
    }
}
