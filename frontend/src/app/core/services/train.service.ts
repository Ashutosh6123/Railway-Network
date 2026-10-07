import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Train {
  id: number;
  trainNumber: string;
  name: string;
}

export interface Fare {
  trainId: number;
  fromStationId: number;
  toStationId: number;
  coachType: number;
  amount: number;
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

@Injectable({
  providedIn: 'root'
})
export class TrainService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/trains`;

  searchTrains(
    fromStationId: number,
    toStationId: number
  ): Observable<Train[]> {
    const params = new HttpParams()
      .set('fromStationId', fromStationId)
      .set('toStationId', toStationId);

    return this.http.get<Train[]>(
      `${this.apiUrl}/search`,
      { params }
    );
  }

    getFare(
        trainId: number,
        fromStationId: number,
        toStationId: number,
        coachType: number
        ): Observable<Fare> {
        const params = new HttpParams()
            .set('fromStationId', fromStationId)
            .set('toStationId', toStationId)
            .set('coachType', coachType);

        return this.http.get<Fare>(
            `${this.apiUrl}/${trainId}/fare`,
            { params }
        );
    }

  getRoute(trainId: number): Observable<RouteStop[]> {
    return this.http.get<RouteStop[]>(
      `${this.apiUrl}/${trainId}/route`
    );
  }

    searchTrainsByNameOrNumber(search: string): Observable<Train[]> {
        const params = new HttpParams().set('search', search.trim());

        return this.http.get<Train[]>(this.apiUrl, { params });
    }
}