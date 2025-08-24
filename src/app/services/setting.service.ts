import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = 'http://localhost:5154/api/account'; 

  constructor(private http: HttpClient) {}

  getProfileDetails(): Observable<any> {
    return this.http.get(`${this.baseUrl}/profile-details`);
  }

  sendOtpForPasswordChange(): Observable<any> {
    return this.http.post(`${this.baseUrl}/send-otp-for-password-change`, {});
  }

  changePasswordWithOtp(model: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/change-password-with-otp`, model);
  }

  verifyCurrentPassword(currentPassword: string) {
  return this.http.post<boolean>(`${this.baseUrl}/verify-password`, { currentPassword });
}

}
