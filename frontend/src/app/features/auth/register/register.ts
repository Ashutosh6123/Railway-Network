import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  AuthService,
  RegisterRequest
} from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  name = '';
  email = '';
  phoneNumber = '';
  password = '';
  confirmPassword = '';

  isLoading = false;
  errorMessage = '';

  register(): void {
    if (
      !this.name.trim() ||
      !this.email.trim() ||
      !this.phoneNumber.trim() ||
      !this.password ||
      !this.confirmPassword
    ) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    if (this.password.length < 8) {
      this.errorMessage = 'Password must be at least 8 characters long.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request: RegisterRequest = {
      name: this.name.trim(),
      email: this.email.trim(),
      phoneNumber: this.phoneNumber.trim(),
      password: this.password
    };

    this.authService.register(request).subscribe({
        next: () => {
            this.isLoading = false;
            this.cdr.detectChanges();
            this.router.navigate(['/login']);
        },

    // error: error => {
    //     console.error('Registration failed:', error);

    //     this.errorMessage =
    //         error?.error?.message ||
    //         'Unable to create your account. Please try again.';

    //     this.isLoading = false;
    //     }
    
        error: error => {
        console.log('REGISTER ERROR CALLBACK REACHED');

        this.isLoading = false;

        this.errorMessage =
            error?.error?.message ||
            'Unable to create your account. Please try again.';

        console.log('isLoading after error:', this.isLoading);
        console.log('errorMessage after error:', this.errorMessage);

        this.cdr.detectChanges();
        }
    });
  }
}