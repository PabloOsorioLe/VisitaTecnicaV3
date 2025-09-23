import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import Swal from 'sweetalert2';
import flatpickr from 'flatpickr';
import { Spanish } from 'flatpickr/dist/l10n/es.js';

flatpickr.localize(Spanish);

@Component({
  selector: 'app-reuniones',
  templateUrl: './reuniones.component.html',
  styleUrls: ['./reuniones.component.css']
})
export class ReunionesComponent implements OnInit {
  reuniones: any[] = [];
  reunionesPaginadas: any[] = [];
  paginaActual = 1;
  elementosPorPagina = 6;
  totalPaginas = 0;
  paginas: number[] = [];

  participantes: string[] = [
    'Juan Pérez',
    'María Gómez',
    'Carlos Ruiz',
    'Ana Torres',
    'Ignacio Ríos'
  ];

  flatpickrOptions = {
    locale: Spanish,
    dateFormat: 'd-m-Y',
    altInput: true,
    altFormat: 'd-m-Y',
    monthSelectorType: 'dropdown',
    static: true
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarReuniones();
  }

  cargarReuniones() {
    this.http.get<any[]>(`${environment.apiUrl}/reuniones`).subscribe(data => {
      this.reuniones = data.map((r, index) => ({
        ...r,
        fecha: r.fecha ? new Date(r.fecha) : null,
        participante: Array.isArray(r.participante)
          ? r.participante
          : r.participante
            ? r.participante.split(',').map((p: string) => p.trim())
            : [],
        contador: index + 1,
        nueva: false,
        bajando: false,
        guardando: false,
        fadeOut: false,
        eliminando: false
      }));
      this.actualizarPaginacion();
    });
  }

  actualizarPaginacion() {
    this.totalPaginas = Math.ceil(this.reuniones.length / this.elementosPorPagina);
    this.paginas = Array.from({ length: this.totalPaginas }, (_, i) => i + 1);
    const inicio = (this.paginaActual - 1) * this.elementosPorPagina;
    const fin = inicio + this.elementosPorPagina;
    this.reunionesPaginadas = this.reuniones.slice(inicio, fin);
  }

  cambiarPagina(pagina: number) {
    if (pagina < 1 || pagina > this.totalPaginas) return;
    this.paginaActual = pagina;
    this.actualizarPaginacion();
  }

  guardar(row: any): void {
    let fechaISO: string | null = null;

    if (row.fecha) {
      if (row.fecha instanceof Date) {
        fechaISO = row.fecha.toISOString();
      } else if (typeof row.fecha === 'string') {
        const partes = row.fecha.split('-');
        if (partes.length === 3) {
          const d = new Date(+partes[2], +partes[1] - 1, +partes[0]);
          if (!isNaN(d.getTime())) {
            fechaISO = d.toISOString();
          }
        }
      }
    }

    const payload = {
      ...row,
      fecha: fechaISO,
      participante: Array.isArray(row.participante)
        ? row.participante.join(', ')
        : row.participante
    };

    // ✅ NUEVA REUNIÓN
    if (row.id === 0 || row.id === undefined) {
      row.guardando = true;

      this.http.post(`${environment.apiUrl}/reuniones`, payload).subscribe({
        next: (resp: any) => {
          row.id = resp.id || new Date().getTime();
          row.guardando = false;
          row.bajando = true;

          row.bajando = true;

// Primero aplica la clase .bajando y espera la animación
setTimeout(() => {
  row.bajando = false;
  row.fadeOut = true;

  // Espera el fadeOut, luego mueve al final
  setTimeout(() => {
    const index = this.reuniones.indexOf(row);
    if (index > -1) {
      this.reuniones.splice(index, 1);
      this.reuniones.push(row);
    }

    row.fadeOut = false;
    row.nueva = false;

    this.actualizarContadores();
    this.actualizarPaginacion();
  }, 600); // Tiempo del fadeOut
}, 500); // Tiempo del .bajar

          Swal.fire({
            icon: 'success',
            title: 'Reunión Creada',
            html: `Se creó correctamente la reunión:<br><b>ID:</b> ${row.id}<br><b>Título:</b> ${row.titulo || '(sin título)'}`,
            showConfirmButton: false,
            timer: 2000,
            width: '400px',
            customClass: {
              title: 'swal2-title-sm',
              popup: 'swal2-popup-sm',
              confirmButton: 'swal2-confirm-sm'
            }
          });
        },
        error: () => {
          row.guardando = false;
          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se pudo guardar la reunión.',
            showConfirmButton: false,
            timer: 1500
          });
        }
      });

    } else {
      // ✅ ACTUALIZAR
      this.http.put(`${environment.apiUrl}/reuniones/${row.id}`, payload).subscribe(() => {
        Swal.fire({
          icon: 'success',
          title: 'Reunión Actualizada',
          html: `Se actualizó correctamente la reunión:<br><b>ID:</b> ${row.id}<br><b>Título:</b> ${row.titulo || '(sin título)'}`,
          showConfirmButton: false,
          timer: 2000,
          width: '400px',
          customClass: {
            title: 'swal2-title-sm',
            popup: 'swal2-popup-sm',
            confirmButton: 'swal2-confirm-sm'
          }
        });
      }, () => {
        Swal.fire({
          icon: 'error',
          title: 'Error',
          text: 'No se pudo guardar la reunión.',
          showConfirmButton: false,
          timer: 1500
        });
      });
    }
  }

  eliminar(reunion: any) {
    Swal.fire({
      title: '¿Estás seguro?',
      text: `¿Deseas eliminar la reunión "${reunion.titulo}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
      customClass: {
        popup: 'swal2-popup-sm',
        title: 'swal2-title-sm',
        confirmButton: 'swal2-confirm-sm',
        cancelButton: 'swal2-cancel-sm'
      },
      width: '400px'
    }).then((result) => {
      if (result.isConfirmed) {
        const fila = this.reunionesPaginadas.find(r => r.id === reunion.id);
        if (fila) {
          fila.eliminando = true;

          setTimeout(() => {
            this.http.delete(`${environment.apiUrl}/reuniones/${reunion.id}`).subscribe({
              next: () => {
                this.reuniones = this.reuniones.filter(r => r.id !== reunion.id);
                this.actualizarPaginacion();

                Swal.fire({
                  title: '¡Eliminado!',
                  text: 'La reunión ha sido eliminada correctamente.',
                  icon: 'success',
                  showConfirmButton: false,
                  timer: 1500,
                  width: '400px',
                  customClass: {
                    popup: 'swal2-popup-sm',
                    title: 'swal2-title-sm',
                    confirmButton: 'swal2-confirm-sm'
                  }
                });
              },
              error: (err) => {
                console.error('Error al eliminar reunión:', err);
                Swal.fire({
                  title: 'Error',
                  text: 'No se pudo eliminar la reunión.',
                  icon: 'error',
                  showConfirmButton: true,
                  width: '400px'
                });
              }
            });
          }, 600);
        }
      }
    });
  }

  crearNuevaReunion() {
    const nuevaReunion = {
      id: 0,
      titulo: '',
      glosa: '',
      glosaDetalle: '',
      participante: [],
      fecha: new Date(),
      contador: this.reuniones.length + 1,
      nueva: true,
      guardando: false,
      bajando: false,
      fadeOut: false,
      eliminando: false
    };

    this.reuniones.unshift(nuevaReunion);
    this.paginaActual = 1;
    this.actualizarPaginacion();
  }

  private actualizarContadores() {
    this.reuniones.forEach((r, i) => r.contador = i + 1);
  }

  trackById(index: number, item: any): number {
    return item.id;
  }
}
