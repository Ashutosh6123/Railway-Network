import { Injectable, inject } from '@angular/core';
import {
  HttpClient,
  HttpParams
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AvailabilityResponse {
  availableSeats: number;
}

export enum CoachType {
  General = 0,
  Sleeper = 1,
  AC3Tier = 2,
  AC2Tier = 3,
  AC1Tier = 4
}

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/api/reservations/availability`;

  getAvailability(
    trainId: number,
    fromStationId: number,
    toStationId: number,
    journeyDate: string,
    coachType: CoachType
  ): Observable<AvailabilityResponse> {

    const params = new HttpParams()
      .set('trainId', trainId)
      .set('fromStationId', fromStationId)
      .set('toStationId', toStationId)
      .set('journeyDate', journeyDate)
      .set('coachType', coachType);

    return this.http.get<AvailabilityResponse>(
      this.apiUrl,
      { params }
    );
  }
}