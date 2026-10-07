import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { PnrService } from '../../core/services/pnr.service';
import { RouterLink } from '@angular/router';
import { StationService } from '../../core/services/station.service';
import {
  MyBookingResponse,
  ReservationService
} from '../../core/services/reservation.service';

@Component({
  selector: 'app-pnr',
  imports: [FormsModule, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './pnr.html',
  styleUrl: './pnr.css'
})
export class Pnr implements OnInit {
    
  private readonly pnrService = inject(PnrService);
  private readonly reservationService = inject(ReservationService);
  private readonly stationService = inject(StationService);

  readonly authService = inject(AuthService);

  pnr = '';
  stationNames = signal<Record<number, string>>({});
  isLoading = signal(false);
  errorMessage = signal('');

  publicPnrResult = signal<{
    pnr: string;
    status: number;
    journeyDate: string;
  } | null>(null);

  bookingResult = signal<MyBookingResponse | null>(null);

    ngOnInit(): void {
        this.loadStationNames();
    }

    getStationName(stationId: number): string {
        return this.stationNames()[stationId] ?? `Station #${stationId}`;
    }
  checkPnr(): void {
    const pnr = this.pnr.trim().toUpperCase();

    this.errorMessage.set('');
    this.publicPnrResult.set(null);
    this.bookingResult.set(null);

    if (!pnr) {
      this.errorMessage.set('Please enter a PNR number.');
      return;
    }

    this.isLoading.set(true);

    if (this.authService.isLoggedIn()) {
      this.reservationService.getBookingByPnr(pnr).subscribe({
        next: response => {
          this.bookingResult.set(response);
          this.isLoading.set(false);
        },
        error: error => {
          this.handleError(error);
        }
      });

      return;
    }

    this.pnrService.getPnrStatus(pnr).subscribe({
      next: response => {
        this.publicPnrResult.set(response);
        this.isLoading.set(false);
      },
      error: error => {
        this.handleError(error);
      }
    });
  }

  clearResult(): void {
    this.pnr = '';
    this.publicPnrResult.set(null);
    this.bookingResult.set(null);
    this.errorMessage.set('');
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

  private handleError(error: any): void {
    this.isLoading.set(false);

    const message =
      typeof error?.error === 'string'
        ? error.error
        : error?.error?.message ?? error?.message;

    this.errorMessage.set(
      message || 'We could not find a booking with that PNR.'
    );
  }
}