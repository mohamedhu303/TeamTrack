import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

canActivate(route: ActivatedRouteSnapshot): boolean {
  const userStr = sessionStorage.getItem('user');
  if (!userStr) {
    this.router.navigate(['/login']);
    return false;
  }

  const user = JSON.parse(userStr);
  const allowedRoles = route.data['roles'] as string[];
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    this.router.navigate(['/error']);
    return false;
  }

  return true;
}

}
