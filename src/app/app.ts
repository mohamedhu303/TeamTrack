import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { AlertComponent } from './components/alert/alert.component';
import {ThemeToggleComponent} from './components/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-root',
  template: `
    <app-alert></app-alert>
    <app-theme-toggle></app-theme-toggle>
    <router-outlet></router-outlet>
  `,
  standalone: true,
  imports: [
    RouterOutlet,
    CommonModule,
    MatMenuModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    NgSelectModule,
    AlertComponent,
    ThemeToggleComponent
  ]
})
export class AppComponent implements OnInit{
    ngOnInit() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);

    if (savedTheme === 'dark') {
      const input = document.querySelector<HTMLInputElement>('.switch input');
      if (input) input.checked = true;
    }
  }

  toggleTheme(event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    const theme = isChecked ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }
}
