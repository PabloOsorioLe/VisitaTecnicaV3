export interface Visita {
  id: number;
  nombre: string;
  descripcion: string;
  fechaVisita: Date;
  eliminando?: boolean;
}