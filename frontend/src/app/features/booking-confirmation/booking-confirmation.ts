import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { BookingResponse } from '../../core/services/reservation.service';

@Component({
  selector: 'app-booking-confirmation',
  imports: [RouterLink],
  templateUrl: './booking-confirmation.html',
  styleUrl: './booking-confirmation.css'
})
export class BookingConfirmation {
  booking: BookingResponse | null = null;

  constructor(private readonly router: Router) {
    const navigation = this.router.getCurrentNavigation();

    this.booking =
      navigation?.extras.state?.['booking'] ?? null;
  }
}