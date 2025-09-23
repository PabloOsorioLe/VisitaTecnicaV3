import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface InformeDto {
  cliente: string;
  fecha: string;
  direccion: string;
  equipo: string;
  horas: number;
  estado: string;
  trabajos: string;
  observaciones: string;
  lubricanteCarter: string;
  lubricanteTransmision: string;
  lubricanteCaja: string;
  lubricanteDiferencial: string;
  lubricanteHidraulico: string;
  filtroMotor: string;
  filtroAire: string;
  filtroConvertidor: string;
  filtroHidraulico: string;
  filtroRespiradores: string;
  correo: string;
  firmaBase64: string;
  userName?: string;
}


export interface EmailRequest {
  destinatario: string;
  parametros: InformeDto;
}

@Injectable({
  providedIn: 'root'
})
export class ReporteService {
  //private apiUrl = environment.apiUrl + '/Reporte';
  private apiUrl = environment.apiUrl + '/ReportQuest';

  constructor(private http: HttpClient) {}

  descargarReporte(data: InformeDto): Observable<Blob> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.post(`${this.apiUrl}/descargar-quest`, data, {
      headers,
      responseType: 'blob'
    });
  }

  enviarCorreo(data: EmailRequest): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.post(`${this.apiUrl}/enviar-correo-quest`, data, {
      headers,
      responseType: 'text'
    });
  }
}
