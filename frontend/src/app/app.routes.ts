import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Trains } from './features/trains/trains';
import { Login } from './features/auth/login/login';
import { Booking } from './features/booking/booking';
import { authGuard } from './core/guards/auth.guard';
import { Payment } from './features/payment/payment';
import { BookingConfirmation } from './features/booking-confirmation/booking-confirmation';
import { MyBookings } from './features/my-bookings/my-bookings';
import { Pnr } from './features/pnr/pnr';
import { Register } from './features/auth/register/register';
import { adminGuard } from './core/guards/admin.guard';
import { Admin } from './features/admin/admin';


export const routes: Routes = [
  {
    path: '',
    component: Home
  },
  {
    path: 'trains',
    component: Trains
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'booking',
    component: Booking,
    canActivate: [authGuard]
  },
  {
    path: 'payment',
    component: Payment,
    canActivate: [authGuard]
  },
  {
    path: 'booking-confirmation',
    component: BookingConfirmation,
    canActivate: [authGuard]
  },
  {
    path: 'my-bookings',
    component: MyBookings,
    canActivate: [authGuard]
  },
  {
    path: 'pnr',
    component: Pnr
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: 'admin',
    component: Admin,
    canActivate: [adminGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];