import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Visita } from '../models/visita.model'; // <--- Importa desde models
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VisitasService {
  //private apiUrl = 'https://localhost:7212/api/Visitas';
  //private apiUrl = 'http://192.168.1.88:5206/api/Visitas';
  private apiUrl = environment.apiUrl + '/Visitas';
 

  constructor(private http: HttpClient) {}

  getVisitas(): Observable<Visita[]> {
    return this.http.get<Visita[]>(this.apiUrl);
  }

  postVisita(visita: Visita): Observable<Visita> {
    return this.http.post<Visita>(this.apiUrl, visita);
  }

  deleteVisita(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // Y puedes agregar putVisita si quieres edición
}
