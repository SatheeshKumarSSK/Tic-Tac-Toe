import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { App } from './app';
import { GameStateResponse } from './_models/game';
import { GameService } from './_services/game.service';

describe('App', () => {
  function createGame(overrides: Partial<GameStateResponse> = {}): GameStateResponse {
    return {
      gameId: 1,
      board: [
        ['X', '', ''],
        ['', 'O', ''],
        ['', '', '']
      ],
      currentPlayer: 'X',
      gameMode: 'TwoPlayer',
      gameStatus: 'InProgress',
      winner: null,
      winningCells: [],
      moveHistory: [
        { moveNumber: 1, player: 'X', row: 0, column: 0, position: 'Row 1, Column 1' },
        { moveNumber: 2, player: 'O', row: 1, column: 1, position: 'Row 2, Column 2' }
      ],
      canUndo: true,
      scoreboard: { xWins: 2, oWins: 1, draws: 3 },
      ...overrides
    };
  }

  async function setup(game: GameStateResponse = createGame()) {
    const gameService = {
      currentGame: signal<GameStateResponse | null>(game),
      scoreboard: signal(game.scoreboard),
      createGame: vi.fn(() => of({})),
      makeMove: vi.fn(() => of({})),
      undo: vi.fn(() => of({})),
      resetGame: vi.fn(() => of({})),
      resetScoreboard: vi.fn(() => of({}))
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [{ provide: GameService, useValue: gameService }]
    }).compileComponents();

    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    return { fixture, gameService };
  }

  it('should render the board', async () => {
    const { fixture } = await setup();
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;

    expect(cells.length).toBe(9);
  });

  it('should disable occupied cells', async () => {
    const { fixture } = await setup();
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;

    expect(cells[0].disabled).toBe(true);
    expect(cells[1].disabled).toBe(false);
  });

  it('should disable moves after completion', async () => {
    const { fixture } = await setup(createGame({ gameStatus: 'Won', winner: 'X' }));
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;

    expect(cells[1].disabled).toBe(true);
  });

  it('should show current player status', async () => {
    const { fixture } = await setup(createGame({ currentPlayer: 'O' }));
    const banner = fixture.nativeElement.querySelector('.status-banner') as HTMLElement;

    expect(fixture.nativeElement.textContent).toContain('Player O turn');
    expect(banner.classList.contains('turn-status')).toBe(true);
  });

  it('should show completed status', async () => {
    const { fixture } = await setup(createGame({ gameStatus: 'Won', winner: 'X' }));
    const banner = fixture.nativeElement.querySelector('.status-banner') as HTMLElement;

    expect(fixture.nativeElement.textContent).toContain('Player X wins');
    expect(banner.classList.contains('win-status')).toBe(true);
  });

  it('should show draw status', async () => {
    const { fixture } = await setup(createGame({ gameStatus: 'Draw', winner: null }));
    const banner = fixture.nativeElement.querySelector('.status-banner') as HTMLElement;

    expect(fixture.nativeElement.textContent).toContain('Game drawn');
    expect(banner.classList.contains('draw-status')).toBe(true);
  });

  it('should highlight winning cells', async () => {
    const { fixture } = await setup(createGame({
      gameStatus: 'Won',
      winner: 'X',
      winningCells: [
        { row: 0, column: 0 },
        { row: 0, column: 1 },
        { row: 0, column: 2 }
      ]
    }));
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;

    expect(cells[0].classList.contains('winning-cell')).toBe(true);
    expect(cells[1].classList.contains('winning-cell')).toBe(true);
    expect(cells[2].classList.contains('winning-cell')).toBe(true);
  });

  it('should render move history', async () => {
    const { fixture } = await setup();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr') as NodeListOf<HTMLTableRowElement>;

    expect(fixture.nativeElement.textContent).toContain('Row 1, Column 1');
    expect(fixture.nativeElement.textContent).toContain('Row 2, Column 2');
    expect(rows[0].classList.contains('x-move-row')).toBe(true);
    expect(rows[1].classList.contains('o-move-row')).toBe(true);
  });

  it('should render scoreboard', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.textContent).toContain('X Wins');
    expect(fixture.nativeElement.textContent).toContain('2');
    expect(fixture.nativeElement.textContent).toContain('O Wins');
    expect(fixture.nativeElement.textContent).toContain('Draws');
  });

  it('should call game actions from buttons and board cells', async () => {
    const { fixture, gameService } = await setup();
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;
    const undoButton = fixture.nativeElement.querySelector('button[title="Undo last move"]') as HTMLButtonElement;
    const resetButton = fixture.nativeElement.querySelector('button[title="Reset game"]') as HTMLButtonElement;
    const resetScoreboardButton = fixture.nativeElement.querySelector('button[title="Reset scoreboard"]') as HTMLButtonElement;

    cells[1].click();
    undoButton.click();
    resetButton.click();
    resetScoreboardButton.click();

    expect(gameService.makeMove).toHaveBeenCalledWith(1, 'X', 0, 1);
    expect(gameService.undo).toHaveBeenCalledWith(1);
    expect(gameService.resetGame).toHaveBeenCalledWith(1);
    expect(gameService.resetScoreboard).toHaveBeenCalled();
  });

  it('should lock undo after one successful undo', async () => {
    const { fixture, gameService } = await setup();
    const component = fixture.componentInstance;
    const undoButton = fixture.nativeElement.querySelector('button[title="Undo last move"]') as HTMLButtonElement;

    undoButton.click();
    fixture.detectChanges();
    undoButton.click();

    expect(gameService.undo).toHaveBeenCalledTimes(1);
    expect(component.undoLocked).toBe(true);
    expect(undoButton.disabled).toBe(true);
  });

  it('should unlock undo after next successful move', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance;
    const undoButton = fixture.nativeElement.querySelector('button[title="Undo last move"]') as HTMLButtonElement;
    const cells = fixture.nativeElement.querySelectorAll('.board-cell') as NodeListOf<HTMLButtonElement>;

    undoButton.click();
    fixture.detectChanges();

    expect(component.undoLocked).toBe(true);

    cells[1].click();
    fixture.detectChanges();

    expect(component.undoLocked).toBe(false);
    expect(component.isUndoDisabled()).toBe(false);
  });

  it('should clear undo lock after reset and new game', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance;
    const undoButton = fixture.nativeElement.querySelector('button[title="Undo last move"]') as HTMLButtonElement;
    const resetButton = fixture.nativeElement.querySelector('button[title="Reset game"]') as HTMLButtonElement;
    const newGameButton = fixture.nativeElement.querySelector('button[title="New game"]') as HTMLButtonElement;

    undoButton.click();
    fixture.detectChanges();
    resetButton.click();

    expect(component.undoLocked).toBe(false);

    undoButton.click();
    fixture.detectChanges();
    newGameButton.click();

    expect(component.undoLocked).toBe(false);
  });
});
