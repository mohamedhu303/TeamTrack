import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../services/setting.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-settings',
  templateUrl: './setting.component.html',
  styleUrls: ['./setting.component.scss'],
  imports: [CommonModule, FormsModule, RouterModule],
})
export class SettingComponent implements OnInit {
  showOtpForm: boolean = false;
  showConfirmDialog: boolean = false;
  user: any = null;
  isDark = false;
  showPasswordModal = false;
  showCurrent = false;
  showNew = false;
  showConfirm = false;
  confirmNewPassword = '';
  passwordError = '';
  showPassword = false;
  confirmPassword = '';
  showOTPModal: boolean = false;

  changePasswordModel = {
    email: '',
    currentPassword: '',
    newPassword: '',
    otpCode: '',
  };

  constructor(private accountService: AccountService, private alert: AlertService) {}

  get roleName(): string {
    if (!this.user) return '';

    const raw = this.user.userRole ?? this.user.role;

    const key = String(raw);

    const roleMap: Record<string, string> = {
      '0': 'Administrator',
      '1': 'User',
      '2': 'Manager',
      Admin: 'Administrator',
      User: 'User',
      Manager: 'Manager',
      ProjectManager: 'Manager',
      TeamMember: 'User',
    };

    return roleMap[key] ?? '';
  }

  ngOnInit(): void {
    this.loadProfile();
    const saved = localStorage.getItem('theme');
    this.isDark = saved === 'dark';
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }

  loadProfile() {
    this.accountService.getProfileDetails().subscribe({
      next: (res) => {
        this.user = res;
        this.changePasswordModel.email = res.email;
      },
      error: (err) => this.alert.showAlert('Failed to load profile', 'error'),
    });
  }

  openChangePasswordModal() {
    this.showPasswordModal = true;
  }

  closeModal() {
    this.showPasswordModal = false;
    this.passwordError = '';
  }

  submitPassword() {
    if (this.changePasswordModel.newPassword !== this.confirmNewPassword) {
      this.passwordError = 'Passwords do not match!';
      return;
    }

    if (!this.validatePassword(this.changePasswordModel.newPassword)) {
      this.alert.showAlert(
        'Password must be at least 8 characters, include uppercase, lowercase, number and special character',
        'error'
      );
      return;
    }

    this.accountService.verifyCurrentPassword(this.changePasswordModel.currentPassword).subscribe({
      next: (isValid) => {
        if (!isValid) {
          this.alert.showAlert('Current password is incorrect', 'error');
          return;
        }

        this.accountService.sendOtpForPasswordChange().subscribe({
          next: () => {
            this.showPasswordModal = false;
            this.showOTPModal = true;
            this.alert.showAlert('OTP sent to your email', 'success');
          },
          error: (err) => this.alert.showAlert(err.error?.message || 'Failed to send OTP', 'error'),
        });
      },
      error: () => this.alert.showAlert('Failed to verify current password', 'error'),
    });
  }

  validatePassword(password: string): boolean {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
    return regex.test(password);
  }

  verifyOtp() {
    this.accountService.changePasswordWithOtp(this.changePasswordModel).subscribe({
      next: () => {
        this.showOTPModal = false;
        this.alert.showAlert('Password changed successfully', 'success');
      },
      error: (err) =>
        this.alert.showAlert(err.error?.message || 'Failed to change password', 'error'),
    });
  }

  onOtpInput() {
    if (this.changePasswordModel.otpCode.length === 6) {
      this.verifyOtp();
    }
  }

  openModal() {
    this.showOTPModal = true;
  }
}
