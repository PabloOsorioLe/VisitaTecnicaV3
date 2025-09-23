import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SportsdbService {

  private apiKey = '123'; // Usa aquí tu API Key válida
  private baseUrl = `https://www.thesportsdb.com/api/v1/json/${this.apiKey}`;

  constructor(private http: HttpClient) { }

  // Método para obtener jugadores de un equipo por ID
  getPlayersByTeam(teamId: string): Observable<any> {
    const url = `${this.baseUrl}/lookup_all_players.php?id=${teamId}`;
    return this.http.get(url);
  }
}
