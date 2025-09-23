import { Component, OnInit } from '@angular/core';
import { VisitasService } from '../../services/visitas.service';
import { Visita } from '../../models/visita.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-visitas',
  templateUrl: './visitas.component.html',
  styleUrls: ['./visitas.component.css']
})
export class VisitasComponent implements OnInit {
  visitas: Visita[] = [];
  visitasPaginadas: Visita[] = [];

  mostrarModal: boolean = false;
  esNuevo: boolean = false;
  visitaEditada: Visita = { id: 0, nombre: '', descripcion: '', fechaVisita: new Date() };
  fechaEditada: string = '';

  // Paginación
  paginaActual: number = 1;
  tamanoPagina: number = 10;
  totalPaginas: number = 0;
  paginas: number[] = [];

  constructor(private visitasService: VisitasService) {}

  ngOnInit(): void {
    this.cargarVisitas();
  }

  cargarVisitas() {
    this.visitasService.getVisitas().subscribe(data => {
      this.visitas = data.sort((a, b) => a.id - b.id);
      this.calcularPaginacion();
    });
  }

  private calcularPaginacion() {
    this.totalPaginas = Math.ceil(this.visitas.length / this.tamanoPagina);
    this.paginas = Array.from({ length: this.totalPaginas }, (_, i) => i + 1);
    this.actualizarVisitasPaginadas();
  }

  private actualizarVisitasPaginadas() {
    const inicio = (this.paginaActual - 1) * this.tamanoPagina;
    const fin = inicio + this.tamanoPagina;
    this.visitasPaginadas = this.visitas.slice(inicio, fin);
  }

  cambiarPagina(pagina: number) {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;
      this.actualizarVisitasPaginadas();
    }
  }

  crearVisita() {
    this.esNuevo = true;
    this.visitaEditada = { id: 0, nombre: '', descripcion: '', fechaVisita: new Date() };
    this.fechaEditada = new Date().toISOString().substring(0, 10);
    this.mostrarModal = true;
  }

  editarVisita(id: number) {
    this.esNuevo = false;
    const visita = this.visitas.find(v => v.id === id);
    if (visita) {
      this.visitaEditada = { ...visita };
      this.fechaEditada = new Date(visita.fechaVisita).toISOString().substring(0, 10);
      this.mostrarModal = true;
    }
  }

guardarCambios() {
  const partes = this.fechaEditada.split('-'); // yyyy-mm-dd
  const fechaLocal = new Date(
    Number(partes[0]),
    Number(partes[1]) - 1,
    Number(partes[2]),
    12, 0, 0
  );
  this.visitaEditada.fechaVisita = fechaLocal;

  if (this.esNuevo) {
    this.visitasService.postVisita(this.visitaEditada).subscribe({
      next: () => {
        this.cargarVisitas();
        Swal.fire({
          icon: 'success',
          title: 'Guardado',
          text: 'La persona fue registrada correctamente',
          showConfirmButton: false,
          timer: 1500,
          width: '400px',
          customClass: {
            title: 'swal2-title-sm',
            popup: 'swal2-popup-sm',
            confirmButton: 'swal2-confirm-sm'
          }
        });
        this.cerrarModal();
      },
      error: err => {
        console.error('Error creando visita', err);
        Swal.fire({
          icon: 'error',
          title: 'Error',
          text: 'Error creando persona',
          showConfirmButton: false,
          timer: 1500,
          width: '400px',
          customClass: {
            title: 'swal2-title-sm',
            popup: 'swal2-popup-sm',
            confirmButton: 'swal2-confirm-sm'
          }
        });
      }
    });
  } else {
    const index = this.visitas.findIndex(v => v.id === this.visitaEditada.id);
    if (index !== -1) {
      this.visitas[index] = { ...this.visitaEditada };
      this.actualizarVisitasPaginadas();

      // SweetAlert con texto en HTML (negrita + saltos de línea)
      Swal.fire({
        icon: 'success',
        title: 'Actualizado',
        html: `Se actualizó correctamente la visita:<br><b>ID:</b> ${this.visitaEditada.id}<br><b>Nombre:</b> ${this.visitaEditada.nombre}`,
        showConfirmButton: false,
        timer: 2000,
        width: '400px',
        customClass: {
          title: 'swal2-title-sm',
          popup: 'swal2-popup-sm',
          confirmButton: 'swal2-confirm-sm'
        }
      });
    }
    this.cerrarModal();
  }
}



 eliminarVisita(id: number) {
  const visita = this.visitas.find(v => v.id === id);

  if (!visita) return;

  Swal.fire({
    title: '¿Estás seguro?',
    html: `¿Deseas eliminar la visita:<br><b>ID:</b> ${visita.id}<br><b>Nombre:</b> ${visita.nombre}?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, eliminar',
    cancelButtonText: 'Cancelar',
    width: '400px',
    customClass: {
      title: 'swal2-title-sm',
      popup: 'swal2-popup-sm',
      confirmButton: 'swal2-confirm-sm',
      cancelButton: 'swal2-cancel-sm'
    }
  }).then((result) => {
    if (result.isConfirmed) {
      const index = this.visitas.findIndex(v => v.id === id);
      if (index !== -1) {
        this.visitas[index].eliminando = true;
        this.actualizarVisitasPaginadas();

        setTimeout(() => {
          this.visitasService.deleteVisita(id).subscribe({
            next: () => {
              this.visitas = this.visitas.filter(v => v.id !== id);
              this.calcularPaginacion();

              Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: `La visita "${visita.nombre}" fue eliminada correctamente.`,
                showConfirmButton: false,
                timer: 1500,
                width: '400px',
                customClass: {
                  title: 'swal2-title-sm',
                  popup: 'swal2-popup-sm',
                  confirmButton: 'swal2-confirm-sm'
                }
              });
            },
            error: err => {
              console.error('Error eliminando visita', err);
              Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al eliminar visita',
                showConfirmButton: true,
                width: '400px',
                customClass: {
                  title: 'swal2-title-sm',
                  popup: 'swal2-popup-sm',
                  confirmButton: 'swal2-confirm-sm'
                }
              });
              this.visitas[index].eliminando = false;
              this.actualizarVisitasPaginadas();
            }
          });
        }, 600);
      }
    }
  });
}

  cerrarModal() {
    this.mostrarModal = false;
  }
}
