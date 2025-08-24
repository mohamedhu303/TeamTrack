import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule, NgIf } from '@angular/common';
import { SpinnerComponent } from '../loading-spinner/loading-spinner.component';
import { LoadingService } from '../../services/loading-spinner.service';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, NgIf, SpinnerComponent, RouterModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
})
export class ProfileComponent implements OnInit {
  user: { name: string; role: string } | null = null;
  backendError = '';

  constructor(
    private authService: AuthService,
    public router: Router,
    public loadingService: LoadingService,
    private alert: AlertService
  ) {}
  showLogoutModal = false;
  project: any;
  ngOnInit() {
    const savedUser = sessionStorage.getItem('user');
    if (savedUser) {
      this.user = JSON.parse(savedUser);
    } else {
      this.authService.getProfile().subscribe({
        next: (res) => {
          this.user = res;
          sessionStorage.setItem('user', JSON.stringify(res));
        },
        error: (err) => {
          this.alert.showAlert(this.backendError, 'error');
        },
      });
    }
  }

  get roleName(): string {
    if (!this.user) return '';
    switch (this.user.role) {
      case 'Admin':
        return 'Administrator';
      case 'User':
        return 'User';
      case 'Manager':
        return 'Manager';
      default:
        return this.user.role;
    }
  }

  get isAdmin(): boolean {
    return this.user?.role === 'Admin';
  }

  logout() {
    sessionStorage.clear();
    this.alert.showAlert('Logged out successfully ✅', 'success');
    this.router.navigate(['/login']);
  }

  openLogoutModal() {
    this.showLogoutModal = true;
  }

  closeLogoutModal() {
    this.showLogoutModal = false;
  }

  confirmLogout() {
    this.showLogoutModal = false;
    sessionStorage.clear();
    this.alert.showAlert('Logged out successfully ✅', 'success');
    this.router.navigate(['/login']);
  }
}
