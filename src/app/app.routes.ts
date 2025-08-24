import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { ForgetPasswordComponent } from './components/forget-password/forget-password.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { ErrorPageComponent } from './components/error-page/error-page.component';
import { RegisterComponent } from './components/register/register.component';
import { ConfirmOtpComponent } from './components/confirm-otp/confirm-otp.component';
import { ProfileComponent } from './components/profile/profile.component';
import { AuthGuard } from './guards/auth.guard';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { SettingComponent } from './components/setting/setting.component';
import { ProjectComponent } from './components/projects/projects.component';


export const routes: Routes = [

  // Auth Component
  { path: 'login', component: LoginComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [AuthGuard], data: { roles: ['Admin'] } },
  { path: 'setting', component: SettingComponent },
  { path: 'forgot-password', component: ForgetPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-otp', component: ConfirmOtpComponent },
  // { path: 'projects', component: ProjectComponent },
  { path: 'projects', component: ProjectComponent, /* canActivate: [AuthGuard], data: { roles: ['Admin','ProjectManager'] } */ },
// { path: 'projects/:id', component: ProjectComponent },

  // Automatic Direction
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  // Error Direction
    { path: 'error', component: ErrorPageComponent },
    { path: '**', redirectTo: '/error' }

];
