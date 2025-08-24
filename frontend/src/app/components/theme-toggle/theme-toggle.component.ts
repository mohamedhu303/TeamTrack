import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.component.html',
  styleUrls: ['./theme-toggle.component.scss'],
  standalone: true
})
export class ThemeToggleComponent implements OnInit {
  isDark = false;

  ngOnInit() {
    const saved = localStorage.getItem('theme');
    this.isDark = saved === 'dark';
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }

  toggleTheme() {
    this.isDark = !this.isDark;
    const theme = this.isDark ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }
}
