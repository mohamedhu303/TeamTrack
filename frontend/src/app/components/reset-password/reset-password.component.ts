import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common';
import { LoadingService } from '../../services/loading-spinner.service';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule],
})
export class ResetPasswordComponent {
  email = '';
  otp = '';
  newPassword = '';

  otpError = '';
  passwordError = '';
  backendMessage = '';

  newPasswordVisible: boolean = false;
  newPasswordError: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router,
    public location: Location,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {
    const state = this.router.getCurrentNavigation()?.extras.state as any;
    if (state?.email) this.email = state.email;
  }
  togglePasswordVisibility() {
    this.newPasswordVisible = !this.newPasswordVisible;
  }

  validatePassword(password: string): string | null {
    if (password.length < 8) return 'Password must be at least 8 characters';
    if (!/[A-Z]/.test(password)) return 'Password must include at least one uppercase letter';
    if (!/[a-z]/.test(password)) return 'Password must include at least one lowercase letter';
    if (!/[0-9]/.test(password)) return 'Password must include at least one number';
    if (!/[!@#$%^&*(),.?":{}|<>]/.test(password))
      return 'Password must include at least one symbol';
    return null;
  }

  onSubmit() {
    this.otpError = '';
    this.passwordError = '';
    this.backendMessage = '';

    if (!this.otp) {
      this.otpError = 'OTP is required';
      return;
    }

    const pwError = this.validatePassword(this.newPassword);
    if (pwError) {
      this.passwordError = pwError;
      return;
    }

    this.authService.resetPassword(this.email, this.otp, this.newPassword).subscribe({
      next: (res) => {
        this.alert.showAlert('Password reset successful!', 'success');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        const msg = err.error?.message || 'Something went wrong';
        this.backendMessage = msg;
        this.alert.showAlert(msg, 'error');
      },
    });
  }
}
