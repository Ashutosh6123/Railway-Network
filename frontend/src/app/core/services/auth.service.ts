import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: number;
  role: string;
  expiresIn: number;
}

export interface RegisterRequest {
  name: string;
  email: string;
  phoneNumber: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl =
    `${environment.apiUrl}/api/auth`;

  private readonly tokenKey = 'railreserve_token';
  private readonly userIdKey = 'railreserve_user_id';
  private readonly roleKey = 'railreserve_role';
  
  private readonly loggedIn = signal(
    !!localStorage.getItem(this.tokenKey)
  );
  private readonly role = signal<string | null>(
    localStorage.getItem(this.roleKey)
  );

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.apiUrl}/login`,
        request
      )
      .pipe(
        tap(response => {
          this.storeAuthentication(response);
        })
      );
  }

  register(request: RegisterRequest): Observable<void> {
      return this.http.post<void>(
          `${this.apiUrl}/register`,
          request
      );
  }

  private storeAuthentication(
    response: LoginResponse
  ): void {
    localStorage.setItem(
      this.tokenKey,
      response.token
    );

    localStorage.setItem(
      this.userIdKey,
      response.userId.toString()
    );

    localStorage.setItem(
      this.roleKey,
      response.role
    );
    this.loggedIn.set(true);

    this.role.set(response.role);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }

  isAuthenticated() {
    return this.loggedIn();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUserId(): number | null {
    const userId = localStorage.getItem(
      this.userIdKey
    );

    return userId ? Number(userId) : null;
  }

  getRole(): string | null {
    return this.role();
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userIdKey);
    localStorage.removeItem(this.roleKey);
    this.role.set(null);
    this.loggedIn.set(false);
  }
}