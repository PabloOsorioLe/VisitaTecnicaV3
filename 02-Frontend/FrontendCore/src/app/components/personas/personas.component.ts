import { Component, OnInit } from '@angular/core';
import { PersonasService } from '../../services/personas.service';
import { Persona } from '../../models/persona.model';
import Swal from 'sweetalert2';
import { MatPaginatorIntl, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';


@Component({
  selector: 'app-personas',
  templateUrl: './personas.component.html',
  styleUrls: ['./personas.component.css']
})
export class PersonasComponent implements OnInit {
  personas: Persona[] = [];

  mostrarModal: boolean = false;
  esNuevo: boolean = false;
  personaEditada: Persona = { id: 0, nombre: '', direccion: '', rut: '' };

  // Paginación
  paginaActual: number = 1;
  tamanoPagina: number = 10;
  totalPaginas: number = 0;
  paginas: number[] = [];
  personasPaginadas: Persona[] = [];

  constructor(private personasService: PersonasService) {}

  ngOnInit(): void {
    this.cargarPersonas();
  }

  cargarPersonas() {
    this.personasService.getPersonas().subscribe(data => {
      this.personas = data;
      this.calcularPaginacion();
    });
  }

  crearPersona() {
    this.esNuevo = true;
    this.personaEditada = { id: 0, nombre: '', direccion: '', rut: '' };
    this.mostrarModal = true;
  }

  editarPersona(id: number) {
    this.esNuevo = false;
    const persona = this.personas.find(p => p.id === id);
    if (persona) {
      this.personaEditada = { ...persona };
      this.mostrarModal = true;
    }
  }

  guardarCambios() {
  if (!this.validarRut(this.personaEditada.rut)) {
    Swal.fire({
      icon: 'error',
      title: 'RUT inválido',
      text: 'El RUT ingresado no es válido. Por favor, revísalo.',
      showConfirmButton: false,
      timer: 1500,
      width: '400px',
      customClass: {
        title: 'swal2-title-sm',
        popup: 'swal2-popup-sm',
        confirmButton: 'swal2-confirm-sm'
      }
    });
    return;
  }

  if (this.esNuevo) {
    this.personasService.postPersona(this.personaEditada).subscribe({
      next: () => {
        this.cargarPersonas();
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
        console.error('Error creando persona', err);
        Swal.fire({
          icon: 'error',
          title: 'ERR',
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
    const index = this.personas.findIndex(p => p.id === this.personaEditada.id);
    if (index !== -1) {
      this.personas[index] = { ...this.personaEditada };
      this.actualizarPersonasPaginadas();

      // SweetAlert para actualización
      Swal.fire({
        icon: 'success',
        title: 'Actualizado',
        html: `Se actualizó correctamente la persona:<br><b>ID:</b> ${this.personaEditada.id}<br><b>Nombre:</b> ${this.personaEditada.nombre}`,
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



eliminarPersona(id: number) {
  const persona = this.personas.find(p => p.id === id);
  if (!persona) return;

  Swal.fire({
    title: '¿Estás seguro?',
    html: `¿Deseas eliminar a la persona:<br><b>ID:</b> ${persona.id}<br><b>Nombre:</b> ${persona.nombre}?`,
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
      const index = this.personas.findIndex(p => p.id === id);
      if (index !== -1) {
        this.personas[index].eliminando = true;

        setTimeout(() => {
          this.personasService.deletePersona(id).subscribe({
            next: () => {
              this.personas = this.personas.filter(p => p.id !== id);
              this.calcularPaginacion();

              Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: `La persona "${persona.nombre}" fue eliminada correctamente.`,
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
              console.error('Error eliminando persona', err);
              this.personas[index].eliminando = false;

              Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al eliminar persona',
                showConfirmButton: true,
                width: '400px',
                customClass: {
                  title: 'swal2-title-sm',
                  popup: 'swal2-popup-sm',
                  confirmButton: 'swal2-confirm-sm'
                }
              });
            }
          });
        }, 1000);
      }
    }
  });
}


  cerrarModal() {
    this.mostrarModal = false;
  }

  private calcularPaginacion() {
    this.totalPaginas = Math.ceil(this.personas.length / this.tamanoPagina);
    this.paginas = Array.from({ length: this.totalPaginas }, (_, i) => i + 1);
    this.paginaActual = 1; // Reiniciar a página 1
    this.actualizarPersonasPaginadas();
  }

  private actualizarPersonasPaginadas() {
    const inicio = (this.paginaActual - 1) * this.tamanoPagina;
    const fin = inicio + this.tamanoPagina;
    this.personasPaginadas = this.personas.slice(inicio, fin);
  }

  cambiarPagina(pagina: number) {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;
      this.actualizarPersonasPaginadas();
    }
  }

formatearRut() {
  let rut = this.personaEditada.rut.replace(/[^\dkK]/gi, '');

  // Permitir máximo 10 caracteres sin formato (9 cuerpo + 1 DV)
  rut = rut.substring(0, 10);

  const cuerpo = rut.slice(0, -1);
  let dv = rut.slice(-1);

  // Validar DV (solo un carácter 0-9 o k/K)
  if (!/^[0-9kK]$/.test(dv)) {
    dv = '';
  }

  // Formatear el cuerpo (con puntos)
  const cuerpoConPuntos = cuerpo
    .split('')
    .reverse()
    .reduce((acc, char, i) => {
      return char + ((i > 0 && i % 3 === 0) ? '.' : '') + acc;
    }, '');

  // Armar el RUT final
  this.personaEditada.rut = cuerpoConPuntos + (dv ? '-' + dv.toUpperCase() : '');
}

validarRut(rutCompleto: string): boolean {
  if (!rutCompleto || typeof rutCompleto !== 'string') return false;

  // Limpiar puntos y guion
  const rut = rutCompleto.replace(/\./g, '').replace('-', '').toUpperCase();

  if (rut.length < 2) return false;

  const cuerpo = rut.slice(0, -1);
  const dv = rut.slice(-1);

  let suma = 0;
  let multiplo = 2;

  for (let i = cuerpo.length - 1; i >= 0; i--) {
    suma += parseInt(cuerpo.charAt(i), 10) * multiplo;
    multiplo = multiplo === 7 ? 2 : multiplo + 1;
  }

  const dvEsperado = 11 - (suma % 11);
  let dvFinal = '';

  if (dvEsperado === 11) dvFinal = '0';
  else if (dvEsperado === 10) dvFinal = 'K';
  else dvFinal = dvEsperado.toString();

  return dv === dvFinal;
}

}
