using TrainService.DTOs;
using TrainService.Entities;
using TrainService.Enums;
using TrainService.Repositories;

namespace TrainService.Services;

public class TrainAdminService(
    ITrainRepository trains,
    IStationRepository stations,
    IRouteStopRepository routeStops,
    ICoachRepository coaches,
    ISeatRepository seats,
    IFareRepository fares) : ITrainAdminService
{
    public async Task<TrainDto> CreateTrainAsync(TrainAdminRequest request)
    {
        Required(request.TrainNumber, "Train number");
        Required(request.Name, "Train name");
        if (await trains.GetByTrainNumberAsync(request.TrainNumber) is not null) throw new InvalidOperationException("Train number already exists.");
        var train = new Train { TrainNumber = request.TrainNumber, Name = request.Name };
        await trains.AddAsync(train);
        return new(train.Id, train.TrainNumber, train.Name);
    }

    public async Task<TrainDto> UpdateTrainAsync(int trainId, TrainAdminRequest request)
    {
        Positive(trainId, nameof(trainId));
        Required(request.TrainNumber, "Train number");
        Required(request.Name, "Train name");
        var train = await TrainOrThrow(trainId);
        var matchingTrain = await trains.GetByTrainNumberAsync(request.TrainNumber);
        if (matchingTrain is not null && matchingTrain.Id != trainId) throw new InvalidOperationException("Train number already exists.");
        train.TrainNumber = request.TrainNumber;
        train.Name = request.Name;
        await trains.UpdateAsync(train);
        return new(train.Id, train.TrainNumber, train.Name);
    }

    public async Task DeleteTrainAsync(int trainId) => await trains.DeleteAsync(await TrainOrThrow(trainId));

    public async Task CreateStationAsync(StationAdminRequest request)
    {
        Required(request.Code, "Station code");
        Required(request.Name, "Station name");
        if (await stations.GetByCodeAsync(request.Code) is not null) throw new InvalidOperationException("Station code already exists.");
        await stations.AddAsync(new Station { Code = request.Code, Name = request.Name });
    }

    public async Task UpdateStationAsync(int stationId, StationAdminRequest request)
    {
        Positive(stationId, nameof(stationId));
        Required(request.Code, "Station code");
        Required(request.Name, "Station name");
        var station = await StationOrThrow(stationId);
        var matchingStation = await stations.GetByCodeAsync(request.Code);
        if (matchingStation is not null && matchingStation.Id != stationId) throw new InvalidOperationException("Station code already exists.");
        station.Code = request.Code;
        station.Name = request.Name;
        await stations.UpdateAsync(station);
    }

    public async Task DeleteStationAsync(int stationId) => await stations.DeleteAsync(await StationOrThrow(stationId));

    public async Task AddRouteStopAsync(RouteStopAdminRequest request)
    {
        await ValidateRouteStopAsync(request, null);
        await routeStops.AddAsync(new RouteStop { TrainId = request.TrainId, StationId = request.StationId, StopOrder = request.StopOrder, ArrivalTime = request.ArrivalTime, DepartureTime = request.DepartureTime });
    }

    public async Task UpdateRouteStopAsync(int routeStopId, RouteStopAdminRequest request)
    {
        Positive(routeStopId, nameof(routeStopId));
        var routeStop = await routeStops.GetByIdAsync(routeStopId) ?? throw new InvalidOperationException("Route stop was not found.");
        await ValidateRouteStopAsync(request, routeStopId);
        routeStop.TrainId = request.TrainId; routeStop.StationId = request.StationId; routeStop.StopOrder = request.StopOrder; routeStop.ArrivalTime = request.ArrivalTime; routeStop.DepartureTime = request.DepartureTime;
        await routeStops.UpdateAsync(routeStop);
    }

    public async Task DeleteRouteStopAsync(int routeStopId)
    {
        Positive(routeStopId, nameof(routeStopId));
        var routeStop = await routeStops.GetByIdAsync(routeStopId) ?? throw new InvalidOperationException("Route stop was not found.");
        await routeStops.DeleteAsync(routeStop);
    }

    public async Task<List<RouteStopDto>> GetRouteStopsAsync(int trainId)
    {
        await TrainOrThrow(trainId);
        var result = new List<RouteStopDto>();
        foreach (var routeStop in await routeStops.GetByTrainIdAsync(trainId))
        {
            var station = await StationOrThrow(routeStop.StationId);
            result.Add(new(routeStop.StopOrder, station.Id, station.Code, station.Name, routeStop.ArrivalTime, routeStop.DepartureTime));
        }
        return result;
    }

    public async Task CreateCoachAsync(CoachAdminRequest request)
    {
        Positive(request.TrainId, nameof(request.TrainId));
        Required(request.CoachNumber, "Coach number");
        if (!Enum.IsDefined(request.CoachType)) throw new ArgumentException("Coach type is invalid.");
        await TrainOrThrow(request.TrainId);
        if ((await coaches.GetByTrainIdAsync(request.TrainId)).Any(coach => coach.CoachNumber == request.CoachNumber)) throw new InvalidOperationException("Coach number already exists for this train.");
        await coaches.AddAsync(new Coach { TrainId = request.TrainId, CoachNumber = request.CoachNumber, CoachType = request.CoachType });
    }

    public async Task UpdateCoachAsync(int coachId, CoachAdminRequest request)
    {
        Positive(coachId, nameof(coachId));
        var coach = await coaches.GetByIdAsync(coachId) ?? throw new InvalidOperationException("Coach was not found.");
        await ValidateCoachUpdateAsync(request, coachId);
        coach.TrainId = request.TrainId; coach.CoachNumber = request.CoachNumber; coach.CoachType = request.CoachType;
        await coaches.UpdateAsync(coach);
    }

    public async Task DeleteCoachAsync(int coachId)
    {
        Positive(coachId, nameof(coachId));
        var coach = await coaches.GetByIdAsync(coachId) ?? throw new InvalidOperationException("Coach was not found.");
        await coaches.DeleteAsync(coach);
    }

    public async Task CreateSeatAsync(SeatAdminRequest request)
    {
        await ValidateSeatAsync(request, null);
        await seats.AddAsync(new Seat { CoachId = request.CoachId, SeatNumber = request.SeatNumber });
    }

    public async Task UpdateSeatAsync(int seatId, SeatAdminRequest request)
    {
        Positive(seatId, nameof(seatId));
        var seat = await seats.GetByIdAsync(seatId) ?? throw new InvalidOperationException("Seat was not found.");
        await ValidateSeatAsync(request, seatId);
        seat.CoachId = request.CoachId; seat.SeatNumber = request.SeatNumber;
        await seats.UpdateAsync(seat);
    }

    public async Task DeleteSeatAsync(int seatId)
    {
        Positive(seatId, nameof(seatId));
        var seat = await seats.GetByIdAsync(seatId) ?? throw new InvalidOperationException("Seat was not found.");
        await seats.DeleteAsync(seat);
    }

    public async Task CreateFareAsync(FareAdminRequest request)
    {
        await ValidateFareAsync(request, null);
        await fares.AddAsync(new Fare { TrainId = request.TrainId, FromStationId = request.FromStationId, ToStationId = request.ToStationId, CoachType = request.CoachType, Amount = request.Amount });
    }

    public async Task UpdateFareAsync(int fareId, FareAdminRequest request)
    {
        Positive(fareId, nameof(fareId));
        var fare = await fares.GetByIdAsync(fareId) ?? throw new InvalidOperationException("Fare was not found.");
        await ValidateFareAsync(request, fareId);
        fare.TrainId = request.TrainId; fare.FromStationId = request.FromStationId; fare.ToStationId = request.ToStationId; fare.CoachType = request.CoachType; fare.Amount = request.Amount;
        await fares.UpdateAsync(fare);
    }

    public async Task DeleteFareAsync(int fareId)
    {
        Positive(fareId, nameof(fareId));
        var fare = await fares.GetByIdAsync(fareId) ?? throw new InvalidOperationException("Fare was not found.");
        await fares.DeleteAsync(fare);
    }

    private async Task ValidateRouteStopAsync(RouteStopAdminRequest request, int? ignoredId)
    {
        Positive(request.TrainId, nameof(request.TrainId)); Positive(request.StationId, nameof(request.StationId));
        if (request.StopOrder <= 0) throw new ArgumentException("Stop order must be positive.");
        if (request.ArrivalTime < TimeSpan.Zero || request.DepartureTime < TimeSpan.Zero) throw new ArgumentException("Route stop times must be valid.");
        await TrainOrThrow(request.TrainId); await StationOrThrow(request.StationId);
        if ((await routeStops.GetByTrainIdAsync(request.TrainId)).Any(stop => stop.Id != ignoredId && (stop.StopOrder == request.StopOrder || stop.StationId == request.StationId))) throw new InvalidOperationException("Train route already contains this station or stop order.");
    }

    private async Task ValidateCoachUpdateAsync(CoachAdminRequest request, int? ignoredId)
    {
        Positive(request.TrainId, nameof(request.TrainId)); Required(request.CoachNumber, "Coach number");
        if (!Enum.IsDefined(request.CoachType)) throw new ArgumentException("Coach type is invalid.");
        await TrainOrThrow(request.TrainId);
        if ((await coaches.GetByTrainIdAsync(request.TrainId)).Any(coach => coach.Id != ignoredId && coach.CoachNumber == request.CoachNumber)) throw new InvalidOperationException("Coach number already exists for this train.");
    }

    private async Task ValidateSeatAsync(SeatAdminRequest request, int? ignoredId)
    {
        Positive(request.CoachId, nameof(request.CoachId)); Required(request.SeatNumber, "Seat number");
        if (!int.TryParse(request.SeatNumber, out var seatNumber) || seatNumber <= 0) throw new ArgumentException("Seat number must be positive.");
        if (await coaches.GetByIdAsync(request.CoachId) is null) throw new InvalidOperationException("Coach was not found.");
        if ((await seats.GetByCoachIdAsync(request.CoachId)).Any(seat => seat.Id != ignoredId && seat.SeatNumber == request.SeatNumber)) throw new InvalidOperationException("Seat number already exists for this coach.");
    }

    private async Task ValidateFareAsync(FareAdminRequest request, int? ignoredId)
    {
        Positive(request.TrainId, nameof(request.TrainId)); Positive(request.FromStationId, nameof(request.FromStationId)); Positive(request.ToStationId, nameof(request.ToStationId));
        if (request.FromStationId == request.ToStationId) throw new ArgumentException("Origin and destination stations must be different.");
        if (!Enum.IsDefined(request.CoachType)) throw new ArgumentException("Coach type is invalid.");
        if (request.Amount <= 0) throw new ArgumentException("Fare amount must be greater than zero.");
        await TrainOrThrow(request.TrainId); await StationOrThrow(request.FromStationId); await StationOrThrow(request.ToStationId);
        var stops = await routeStops.GetByTrainIdAsync(request.TrainId);
        var from = stops.FirstOrDefault(stop => stop.StationId == request.FromStationId);
        var to = stops.FirstOrDefault(stop => stop.StationId == request.ToStationId);
        if (from is null || to is null || from.StopOrder >= to.StopOrder) throw new ArgumentException("Fare stations must be an ordered segment on the train route.");
        var existingFare = await fares.GetFareAsync(request.TrainId, request.FromStationId, request.ToStationId, request.CoachType);
        if (existingFare is not null && existingFare.Id != ignoredId) throw new InvalidOperationException("Fare already exists for this route and coach type.");
    }

    private async Task<Train> TrainOrThrow(int id) { Positive(id, nameof(id)); return await trains.GetByIdAsync(id) ?? throw new InvalidOperationException("Train was not found."); }
    private async Task<Station> StationOrThrow(int id) { Positive(id, nameof(id)); return await stations.GetByIdAsync(id) ?? throw new InvalidOperationException("Station was not found."); }
    private static void Positive(int id, string name) { if (id <= 0) throw new ArgumentException("ID must be positive.", name); }
    private static void Required(string value, string name) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required."); }
}
