import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Location } from '@angular/common';


@Component({
  selector: 'app-error-page',
  template: `
    <div class="error-container">
      <h1><strong>ERROR 404</strong></h1>
      <h1>😢 Oops! Something went wrong.</h1>
      <p>
        Try refreshing or
        <a (click)="goBack()">Go Back</a>
      </p>
    </div>
  `,
  standalone: true,
  imports: [CommonModule, RouterLink],
  styleUrls: ['./error-page.component.scss'],
})
export class ErrorPageComponent {
    constructor(private location: Location) {}
    
    goBack() {
    this.location.back();
  }
}
