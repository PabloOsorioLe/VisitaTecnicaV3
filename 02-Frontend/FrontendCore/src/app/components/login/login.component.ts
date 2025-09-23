import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { IdleTimeoutService } from 'src/app/services/idle-timeout.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-clogin',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, AfterViewInit {
  usuario: string = '';
  password: string = '';
  mostrarPassword: boolean = false;
  deferredPrompt: any;

  @ViewChild('usuarioInput') usuarioInputRef!: ElementRef<HTMLInputElement>;

  constructor(
    private router: Router,
    private idleService: IdleTimeoutService,
    private http: HttpClient
  ) {}

  iniciarSesion(): void {
    console.log(`Intentando login con usuario: ${this.usuario}`);

    if (!this.usuario || !this.password) {
      console.warn('Campos usuario o contraseña vacíos');
      Swal.fire({
        icon: 'warning',
        title: 'Campos requeridos',
        text: 'Por favor, ingrese usuario y contraseña.'
      });
      return;
    }

    const loginPayload = {
      rut: this.usuario,
      password: this.password
    };

    console.log('Enviando payload al backend:', loginPayload);

    this.http.post<{ token: string }>('/api/auth/login', loginPayload).subscribe({
      next: (response) => {
        console.log('Respuesta recibida del backend:', response);
        if (response.token) {
          sessionStorage.setItem('token', response.token);

          Swal.fire({
            icon: 'success',
            title: 'Bienvenido',
            text: 'Inicio de sesión exitoso',
            timer: 1500,
            showConfirmButton: false,
            width: '350px',
            background: 'rgba(24, 24, 24, 0.85)',
            color: '#99caff',
            iconColor: '#3399ff',
            customClass: {
              popup: 'swal2-popup-custom',
              title: 'swal2-title-custom',
              confirmButton: 'swal2-confirm-custom'
            }
          });

          setTimeout(() => {
            console.log('Navegando a /visitas');
            this.router.navigate(['/visitas']);
            this.idleService.startWatching();
          }, 1500);
        } else {
          console.error('Token no recibido en la respuesta');
        }
      },
      error: (err) => {
        console.error('Error en la llamada HTTP:', err);

        let mensaje = 'Problemas de conexión con el servidor';

        if (err.status === 401) {
          mensaje = 'Clave inválida o usuario incorrecto';
        } else if (err.status === 404) {
          mensaje = 'Servicio no encontrado. Verifica conexión de frontend y backend.';
        } else if (err.error) {
          if (typeof err.error === 'string') {
            mensaje = err.error;
          } else if (err.error.message) {
            mensaje = err.error.message;
          }
        }

        Swal.fire({
          icon: 'error',
          title: 'Acceso denegado',
          text: mensaje,
          timer: 2000,
          showConfirmButton: false,
          width: '350px',
          background: 'rgba(30, 10, 10, 0.85)',
          color: '#ff9999',
          iconColor: '#ff4444',
          customClass: {
            popup: 'swal2-popup-custom',
            title: 'swal2-title-custom',
            confirmButton: 'swal2-confirm-custom'
          }
        });
      }
    });
  }

  ngOnInit(): void {
    console.log('LoginComponent OnInit');

    sessionStorage.clear();

    history.pushState(null, '', location.href);
    window.onpopstate = () => {
      history.pushState(null, '', location.href);
    };

    const isStandalone =
      window.matchMedia('(display-mode: standalone)').matches ||
      (window.navigator as any).standalone === true;

    if (!isStandalone && localStorage.getItem('pwa-dismissed') !== 'true') {
      window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        this.deferredPrompt = e;
        console.log('Evento beforeinstallprompt capturado');

        Swal.fire({
          toast: true,
          position: 'top',
          icon: 'info',
          title: '¿Deseas instalar esta aplicación?',
          showConfirmButton: true,
          confirmButtonText: 'Instalar',
          showCancelButton: true,
          cancelButtonText: 'No mostrar más',
          timer: 3000,
          timerProgressBar: true,
          customClass: {
            popup: 'swal2-popup-custom'
          }
        }).then((result) => {
          if (result.isConfirmed && this.deferredPrompt) {
            console.log('Usuario aceptó instalar la app');
            this.deferredPrompt.prompt();
            this.deferredPrompt.userChoice.then(() => {
              this.deferredPrompt = null;
            });
          }

          if (result.dismiss === Swal.DismissReason.cancel) {
            console.log('Usuario canceló instalación PWA');
            localStorage.setItem('pwa-dismissed', 'true');
          }
        });
      });
    }

    const isIos = /iphone|ipad|ipod/.test(window.navigator.userAgent.toLowerCase());
    const isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

    if (isIos && isSafari && localStorage.getItem('ios-dismissed') !== 'true') {
      Swal.fire({
        icon: 'info',
        title: '¿Deseas instalar esta app?',
        html: 'Presiona <strong>Compartir</strong> y luego <strong>"Agregar a pantalla de inicio"</strong>',
        toast: true,
        position: 'top',
        showConfirmButton: false,
        showCancelButton: true,
        cancelButtonText: 'No mostrar más',
        timer: 6000
      }).then(result => {
        if (result.dismiss === Swal.DismissReason.cancel) {
          console.log('Usuario canceló mensaje instalación iOS');
          localStorage.setItem('ios-dismissed', 'true');
        }
      });
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      if (this.usuarioInputRef) {
        this.usuarioInputRef.nativeElement.focus();
        console.log('Input usuario enfocado');
      }
    }, 300);
  }

  scrollToInput(event: FocusEvent): void {
    const target = event.target as HTMLElement;
    setTimeout(() => {
      target.scrollIntoView({ behavior: 'smooth', block: 'center' });
      console.log(`Scroll a input: ${target.id}`);
    }, 300);
  }
}
