import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
type AlertType = 'success' | 'error' | 'warning' | 'info';

@Injectable({ providedIn: 'root' })
export class AlertService {
  private alertSubject = new Subject<{ message: string; type: string }>();
  alert$ = this.alertSubject.asObservable();

  showAlert(message: string, type: AlertType = 'success') {
    this.alertSubject.next({ message, type });
  }
}
