import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoadingService } from './loading-spinner.service';
import { Observable, finalize, map } from 'rxjs';

interface LoginResponse {
  token: string;
  user: {
    id: string;
    name: string;
    email: string;
    role: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'http://localhost:5154/api/auth';

  constructor(private http: HttpClient, private loadingService: LoadingService) {}

  login(email: string, password: string): Observable<LoginResponse> {
    this.loadingService.show();
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/login`, { email, password })
      .pipe(finalize(() => this.loadingService.hide()));
  }

  forgetPassword(email: string) {
    return this.http.post(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(email: string, otp: string, newPassword: string) {
    return this.http.post(`${this.baseUrl}/reset-password`, {
      Email: email,
      Otp: otp,
      NewPassword: newPassword,
    });
  }

  register(name: string, email: string, password: string, role: string) {
    return this.http.post(`${this.baseUrl}/register`, { name, email, password });
  }

  confirmOtp(email: string, otp: string) {
    return this.http.post(`${this.baseUrl}/confirm-otp`, { Email: email, Otp: otp });
  }

  getProfile(): Observable<{ name: string; role: string }> {
    this.loadingService.show();

    return this.http
      .get<{ name: string; role: string }>(`${this.baseUrl}/profile`)
      .pipe(finalize(() => this.loadingService.hide()));
  }
}
