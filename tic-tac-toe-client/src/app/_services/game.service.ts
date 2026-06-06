import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { GameStateResponse, HttpResponse, ScoreboardResponse } from '../_models/game';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private http = inject(HttpClient);
  baseUrl = 'http://localhost:5127/api';
  currentGame = signal<GameStateResponse | null>(null);
  scoreboard = signal<ScoreboardResponse>({ xWins: 0, oWins: 0, draws: 0 });

  createGame(gameMode: string) {
    return this.http.post<HttpResponse<GameStateResponse>>(`${this.baseUrl}/games`, { gameMode }).pipe(
      tap(response => this.setGame(response.data))
    );
  }

  getGame(gameId: number) {
    return this.http.get<HttpResponse<GameStateResponse>>(`${this.baseUrl}/games/${gameId}`).pipe(
      tap(response => this.setGame(response.data))
    );
  }

  makeMove(gameId: number, player: string, row: number, column: number) {
    return this.http.post<HttpResponse<GameStateResponse>>(`${this.baseUrl}/games/${gameId}/moves`, { gameId, player, row, column }).pipe(
      tap(response => this.setGame(response.data))
    );
  }

  undo(gameId: number) {
    return this.http.post<HttpResponse<GameStateResponse>>(`${this.baseUrl}/games/${gameId}/undo`, {}).pipe(
      tap(response => this.setGame(response.data))
    );
  }

  resetGame(gameId: number) {
    return this.http.post<HttpResponse<GameStateResponse>>(`${this.baseUrl}/games/${gameId}/reset`, {}).pipe(
      tap(response => this.setGame(response.data))
    );
  }

  getScoreboard() {
    return this.http.get<HttpResponse<ScoreboardResponse>>(`${this.baseUrl}/scoreboard`).pipe(
      tap(response => this.scoreboard.set(response.data))
    );
  }

  resetScoreboard() {
    return this.http.post<HttpResponse<ScoreboardResponse>>(`${this.baseUrl}/scoreboard/reset`, {}).pipe(
      tap(response => {
        this.scoreboard.set(response.data);
        this.currentGame.update(game => game ? { ...game, scoreboard: response.data } : game);
      })
    );
  }

  private setGame(game: GameStateResponse) {
    this.currentGame.set(game);
    this.scoreboard.set(game.scoreboard);
  }
}
