import { Injectable } from '@angular/core';
import { BookingRequest } from './reservation.service';

export interface PendingBooking {
  request: BookingRequest;
  fromStationName: string;
  toStationName: string;
}

@Injectable({
  providedIn: 'root'
})
export class PendingBookingService {
  private pendingBooking: PendingBooking | null = null;

  setBooking(booking: PendingBooking): void {
    this.pendingBooking = booking;
  }

  getBooking(): PendingBooking | null {
    return this.pendingBooking;
  }

  clearBooking(): void {
    this.pendingBooking = null;
  }
}