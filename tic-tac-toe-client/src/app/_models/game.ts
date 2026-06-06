export interface HttpResponse<T> {
    status: number;
    message: string;
    data: T;
}

export interface CreateGameRequest {
    gameMode: string;
}

export interface MoveRequest {
    gameId: number;
    player: string;
    row: number;
    column: number;
}

export interface CellModel {
    row: number;
    column: number;
}

export interface MoveHistoryModel {
    moveNumber: number;
    player: string;
    row: number;
    column: number;
    position: string;
}

export interface ScoreboardResponse {
    xWins: number;
    oWins: number;
    draws: number;
}

export interface GameStateResponse {
    gameId: number;
    board: string[][];
    currentPlayer: string;
    gameMode: string;
    gameStatus: string;
    winner: string | null;
    winningCells: CellModel[];
    moveHistory: MoveHistoryModel[];
    canUndo: boolean;
    scoreboard: ScoreboardResponse;
}
