import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BookingPassengerRequest {
  name: string;
  age: number;
  gender: number;
  address: string;
}

export interface BookingRequest {
  trainId: number;
  fromStationId: number;
  toStationId: number;
  journeyDate: string;
  coachType: number;
  quota: number;
  passengers: BookingPassengerRequest[];
}

export interface BookingPassengerResponse {
  bookingPassengerId: number;
  name: string;
  coachNumber: string;
  seatNumber: string;
}

export interface BookingResponse {
  pnr: string;
  status: number;
  totalFare: number;
  passengers: BookingPassengerResponse[];
  waitlistPosition: number | null;
}

export interface MyBookingResponse {
  pnr: string;
  status: number;
  trainId: number;
  fromStationId: number;
  toStationId: number;
  journeyDate: string;
  coachType: number;
  quota: number;
  totalFare: number;
  passengers: BookingPassengerResponse[];
  waitlistPosition: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/api/reservations`;

    createBooking(request: BookingRequest): Observable<BookingResponse> {
        return this.http.post<BookingResponse>(
        this.apiUrl,
        request
        );
    }
    cancelBooking(pnr: string): Observable<BookingResponse> {
        return this.http.post<BookingResponse>(
            `${this.apiUrl}/${pnr}/cancel`,
            {}
        );
    }
    // createBooking(request: BookingRequest): Observable<any> {
    //     return this.http.post<any>(this.apiUrl, request);
    // }
    getMyBookings(): Observable<MyBookingResponse[]> {
        return this.http.get<MyBookingResponse[]>(`${this.apiUrl}/my`);
    }

    getBookingByPnr(pnr: string): Observable<MyBookingResponse> {
        return this.http.get<MyBookingResponse>(
            `${this.apiUrl}/${pnr}`
        );
    }
}