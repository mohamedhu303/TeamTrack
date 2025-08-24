import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../services/loading-spinner.service';
import { RouterModule } from '@angular/router';
import { Location } from '@angular/common';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-confirm-otp',
  templateUrl: './confirm-otp.component.html',
  styleUrls: ['./confirm-otp.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
})
export class ConfirmOtpComponent {
  email = '';
  otp = '';
  backendError = '';
  successMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    public location: Location,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {
    // جلب الإيميل من state لو جاء من صفحة Register
    const state: any = this.router.getCurrentNavigation()?.extras.state;
    this.email = state?.email || '';
  }

  onSubmit() {
    this.backendError = '';
    this.successMessage = '';

    if (!this.email || !this.otp) {
      this.loadingService.show();
      this.backendError = 'Email and OTP are required';
      this.alert.showAlert(this.backendError, 'error');
      return;
    }

    this.authService.confirmOtp(this.email, this.otp).subscribe({
      next: (res: any) => {
        this.loadingService.hide();
        this.successMessage = res.message;
        localStorage.setItem('token', res.token);
        this.alert.showAlert(res.message, 'success');
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err) => {
        this.loadingService.hide();
        const msg = err.error?.message || err.error || 'OTP confirmation failed';
        this.backendError = msg;
        this.alert.showAlert(msg, 'error');
      },
    });
  }
}
