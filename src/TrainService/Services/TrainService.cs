using TrainService.DTOs;
using TrainService.Enums;
using TrainService.Repositories;

namespace TrainService.Services;

public class TrainService(
    ITrainRepository trainRepository,
    IRouteStopRepository routeStopRepository,
    IStationRepository stationRepository,
    IFareRepository fareRepository,
    ICoachRepository coachRepository,
    ISeatRepository seatRepository) : ITrainService
{
    public async Task<List<TrainDto>> SearchTrainsAsync(int fromStationId, int toStationId)
    {
        ValidateStationPair(fromStationId, toStationId);

        var matchingTrains = new List<TrainDto>();
        var trains = await trainRepository.GetAllAsync();

        foreach (var train in trains)
        {
            var routeStops = await routeStopRepository.GetByTrainIdAsync(train.Id);
            var origin = routeStops.FirstOrDefault(stop => stop.StationId == fromStationId);
            var destination = routeStops.FirstOrDefault(stop => stop.StationId == toStationId);

            if (origin is not null && destination is not null && origin.StopOrder < destination.StopOrder)
            {
                matchingTrains.Add(MapTrain(train));
            }
        }

        return matchingTrains;
    }

    public async Task<TrainDto> GetTrainAsync(int trainId)
    {
        ValidatePositiveId(trainId, nameof(trainId));

        var train = await trainRepository.GetByIdAsync(trainId);
        if (train is null)
        {
            throw new InvalidOperationException("Train was not found.");
        }

        return MapTrain(train);
    }

    public async Task<List<RouteStopDto>> GetRouteAsync(int trainId)
    {
        await GetTrainAsync(trainId);

        var routeStops = await routeStopRepository.GetByTrainIdAsync(trainId);
        var route = new List<RouteStopDto>();

        foreach (var routeStop in routeStops)
        {
            var station = await stationRepository.GetByIdAsync(routeStop.StationId);
            if (station is null)
            {
                throw new InvalidOperationException("Station was not found.");
            }

            route.Add(new RouteStopDto(
                routeStop.Id,
                routeStop.StopOrder,
                station.Id,
                station.Code,
                station.Name,
                routeStop.ArrivalTime,
                routeStop.DepartureTime));
        }

        return route;
    }

    public async Task<FareDto> GetFareAsync(
        int trainId,
        int fromStationId,
        int toStationId,
        CoachType coachType)
    {
        ValidatePositiveId(trainId, nameof(trainId));
        ValidateStationPair(fromStationId, toStationId);

        var fare = await fareRepository.GetFareAsync(
            trainId,
            fromStationId,
            toStationId,
            coachType);

        if (fare is null)
        {
            throw new InvalidOperationException("Fare was not found.");
        }

        return new FareDto(
            fare.TrainId,
            fare.FromStationId,
            fare.ToStationId,
            fare.CoachType,
            fare.Amount);
    }

    public async Task<List<SeatInventoryDto>> GetSeatInventoryAsync(int trainId, CoachType coachType)
    {
        await GetTrainAsync(trainId);

        var coaches = await coachRepository.GetByTrainIdAsync(trainId);
        var seats = new List<SeatInventoryDto>();

        foreach (var coach in coaches.Where(coach => coach.CoachType == coachType))
        {
            var coachSeats = await seatRepository.GetByCoachIdAsync(coach.Id);

            foreach (var seat in coachSeats)
            {
                seats.Add(new SeatInventoryDto(
                    coach.Id,
                    coach.CoachNumber,
                    seat.Id,
                    seat.SeatNumber));
            }
        }

        return seats;
    }

    private static TrainDto MapTrain(Entities.Train train) =>
        new(train.Id, train.TrainNumber, train.Name);

    private static void ValidateStationPair(int fromStationId, int toStationId)
    {
        ValidatePositiveId(fromStationId, nameof(fromStationId));
        ValidatePositiveId(toStationId, nameof(toStationId));

        if (fromStationId == toStationId)
        {
            throw new ArgumentException("Origin and destination stations must be different.");
        }
    }

    private static void ValidatePositiveId(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentException("ID must be positive.", parameterName);
        }
    }
}
