import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AuthService,
  LoginResponse
} from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

  email = '';
  password = '';

  isLoading = false;
  errorMessage = '';

  login(): void {
    if (!this.email.trim() || !this.password) {
      this.errorMessage = 'Please enter your email and password.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login({
      email: this.email.trim(),
      password: this.password
    }).subscribe({
      next: (response: LoginResponse) => {
        console.log('Login successful:', response);

        this.isLoading = false;

        if (this.returnUrl) {
          this.router.navigateByUrl(this.returnUrl);
          return;
        }

        this.router.navigate(['/']);
      },

      error: error => {
        console.error('Login failed:', error);

        this.errorMessage =
          'Invalid email or password. Please try again.';

        this.isLoading = false;
      }
    });
  }
}