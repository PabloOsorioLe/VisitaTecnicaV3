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
  cargando: boolean = true;              // inicialmente true (muestra loader)
  cargandoVisible: boolean = true;       // opacidad a 1 para fade del loader
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
    if (!this.usuario || !this.password) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos requeridos',
        text: 'Por favor, ingrese usuario y contraseña.'
      });
      return;
    }

    this.cargando = true;
    this.cargandoVisible = true;

    const loginPayload = { rut: this.usuario, password: this.password };

    this.http.post<{ token: string }>('/api/auth/login', loginPayload).subscribe({
      next: (response) => {
        this.fadeOutLoader(() => {
          if (response.token) {
            sessionStorage.setItem('token', response.token);
            Swal.fire({
              icon: 'success',
              title: 'Credenciales correctas',
              showConfirmButton: false,
              timer: 1500,
              background: 'rgba(24, 24, 24, 0.85)',
              color: '#99caff',
              iconColor: '#28a745',
              customClass: {
                popup: 'swal2-popup-custom',
                title: 'swal2-title-custom'
              }
            }).then(() => {
              this.router.navigate(['/visitas']);
              this.idleService.startWatching();
            });
          }
        });
      },
      error: (err) => {
        this.fadeOutLoader(() => {
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
        });
      }
    });
  }

  fadeOutLoader(callback: () => void) {
    this.cargandoVisible = false;
    setTimeout(() => {
      this.cargando = false;
      callback();
    }, 500); // Sincronizado con transición de opacidad
  }

  async ngOnInit(): Promise<void> {
    console.log('LoginComponent OnInit');
    sessionStorage.clear();

    // GET a render/back para "calentar" sin esperar el resultado
    const renderPromise = this.http.get('/api/auth/render').toPromise().catch(() => {});

    // Espera 3 segundos mínimo, luego oculta loader
    await Promise.all([
      renderPromise,
      new Promise(res => setTimeout(res, 3000))
    ]);
    this.cargando = false;
    this.cargandoVisible = false;

    // Bloquea back del browser (mantiene tu lógica)
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
          customClass: { popup: 'swal2-popup-custom' }
        }).then((result) => {
          if (result.isConfirmed && this.deferredPrompt) {
            this.deferredPrompt.prompt();
            this.deferredPrompt.userChoice.then(() => {
              this.deferredPrompt = null;
            });
          }
          if (result.dismiss === Swal.DismissReason.cancel) {
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
          localStorage.setItem('ios-dismissed', 'true');
        }
      });
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      if (this.usuarioInputRef) {
        this.usuarioInputRef.nativeElement.focus();
      }
    }, 300);
  }

  scrollToInput(event: FocusEvent): void {
    const target = event.target as HTMLElement;
    setTimeout(() => {
      target.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 300);
  }
}
