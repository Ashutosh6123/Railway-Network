import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { PendingBooking, PendingBookingService } from '../../core/services/pending-booking.service';
import { TrainService } from '../../core/services/train.service';
import { FormsModule } from '@angular/forms';
import {
  BookingRequest,
  BookingResponse,
  ReservationService
} from '../../core/services/reservation.service';


@Component({
  selector: 'app-payment',
  imports: [FormsModule, DatePipe],
  templateUrl: './payment.html',
  styleUrl: './payment.css'
})
export class Payment {
  private readonly pendingBookingService = inject(PendingBookingService);
  private readonly router = inject(Router);
  private readonly trainService = inject(TrainService);
  private readonly reservationService = inject(ReservationService);

  booking: PendingBooking | null = null;
  fare = signal<number | null>(null);
    isFareLoading = signal(false);
    fareError = signal('');
    isPaymentProcessing = signal(false);
    paymentError = signal('');

    cardNumber = '';
    expiry = '';
    cvv = '';
    cardName = '';

  ngOnInit(): void {
        this.booking = this.pendingBookingService.getBooking();

        if (!this.booking) {
            this.router.navigate(['/']);
            return;
        }

        this.loadFare();
    }

    getCoachTypeName(): string {
        switch (this.booking?.request.coachType) {
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

    private loadFare(): void {
        if (!this.booking) {
            return;
        }

        const request = this.booking.request;

        this.isFareLoading.set(true);
        this.fareError.set('');

        this.trainService.getFare(
            request.trainId,
            request.fromStationId,
            request.toStationId,
            request.coachType
        ).subscribe({
            next: response => {
            this.fare.set(response.amount);
            this.isFareLoading.set(false);
            },
            error: error => {
            this.isFareLoading.set(false);
            this.fareError.set('Unable to load the fare. Please try again.');
            console.error('Fare loading failed:', error);
            }
        });
    }

    processPayment(): void {
        if (!this.isPaymentValid() || !this.booking || this.fare() === null) {
            return;
        }

        this.isPaymentProcessing.set(true);
        this.paymentError.set('');

        const request = this.booking.request;

        // Dummy payment processing.
        // Once payment succeeds, create the actual booking.
        setTimeout(() => {
            this.createBooking(request);
        }, 1000);
    }

    private createBooking(request: BookingRequest): void {
        this.reservationService.createBooking(request).subscribe({
            next: response => {
            this.isPaymentProcessing.set(false);

            this.router.navigate(['/booking-confirmation'], {
                state: {
                booking: response
                }
            });
            },
            error: error => {
            this.isPaymentProcessing.set(false);
            this.paymentError.set(
                'Payment was successful, but we could not complete the booking. Please try again.'
            );
            
            console.error('Booking failed after payment:', error);
            }
        });
    }

    getPassengerCount(): number {
        return this.booking?.request.passengers.length ?? 0;
    }

    isPaymentValid(): boolean {
        return (
            this.cardNumber.trim().length > 0 &&
            this.expiry.trim().length > 0 &&
            this.cvv.trim().length > 0 &&
            this.cardName.trim().length > 0
        );
    }
}