import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertService } from '../../services/alert.service';
import { NgIf } from '@angular/common';

type AlertType = 'success' | 'error' | 'warning' | 'info';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, NgIf],
  template: `
    <div *ngIf="visible" class="alert" [ngClass]="type">
      {{ message }}
    </div>
  `,
  styles: [
    `
      .alert {
        padding: 10px;
        border-radius: 5px;
        position: fixed;
        bottom: 20px;
        right: 20px;
        z-index: 999;
      }
      .success {
        background-color: #d4edda;
        color: #155724;
      }
      .error {
        background-color: #f8d7da;
        color: #721c24;
      }
    `,
  ],
})
export class AlertComponent {

  message = '';
  type: 'success' | 'error' = 'success';
  visible = false;

  constructor(private alertService: AlertService) {
    this.alertService.alert$.subscribe((a) => {
      this.message = a.message;
      this.type = a.type === 'success' || a.type === 'error' ? a.type : 'success';
      this.visible = true;

      setTimeout(() => (this.visible = false), 3000);
    });
  }

  close() {
    this.visible = false;
  }
}
