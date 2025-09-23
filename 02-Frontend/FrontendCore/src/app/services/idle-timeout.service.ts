import { Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class IdleTimeoutService {
  private timeout: any;
  private warningTimeout: any;
  private readonly idleTime = 60 * 1000; // 1 minuto
  private readonly warningTime = 30 * 1000; // A los 30 segundos, mostrar advertencia
  private isWatching: boolean = false;

  constructor(private router: Router, private ngZone: NgZone) {
    console.log('IdleTimeoutService iniciado');
  }

  startWatching() {
    if (this.isWatching) {
      console.log('IdleTimeoutService: ya estaba activo');
      return;
    }

    console.log('IdleTimeoutService: startWatching');
    this.isWatching = true;
    this.resetTimer();

    ['mousemove', 'mousedown', 'keypress', 'touchstart', 'scroll'].forEach(event =>
      document.addEventListener(event, this.resetTimer.bind(this))
    );
  }

  stopWatching() {
    if (!this.isWatching) return;

    console.log('IdleTimeoutService: stopWatching');
    this.isWatching = false;

    clearTimeout(this.timeout);
    clearTimeout(this.warningTimeout);

    ['mousemove', 'mousedown', 'keypress', 'touchstart', 'scroll'].forEach(event =>
      document.removeEventListener(event, this.resetTimer.bind(this))
    );

    // Cierra alertas activas si hay alguna abierta
    Swal.close();
  }

  private resetTimer() {
    if (!this.isWatching) return;

    console.log('IdleTimeoutService: resetTimer');
    clearTimeout(this.timeout);
    clearTimeout(this.warningTimeout);

    this.timeout = setTimeout(() => {
      this.handleIdleTimeout();
    }, this.idleTime);

    this.warningTimeout = setTimeout(() => {
      this.showWarningAlert();
    }, this.warningTime);
  }

 private showWarningAlert() {
  if (!this.isWatching) return;

  Swal.fire({
    title: '¿Sigues ahí?',
    text: 'Tu sesión se cerrará en 30 segundos por inactividad.',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Seguir conectado',
    cancelButtonText: 'Cerrar sesión',
    allowOutsideClick: false,
    allowEscapeKey: false,
    timer: 30000,
    timerProgressBar: true,
    didOpen: () => {
      // Cambiar color de la barra de progreso del timer
      const timerBar = Swal.getHtmlContainer()?.querySelector('.swal2-timer-progress-bar') as HTMLElement | null;
      if (timerBar) {
        timerBar.style.background = 'linear-gradient(to right, #1976d2, #64b5f6)'; // azul degradado
        // timerBar.style.background = '#1976d2'; // o color sólido si prefieres
      }
    }
  }).then(result => {
    if (!this.isWatching) return;

    if (result.isConfirmed) {
      this.resetTimer();
    } else {
      this.handleIdleTimeout();
    }
  });
}
  private handleIdleTimeout() {
    this.stopWatching();
    localStorage.clear();
    sessionStorage.clear();

    Swal.fire({
      icon: 'info',
      title: 'Sesión cerrada',
      text: 'Tu sesión ha sido cerrada por inactividad.',
      confirmButtonText: 'OK',
      allowOutsideClick: false
    }).then(() => {
      this.router.navigate(['/login']);
    });
  }
}
