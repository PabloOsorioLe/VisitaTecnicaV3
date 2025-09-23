import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Persona } from '../models/persona.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PersonasService {
  //private apiUrl = 'https://localhost:7212/api/Personas'; // Ajusta la URL según tu API
  //private apiUrl = 'http://192.168.1.88:5206/api/Personas'; // Ajusta la URL según tu API
  private apiUrl = environment.apiUrl + '/Personas';
  constructor(private http: HttpClient) {}

  getPersonas(): Observable<Persona[]> {
    return this.http.get<Persona[]>(this.apiUrl);
  }

  postPersona(persona: Persona): Observable<Persona> {
    return this.http.post<Persona>(this.apiUrl, persona);
  }

  deletePersona(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // Agrega putPersona si quieres edición
}
