import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GameStateResponse } from '../_models/game';
import { GameService } from './game.service';

describe('GameService', () => {
  let service: GameService;
  let httpTesting: HttpTestingController;

  const game: GameStateResponse = {
    gameId: 1,
    board: [
      ['', '', ''],
      ['', '', ''],
      ['', '', '']
    ],
    currentPlayer: 'X',
    gameMode: 'TwoPlayer',
    gameStatus: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    canUndo: false,
    scoreboard: { xWins: 0, oWins: 0, draws: 0 }
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        GameService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(GameService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should call create game API and store game state', () => {
    service.createGame('TwoPlayer').subscribe();

    const req = httpTesting.expectOne('http://localhost:5127/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ gameMode: 'TwoPlayer' });
    req.flush({ status: 201, message: 'Created Successfully', data: game });

    expect(service.currentGame()?.gameId).toBe(1);
  });

  it('should call move API with game id and move data', () => {
    service.makeMove(1, 'X', 0, 1).subscribe();

    const req = httpTesting.expectOne('http://localhost:5127/api/games/1/moves');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ gameId: 1, player: 'X', row: 0, column: 1 });
    req.flush({ status: 200, message: 'Success', data: game });

    expect(service.currentGame()?.currentPlayer).toBe('X');
  });

  it('should call reset and undo APIs', () => {
    service.undo(1).subscribe();
    service.resetGame(1).subscribe();

    const undoReq = httpTesting.expectOne('http://localhost:5127/api/games/1/undo');
    const resetReq = httpTesting.expectOne('http://localhost:5127/api/games/1/reset');

    expect(undoReq.request.method).toBe('POST');
    expect(resetReq.request.method).toBe('POST');

    undoReq.flush({ status: 200, message: 'Success', data: game });
    resetReq.flush({ status: 200, message: 'Success', data: game });
  });

  it('should call scoreboard APIs and update signals', () => {
    service.getScoreboard().subscribe();
    const getReq = httpTesting.expectOne('http://localhost:5127/api/scoreboard');
    getReq.flush({ status: 200, message: 'Fetched Successfully', data: { xWins: 1, oWins: 2, draws: 3 } });

    service.currentGame.set(game);
    service.resetScoreboard().subscribe();
    const resetReq = httpTesting.expectOne('http://localhost:5127/api/scoreboard/reset');
    resetReq.flush({ status: 200, message: 'Success', data: { xWins: 0, oWins: 0, draws: 0 } });

    expect(service.scoreboard().draws).toBe(0);
    expect(service.currentGame()?.scoreboard.xWins).toBe(0);
  });
});
