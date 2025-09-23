import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { ReporteService, EmailRequest, InformeDto } from '../../services/reporte.service';
import Swal from 'sweetalert2';
import { jwtDecode } from 'jwt-decode';

@Component({
  selector: 'app-informe-tecnico',
  templateUrl: './informe-tecnico.component.html',
  styleUrls: ['./informe-tecnico.component.css'],
})
export class InformeTecnicoComponent implements AfterViewInit {
  @ViewChild('firmaCanvas', { static: false }) firmaCanvas!: ElementRef<HTMLCanvasElement>;

  private ctx!: CanvasRenderingContext2D;
  private dibujando = false;

  informe = {
    cliente: '',
    fecha: '',
    direccion: '',
    equipo: '',
    horas: 1,
    estado: '',
    trabajos: '',
    observaciones: '',
    lubricantes: {
      carter: '1',
      transmision: '1',
      caja: '1',
      diferencial: '1',
      hidraulico: '1',
    },
    filtros: {
      motor: '1',
      aire: '1',
      convertidor: '1',
      hidraulico: '1',
      respiradores: '1',
    },
    correo: '',
  };

  constructor(private reporteService: ReporteService) {}

  ngOnInit() {
    this.informe.fecha = this.getFechaActual();
  }

  getFechaActual(): string {
    const hoy = new Date();
    const yyyy = hoy.getFullYear();
    const mm = String(hoy.getMonth() + 1).padStart(2, '0');
    const dd = String(hoy.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  ngAfterViewInit() {
    const canvas = this.firmaCanvas.nativeElement;
    canvas.width = 400;
    canvas.height = 150;

    this.ctx = canvas.getContext('2d')!;
    this.ctx.lineWidth = 2;
    this.ctx.lineCap = 'round';

    canvas.addEventListener('mousedown', (e) => this.iniciarDibujo(e));
    canvas.addEventListener('mousemove', (e) => this.dibujar(e));
    canvas.addEventListener('mouseup', () => this.detenerDibujo());
    canvas.addEventListener('mouseleave', () => this.detenerDibujo());

    canvas.addEventListener('touchstart', (e) => this.iniciarDibujoTouch(e));
    canvas.addEventListener('touchmove', (e) => this.dibujarTouch(e));
    canvas.addEventListener('touchend', () => this.detenerDibujo());
  }

  private getPosicionMouse(event: MouseEvent) {
    const rect = this.firmaCanvas.nativeElement.getBoundingClientRect();
    return {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top,
    };
  }

  private getPosicionTouch(event: TouchEvent) {
    const rect = this.firmaCanvas.nativeElement.getBoundingClientRect();
    const touch = event.touches[0];
    return {
      x: touch.clientX - rect.left,
      y: touch.clientY - rect.top,
    };
  }

  iniciarDibujo(event: MouseEvent) {
    this.dibujando = true;
    const pos = this.getPosicionMouse(event);
    this.ctx.beginPath();
    this.ctx.moveTo(pos.x, pos.y);
  }

  dibujar(event: MouseEvent) {
    if (!this.dibujando) return;
    const pos = this.getPosicionMouse(event);
    this.ctx.lineTo(pos.x, pos.y);
    this.ctx.stroke();
  }

  iniciarDibujoTouch(event: TouchEvent) {
    event.preventDefault();
    this.dibujando = true;
    const pos = this.getPosicionTouch(event);
    this.ctx.beginPath();
    this.ctx.moveTo(pos.x, pos.y);
  }

  dibujarTouch(event: TouchEvent) {
    event.preventDefault();
    if (!this.dibujando) return;
    const pos = this.getPosicionTouch(event);
    this.ctx.lineTo(pos.x, pos.y);
    this.ctx.stroke();
  }

  detenerDibujo() {
    this.dibujando = false;
    this.ctx.closePath();
  }

  limpiarFirma() {
    this.ctx.clearRect(0, 0, this.firmaCanvas.nativeElement.width, this.firmaCanvas.nativeElement.height);
  }

  private getFirmaBase64(): string {
    const canvas = this.firmaCanvas.nativeElement;
    return canvas.toDataURL('image/png').split(',')[1];
  }

  enviarCorreo() {
    const informeDto: InformeDto = {
      cliente: this.informe.cliente,
      fecha: this.informe.fecha,
      direccion: this.informe.direccion,
      equipo: this.informe.equipo,
      horas: this.informe.horas,
      estado: this.informe.estado,
      trabajos: this.informe.trabajos,
      observaciones: this.informe.observaciones,
      lubricanteCarter: this.informe.lubricantes.carter,
      lubricanteTransmision: this.informe.lubricantes.transmision,
      lubricanteCaja: this.informe.lubricantes.caja,
      lubricanteDiferencial: this.informe.lubricantes.diferencial,
      lubricanteHidraulico: this.informe.lubricantes.hidraulico,
      filtroMotor: this.informe.filtros.motor,
      filtroAire: this.informe.filtros.aire,
      filtroConvertidor: this.informe.filtros.convertidor,
      filtroHidraulico: this.informe.filtros.hidraulico,
      filtroRespiradores: this.informe.filtros.respiradores,
      correo: this.informe.correo,
      firmaBase64: this.getFirmaBase64(),
    };

    const data: EmailRequest = {
      destinatario: this.informe.correo,
      parametros: informeDto,
    };

    this.reporteService.enviarCorreo(data).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: '¡Correo enviado!',
          text: 'El informe fue enviado correctamente.',
        });
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'Error al enviar',
          text: err?.error || 'Ocurrió un error inesperado al enviar el correo.',
        });
      },
    });
  }

  descargarPDF() {
    // Extraer UserName desde JWT
    let userName = 'Nombre no disponible';

    const token = sessionStorage.getItem('token');
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        userName = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decodedToken['name'] || userName;
      } catch (error) {
        console.warn('No se pudo decodificar token:', error);
      }
    }

    const datos: InformeDto = {
      cliente: this.informe.cliente,
      fecha: this.informe.fecha,
      direccion: this.informe.direccion,
      equipo: this.informe.equipo,
      horas: this.informe.horas,
      estado: this.informe.estado,
      trabajos: this.informe.trabajos,
      observaciones: this.informe.observaciones,
      lubricanteCarter: this.informe.lubricantes.carter,
      lubricanteTransmision: this.informe.lubricantes.transmision,
      lubricanteCaja: this.informe.lubricantes.caja,
      lubricanteDiferencial: this.informe.lubricantes.diferencial,
      lubricanteHidraulico: this.informe.lubricantes.hidraulico,
      filtroMotor: this.informe.filtros.motor,
      filtroAire: this.informe.filtros.aire,
      filtroConvertidor: this.informe.filtros.convertidor,
      filtroHidraulico: this.informe.filtros.hidraulico,
      filtroRespiradores: this.informe.filtros.respiradores,
      correo: this.informe.correo,
      firmaBase64: this.getFirmaBase64().replace(/^data:image\/(png|jpg);base64,/, ''),
      userName: userName, // asignar nombre extraído
    };

    this.reporteService.descargarReporte(datos).subscribe({
      next: (data: Blob) => {
        const blob = new Blob([data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `InformeTecnico.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error al descargar el PDF:', err);
        Swal.fire({
          icon: 'error',
          title: 'Error al descargar PDF',
          text: 'Hubo un problema al generar el archivo. Intenta nuevamente.',
        });
      },
    });
  }
}
