import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PendingBookingService } from '../../core/services/pending-booking.service';
import {
  ReservationService,
  BookingRequest,
  BookingResponse
} from '../../core/services/reservation.service';


interface Passenger {
  name: string;
  age: number | null;
  gender: string;
  address: string;
}

@Component({
  selector: 'app-booking',
  imports: [FormsModule, RouterLink],
  templateUrl: './booking.html',
  styleUrl: './booking.css'
})

export class Booking {
  private readonly route = inject(ActivatedRoute);
//   private readonly reservationService = inject(ReservationService);
  private readonly pendingBookingService = inject(PendingBookingService);
  private readonly router = inject(Router);

    trainId: number | null = null;
    fromStationId: number | null = null;
    toStationId: number | null = null;
    bookingResponse = signal<BookingResponse | null>(null);
    isBookingProcessing = signal(false);

    fromStationName = '';
    toStationName = '';

    journeyDate = '';
    coachType: number | null = null;

    selectedQuota = 'General';

    passengers: Passenger[] = [
        {
            name: '',
            age: null,
            gender: '',
            address: ''
        }
    ];

  ngOnInit(): void {
        this.route.queryParamMap.subscribe(params => {
        this.trainId = Number(params.get('trainId'));
        this.fromStationId = Number(params.get('fromStationId'));
        this.toStationId = Number(params.get('toStationId'));
        this.fromStationName = params.get('fromStationName') ?? '';
        this.toStationName = params.get('toStationName') ?? '';
        this.journeyDate = params.get('journeyDate') ?? '';

            

        const coachType = params.get('coachType');
        this.coachType = coachType !== null ? Number(coachType) : null;
        });
    }
    getCoachTypeName(): string {
        switch (this.coachType) {
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

    isPassengerComplete(passenger: Passenger): boolean {
        return (
            passenger.name.trim().length > 0 &&
            passenger.age !== null &&
            passenger.age > 0 &&
            passenger.gender !== '' &&
            passenger.address.trim().length > 0
        );
    }

    isPassengerValid(passenger: Passenger): boolean {
        return (
            passenger.name.trim().length > 0 &&
            passenger.age !== null &&
            passenger.age > 0 &&
            passenger.gender !== '' &&
            passenger.address.trim().length > 0
        );
    }

    arePassengersValidForQuota(): boolean {
        if (this.selectedQuota !== 'Ladies') {
            return true;
        }

        return this.passengers.every(
            passenger => passenger.gender === 'Female'
        );
    }

    isBookingValid(): boolean {
        return (
            this.passengers.every(passenger => this.isPassengerValid(passenger)) &&
            this.arePassengersValidForQuota()
        );
    }

    addPassenger(): void {
        if (this.passengers.length >= 6) {
            return;
        }

        const currentPassenger = this.passengers[this.passengers.length - 1];

        if (!this.isPassengerComplete(currentPassenger)) {
            return;
        }

        this.passengers.push({
            name: '',
            age: null,
            gender: '',
            address: ''
        });
    }

    removePassenger(index: number): void {
        if (this.passengers.length <= 1) {
            return;
        }

        this.passengers.splice(index, 1);
    }

    submitBooking(): void {
        if (!this.isBookingValid()) {
            return;
        }

        if (
            this.trainId === null ||
            this.fromStationId === null ||
            this.toStationId === null ||
            this.coachType === null
        ) {
            return;
        }

        const request: BookingRequest = {
            trainId: this.trainId,
            fromStationId: this.fromStationId,
            toStationId: this.toStationId,
            journeyDate: this.journeyDate,
            coachType: this.coachType,
            quota: this.selectedQuota === 'General' ? 0 : 1,
            passengers: this.passengers.map(passenger => ({
            name: passenger.name.trim(),
            age: passenger.age as number,
            gender: passenger.gender === 'Male' ? 0 : 1,
            address: passenger.address.trim()
            }))
        };

        this.pendingBookingService.setBooking({
            request,
            fromStationName: this.fromStationName,
            toStationName: this.toStationName
        });
        this.router.navigate(['/payment']);
    }
}