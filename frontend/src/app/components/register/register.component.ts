import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../services/loading-spinner.service';
import { RouterModule } from '@angular/router';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
})
export class RegisterComponent {
  name = '';
  email = '';
  password = '';
  role: 'TeamMember' = 'TeamMember';
  backendError = '';
  successMessage = '';
  passwordError = '';
  passwordVisible = false;
  confirmPasswordVisible = false;
  confirmPasswordError = '';
  confirmPassword = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {}

  togglePasswordVisibility() {
    this.passwordVisible = !this.passwordVisible;
  }

  toggleConfirmPasswordVisibility() {
    this.confirmPasswordVisible = !this.confirmPasswordVisible;
  }

  onSubmit() {
    this.backendError = '';
    this.successMessage = '';

    if (!this.name || !this.email || !this.password) {
      this.backendError = 'All fields are required';
      return;
    }
    if (!this.confirmPassword) {
      this.confirmPasswordError = 'Confirm Password is required';
    } else if (this.password !== this.confirmPassword) {
      this.confirmPasswordError = "Passwords don't match";
    }

    this.authService.register(this.name, this.email, this.password, this.role).subscribe({
      next: (res: any) => {
        this.successMessage = res.message; // "OTP sent to email..."
        this.alert.showAlert(this.successMessage, 'success');
        // Navigate to OTP page
        this.router.navigate(['/confirm-otp'], { state: { email: this.email } });
      },
      error: (err) => {
        this.backendError = err.error?.message || err.error || 'Registration failed';
        this.alert.showAlert(this.backendError, 'error');
      },
    });
  }
}
