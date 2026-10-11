import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Seat } from '../../features/admin/admin';

export interface Train {
  id: number;
  trainNumber: string;
  name: string;
}

export interface TrainAdminRequest {
  trainNumber: string;
  name: string;
}

export interface Station {
  id: number;
  code: string;
  name: string;
}

export interface StationAdminRequest {
  code: string;
  name: string;
}

export interface RouteStop {
  id: number;
  stopOrder: number;
  stationId: number;
  stationCode: string;
  stationName: string;
  arrivalTime: string;
  departureTime: string;
}

export interface RouteStopRequest {
  stationId: number;
  stopOrder: number;
  arrivalTime: string;
  departureTime: string;
}

export interface RouteStopAdminRequest {
  trainId: number;
  stationId: number;
  stopOrder: number;
  arrivalTime: string;
  departureTime: string;
}

export interface Coach {
  id: number;
  trainId: number;
  coachNumber: string;
  coachType: number;
}

export interface CoachRequest {
  coachNumber: string;
  coachType: number;
}

export interface CoachAdminRequest {
  trainId: number;
  coachNumber: string;
  coachType: number;
}

export interface SeatRequest {
  seatNumber: string;
}

export interface Fare {
  id: number;
  trainId: number;
  fromStationId: number;
  toStationId: number;
  coachType: number;
  amount: number;
}

export interface FareAdminRequest {
  trainId: number;
  fromStationId: number;
  toStationId: number;
  coachType: number;
  amount: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/admin`;

  getTrains(): Observable<Train[]> {
    return this.http.get<Train[]>(`${environment.apiUrl}/api/trains`);
  }

  createTrain(request: TrainAdminRequest): Observable<Train> {
    return this.http.post<Train>(
      `${this.apiUrl}/trains`,
      request
    );
  }

  updateTrain(
    trainId: number,
    request: TrainAdminRequest
  ): Observable<Train> {
    return this.http.put<Train>(
      `${this.apiUrl}/trains/${trainId}`,
      request
    );
  }

  deleteTrain(trainId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/trains/${trainId}`
    );
  }

  getStations(): Observable<Station[]> {
      return this.http.get<Station[]>(
          `${environment.apiUrl}/api/stations`
      );
  }

  createStation(request: StationAdminRequest): Observable<Station> {
      return this.http.post<Station>(
          `${this.apiUrl}/stations`,
          request
      );
  }
  updateStation(
      stationId: number,
      request: StationAdminRequest
      ): Observable<Station> {
      return this.http.put<Station>(
          `${this.apiUrl}/stations/${stationId}`,
          request
      );
  }
  deleteStation(stationId: number): Observable<void> {
      return this.http.delete<void>(
          `${this.apiUrl}/stations/${stationId}`
      );
  }

  getRouteStops(trainId: number): Observable<RouteStop[]> {
      return this.http.get<RouteStop[]>(
          `${this.apiUrl}/trains/${trainId}/route-stops`
      );
  }
  createRouteStop(
      trainId: number,
      request: RouteStopRequest
      ): Observable<RouteStop> {
      return this.http.post<RouteStop>(
          `${this.apiUrl}/trains/${trainId}/route-stops`,
          request
      );
  }
  updateRouteStop(
      routeStopId: number,
      request: RouteStopAdminRequest
      ): Observable<RouteStop> {
      return this.http.put<RouteStop>(
          `${this.apiUrl}/route-stops/${routeStopId}`,
          request
      );
  }
  deleteRouteStop(routeStopId: number): Observable<void> {
      return this.http.delete<void>(
          `${this.apiUrl}/route-stops/${routeStopId}`
      );
  }

  getCoaches(trainId: number) {
      return this.http.get<Coach[]>(
          `${this.apiUrl}/trains/${trainId}/coaches`
      );
  }
  createCoach(trainId: number, request: CoachRequest) {
  return this.http.post(
      `${this.apiUrl}/trains/${trainId}/coaches`,
      request
  );
  }

  updateCoach(coachId: number, request: CoachAdminRequest) {
  return this.http.put(
      `${this.apiUrl}/coaches/${coachId}`,
      request
  );
  }

  deleteCoach(coachId: number) {
  return this.http.delete(
      `${this.apiUrl}/coaches/${coachId}`
  );
  }

  getSeats(coachId: number) {
      return this.http.get<Seat[]>(
        `${this.apiUrl}/coaches/${coachId}/seats`
      );
  }
  createSeat(coachId: number, request: SeatRequest) {
    return this.http.post(
      `${this.apiUrl}/coaches/${coachId}/seats`,
      request
    );
  }
  updateSeat(seatId: number, request: SeatRequest) {
    return this.http.put(
      `${this.apiUrl}/seats/${seatId}`,
      request
    );
  }
  deleteSeat(seatId: number) {
    return this.http.delete(
      `${this.apiUrl}/seats/${seatId}`
    );
  }

  getFares(trainId: number): Observable<Fare[]> {
    return this.http.get<Fare[]>(
      `${this.apiUrl}/trains/${trainId}/fares`
    );
  }

  createFare(request: FareAdminRequest): Observable<Fare> {
    return this.http.post<Fare>(
      `${this.apiUrl}/fares`,
      request
    );
  }

  updateFare(
    fareId: number,
    request: FareAdminRequest
  ): Observable<Fare> {
    return this.http.put<Fare>(
      `${this.apiUrl}/fares/${fareId}`,
      request
    );
  }

  deleteFare(fareId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/fares/${fareId}`
    );
  }
}