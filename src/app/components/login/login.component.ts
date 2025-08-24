import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../services/loading-spinner.service';
import { RouterModule } from '@angular/router';
import { AlertService } from '../../services/alert.service';
 
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
})
export class LoginComponent {
  email = '';
  password = '';
  emailError = '';
  passwordError = '';
  backendError = '';
  passwordVisible = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {}

  togglePasswordVisibility() {
    this.passwordVisible = !this.passwordVisible;
  }

  onSubmit() {
    this.emailError = '';
    this.passwordError = '';
    this.backendError = '';

    if (!this.email) this.emailError = 'Email is required';
    if (!this.password) this.passwordError = 'Password is required';
    if (this.emailError || this.passwordError) return;

    const roleMap: { [key: number]: string } = {
      0: 'Admin',
      1: 'Manager',
      2: 'User',
    };

    this.authService.login(this.email, this.password).subscribe({
      next: (res) => {
        const mappedUser = {
          ...res.user,
          role: roleMap[Number(res.user.role)] || 'Unknown',
        };

        sessionStorage.setItem('token', res.token);
        sessionStorage.setItem('user', JSON.stringify(mappedUser));

        this.alert.showAlert('Logged in successfully ✅', 'success');
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        this.backendError = err.error?.message || 'Login failed';
      },
    });
  }
}
