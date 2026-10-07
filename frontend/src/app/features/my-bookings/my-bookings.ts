import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import {
  MyBookingResponse,
  ReservationService
} from '../../core/services/reservation.service';
import { StationService } from '../../core/services/station.service';

@Component({
  selector: 'app-my-bookings',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css'
})
export class MyBookings implements OnInit {

  private readonly reservationService = inject(ReservationService);
  private readonly stationService = inject(StationService);

  bookings = signal<MyBookingResponse[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  stationNames = signal<Record<number, string>>({});
  bookingToCancel = signal<MyBookingResponse | null>(null);
  isCancellationProcessing = signal(false);
  cancellationError = signal('');
  cancellationSuccess = signal('');

  ngOnInit(): void {
    this.loadBookings();
  }

  private loadBookings(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.reservationService.getMyBookings().subscribe({
      next: response => {
        this.bookings.set(response);
        this.loadStationNames();
        this.isLoading.set(false);
      },
      error: error => {
        this.isLoading.set(false);
        this.errorMessage.set(
          'Unable to load your bookings. Please try again.'
        );

        console.error('Failed to load bookings:', error);
      }
    });
  }

  private loadStationNames(): void {
    this.stationService.getAllStations().subscribe({
      next: stations => {
        const names: Record<number, string> = {};

        stations.forEach(station => {
          names[station.id] = station.name;
        });

        this.stationNames.set(names);
      },
      error: error => {
        console.error('Failed to load stations:', error);
      }
    });
  }

  getStationName(stationId: number): string {
    return this.stationNames()[stationId] ?? `Station #${stationId}`;
  }

  getStatusName(status: number): string {
    switch (status) {
      case 0:
        return 'Confirmed';
      case 1:
        return 'Waitlisted';
      case 2:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  getCoachTypeName(coachType: number): string {
    switch (coachType) {
      case 0:
        return 'General';
      case 1:
        return 'Sleeper';
      case 2:
        return 'AC 3 Tier';
      case 3:
        return 'AC 2 Tier';
      case 4:
        return 'AC 1 Tier';
      default:
        return 'Unknown';
    }
  }

  getQuotaName(quota: number): string {
    switch (quota) {
      case 0:
        return 'General';
      case 1:
        return 'Ladies';
      default:
        return 'Unknown';
    }
  }

  canCancelBooking(booking: MyBookingResponse): boolean {
    return booking.status === 0 || booking.status === 1;
  }

  cancelBooking(): void {
    const booking = this.bookingToCancel();

    if (!booking) {
      return;
    }

    this.isCancellationProcessing.set(true);
    this.cancellationError.set('');
    this.cancellationSuccess.set('');

    this.reservationService.cancelBooking(booking.pnr).subscribe({
      next: response => {
        this.isCancellationProcessing.set(false);
        this.bookingToCancel.set(null);

        this.cancellationSuccess.set(
          `Booking ${response.pnr} was cancelled successfully. Expected refund: ₹${response.totalFare.toFixed(2)}.`
        );

        // Reload the bookings so the UI reflects the actual backend state.
        this.loadBookings();
      },

      error: error => {
        this.isCancellationProcessing.set(false);

        const message =
          typeof error?.error === 'string'
            ? error.error
            : error?.error?.message ?? error?.message;

        this.cancellationError.set(
          message || 'We could not cancel this booking. Please try again.'
        );
      }
    });
  }

  // confirmCancellation(booking: MyBookingResponse): void {
  //   this.bookingToCancel.set(booking);
  //   this.cancellationError.set('');
  //   this.cancellationSuccess.set('');
  // }
  confirmCancellation(booking: MyBookingResponse): void {
  console.log('Cancel clicked:', booking);

  this.bookingToCancel.set(booking);

  console.log('bookingToCancel:', this.bookingToCancel());

  this.cancellationError.set('');
  this.cancellationSuccess.set('');
}
  closeCancellation(): void {
    if (this.isCancellationProcessing()) {
      return;
    }

    this.bookingToCancel.set(null);
    this.cancellationError.set('');
  }
}