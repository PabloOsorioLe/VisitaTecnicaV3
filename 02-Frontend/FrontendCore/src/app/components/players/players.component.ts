import { Component, OnInit } from '@angular/core';
import { SportsdbService } from '../../services/sportsdb.service';

@Component({
  selector: 'app-players',
  templateUrl: './players.component.html',
  styleUrls: ['./players.component.css']
})
export class PlayersComponent implements OnInit {

  players: any[] = [];
  teamId = '133739'; // Ejemplo: ID del equipo (por ejemplo, FC Barcelona)

  constructor(private sportsdbService: SportsdbService) { }

  ngOnInit(): void {
    this.sportsdbService.getPlayersByTeam(this.teamId).subscribe(response => {
      this.players = response.player || [];
    }, error => {
      console.error('Error fetching players', error);
    });
  }

}
