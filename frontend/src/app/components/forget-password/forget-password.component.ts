import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common';
import { LoadingService } from '../../services/loading-spinner.service';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-forget-password',
  templateUrl: './forget-password.component.html',
  styleUrls: ['./forget-password.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule],
})
export class ForgetPasswordComponent {
  email = '';
  emailError = '';
  backendMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    public location: Location,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {}

  validateEmail(email: string): boolean {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  onSubmit() {
    this.emailError = '';
    this.backendMessage = '';

    if (!this.email) {
      this.emailError = 'Email is required';
      this.alert.showAlert(this.emailError, 'error');
      return;
    }
    if (!this.validateEmail(this.email)) {
      this.emailError = 'Invalid email format';
      this.alert.showAlert(this.emailError, 'error');
      return;
    }

    this.authService.forgetPassword(this.email).subscribe({
      next: (res) => {
        this.alert.showAlert('Reset instructions sent to your email', 'success');
        this.router.navigate(['/reset-password'], { state: { email: this.email } });
      },
      error: (err) => {
        const msg = err.error?.message || 'Something went wrong';
        this.alert.showAlert(msg, 'error');
        this.backendMessage = msg;
      },
    });
  }
}
