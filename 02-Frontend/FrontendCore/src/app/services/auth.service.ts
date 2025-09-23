// src/app/services/auth.service.ts
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { IdleTimeoutService } from './idle-timeout.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(
    private router: Router,
    private idleService: IdleTimeoutService
  ) {}

  logout(): void {
    this.idleService.stopWatching(); // ✅ Detiene el watcher de inactividad
    sessionStorage.clear();
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!sessionStorage.getItem('token') || !!localStorage.getItem('token');
  }
}
