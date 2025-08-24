// spinner.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="spinner-overlay">
      <div class="spinner"></div>
    </div>
  `,
  styleUrls: ['./loading-spinner.component.scss']
})
export class SpinnerComponent {}
