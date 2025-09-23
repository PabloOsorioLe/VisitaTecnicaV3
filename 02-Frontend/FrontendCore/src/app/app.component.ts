// src/app/app.component.ts
import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { IdleTimeoutService } from './services/idle-timeout.service';
import { AuthService } from './services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'FrontendCore';

  // Nueva propiedad para título dinámico según ruta
  tituloActual = '';

  constructor(
    public router: Router,
    private idleService: IdleTimeoutService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        const isLoginPage = this.router.url === '/login';

        if (isLoginPage) {
          this.idleService.stopWatching();
        } else {
          const isLoggedIn = !!localStorage.getItem('token') || !!sessionStorage.getItem('token');
          if (isLoggedIn) {
            this.idleService.startWatching();
          }
        }

        // Cerrar menú colapsado si está abierto (modo móvil)
        const navbarCollapse = document.querySelector('.navbar-collapse');
        if (navbarCollapse && navbarCollapse.classList.contains('show')) {
          navbarCollapse.classList.remove('show');
        }

        // Actualizar título según ruta activa
        this.setTituloDesdeRuta(event.urlAfterRedirects);
      }
    });
  }

  get mostrarLayout(): boolean {
    return this.router.url !== '/login';
  }

  cerrarSesion(event: Event): void {
    event.preventDefault();
    Swal.fire({
      title: '¿Estás seguro?',
      text: "¿Quieres cerrar sesión?",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sí, salir',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.authService.logout();
      }
    });
  }

  setTituloDesdeRuta(url: string): void {
    if (url.includes('visitas')) this.tituloActual = 'Visitas';
    else if (url.includes('personas')) this.tituloActual = 'Personas';
    else if (url.includes('productos')) this.tituloActual = 'Productos';
    else if (url.includes('informe-tecnico')) this.tituloActual = 'Informe Técnico';
    else if (url.includes('reuniones')) this.tituloActual = 'Reuniones';
    else if (url.includes('players')) this.tituloActual = 'Players';
    else this.tituloActual = '';
  }
  cerrarMenu(): void {
  const navbarCollapse = document.querySelector('.navbar-collapse');
  if (navbarCollapse && navbarCollapse.classList.contains('show')) {
    navbarCollapse.classList.remove('show');
  }
}
}

