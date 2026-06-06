import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CellModel } from './_models/game';
import { GameService } from './_services/game.service';

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  gameService = inject(GameService);
  selectedMode = 'TwoPlayer';
  loading = false;
  undoLocked = false;

  ngOnInit(): void {
    this.createGame();
  }

  createGame() {
    this.loading = true;
    this.gameService.createGame(this.selectedMode).subscribe({
      next: _ => this.undoLocked = false,
      error: _ => this.loading = false,
      complete: () => this.loading = false
    })
  }

  makeMove(row: number, column: number) {
    var game = this.gameService.currentGame();

    if (!game || this.loading || game.gameStatus !== 'InProgress' || game.board[row][column])
      return;

    this.loading = true;
    this.gameService.makeMove(game.gameId, game.currentPlayer, row, column).subscribe({
      next: _ => this.undoLocked = false,
      error: _ => this.loading = false,
      complete: () => this.loading = false
    })
  }

  undo() {
    var game = this.gameService.currentGame();

    if (!game || this.isUndoDisabled())
      return;

    this.loading = true;
    this.gameService.undo(game.gameId).subscribe({
      next: _ => this.undoLocked = true,
      error: _ => this.loading = false,
      complete: () => this.loading = false
    })
  }

  resetGame() {
    var game = this.gameService.currentGame();

    if (!game || this.loading)
      return;

    this.loading = true;
    this.gameService.resetGame(game.gameId).subscribe({
      next: _ => this.undoLocked = false,
      error: _ => this.loading = false,
      complete: () => this.loading = false
    })
  }

  resetScoreboard() {
    this.loading = true;
    this.gameService.resetScoreboard().subscribe({
      next: _ => { },
      error: _ => this.loading = false,
      complete: () => this.loading = false
    })
  }

  isWinningCell(row: number, column: number) {
    var game = this.gameService.currentGame();
    return game?.winningCells.some((cell: CellModel) => cell.row === row && cell.column === column) ?? false;
  }

  cellLabel(row: number, column: number) {
    return `Row ${row + 1}, Column ${column + 1}`;
  }

  statusMessage() {
    var game = this.gameService.currentGame();

    if (!game)
      return 'Loading';

    if (game.gameStatus === 'Won')
      return `Player ${game.winner} wins`;

    if (game.gameStatus === 'Draw')
      return 'Game drawn';

    return `Player ${game.currentPlayer} turn`;
  }

  statusClass() {
    var game = this.gameService.currentGame();

    if (!game || game.gameStatus === 'InProgress')
      return 'turn-status';

    if (game.gameStatus === 'Won')
      return 'win-status';

    return 'draw-status';
  }

  statusIcon() {
    var game = this.gameService.currentGame();

    if (!game || game.gameStatus === 'InProgress')
      return 'fa-solid fa-user-clock';

    if (game.gameStatus === 'Won')
      return 'fa-solid fa-trophy';

    return 'fa-solid fa-handshake';
  }

  isUndoDisabled() {
    var game = this.gameService.currentGame();
    return this.loading || !game || !game.canUndo || game.gameStatus !== 'InProgress' || this.undoLocked;
  }
}
