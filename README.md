# Tic Tac Toe

Browser-based Tic Tac Toe with an Angular 21 frontend and a .NET 10 Web API backend. The backend is the source of truth for game sessions, moves, validation, game status, computer play, undo, and scoreboard state.

## Tech Stack

- Angular 21 + TypeScript
- ASP.NET Core Web API on .NET 10
- EF Core 10 with SQLite
- xUnit for backend unit tests
- Angular unit tests with Vitest
- Bootstrap and Font Awesome

## Features Implemented

- 3 x 3 Tic Tac Toe board
- Two Player mode
- Play Against Computer mode
- Backend-owned game session state
- Current player display
- Valid move handling
- Invalid move rejection
- Move outside board validation
- Occupied cell validation
- Wrong player validation
- Move after completion validation
- Row, column, and diagonal win detection
- Winning cell highlighting
- Draw detection
- Move history with move number, player, and position
- Undo Last Move
- Computer-mode undo removes the human/computer pair
- Undo disabled after win or draw
- Session-level scoreboard for X wins, O wins, and draws
- Scoreboard updates only once per completed game
- Reset Game keeps scoreboard unchanged
- Reset Scoreboard clears scoreboard
- Basic computer move priority: win, block, center, corner, any available cell
- Responsive Angular UI for laptop browser review
- Backend xUnit tests for core game logic
- Frontend tests for rendering, actions, and API integration points

## Project Structure

```text
TicTacToe.Api/          .NET 10 Web API
TicTacToe.Api.Tests/    xUnit backend tests
tic-tac-toe-client/     Angular 21 frontend
coding-conventions.md   Project coding conventions
```

## Run The Backend

```powershell
dotnet run --project TicTacToe.Api --launch-profile http
```

The API runs at:

```text
http://localhost:5127
```

Swagger UI is available in Development at:

```text
http://localhost:5127/swagger
```

SQLite uses:

```text
TicTacToe.Api/tictactoe.db
```

The database and scoreboard row are created automatically on API startup with `Database.EnsureCreated()`. No manual migration command is required for this submission.

## Run The Frontend

```powershell
cd tic-tac-toe-client
npm install
npm start
```

The Angular app runs at:

```text
http://localhost:4200
```

The frontend calls:

```text
http://localhost:5127/api
```

## API Endpoints

Use Swagger UI at `http://localhost:5127/swagger` to view request models and test endpoints manually.

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/api/games` | Create a new game session |
| GET | `/api/games/{id}` | Get current game state |
| POST | `/api/games/{id}/moves` | Submit a player move |
| POST | `/api/games/{id}/undo` | Undo last move |
| POST | `/api/games/{id}/reset` | Reset current game |
| GET | `/api/scoreboard` | Get scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

Create game request:

```json
{
  "gameMode": "TwoPlayer"
}
```

Move request:

```json
{
  "gameId": 1,
  "player": "X",
  "row": 0,
  "column": 0
}
```

`gameId` identifies the game session and links moves to that session. It is not game status. `gameStatus` is `InProgress`, `Won`, or `Draw`.

## Run Tests

Backend:

```powershell
dotnet test TicTacToe.sln
```

Frontend:

```powershell
cd tic-tac-toe-client
npm test
```

Build checks:

```powershell
dotnet build TicTacToe.sln
cd tic-tac-toe-client
npm run build
```

## Design Decisions

- SQLite was used instead of in-memory storage so games and scoreboard survive API restarts.
- The backend owns all game rules and returns complete game state after every action.
- Undo is disabled after win or draw, so scoreboard results remain final and do not need rollback.
- The Angular app renders the latest backend response and does not duplicate game-rule decisions.
- Computer mode keeps the human as `X` and the computer as `O`.
- The API uses route `{id}` plus body `gameId` for moves, and validates that they match.

## AI Workflow Notes

- The problem statement was extracted from `Round 2 - Problem Statement.docx`.
- The implementation plan was refined to use Angular 21, .NET 10, SQLite, and xUnit.
- AI assistance was used to draft the plan, scaffold the projects, implement backend/frontend code, and create tests.
- Manual review focused on game-state ownership, invalid move behavior, scoreboard update-once behavior, computer move priority, and route/body `gameId` consistency.

## Clarifications And Assumptions

- Angular version is Angular 21.
- Backend target is .NET 10.
- Backend unit testing uses xUnit.
- SQLite is the selected storage approach.
- Rows and columns are zero-based in the API.
- Undo after completion is disabled.
- Reset Game starts the same game session fresh and keeps scoreboard unchanged.
- Reset Scoreboard is separate from Reset Game.

## Known Limitations

- There is one global scoreboard, not a per-user scoreboard.
- There is no authentication.
- There is no online multiplayer or real-time sync.
- The SQLite database is created with `EnsureCreated()` rather than EF migrations.
- The Angular API base URL is configured directly in `GameService` for local review.

## Future Improvements

- Add EF Core migrations for production-style database evolution.
- Add environment-based Angular API URLs.
- Add API integration tests with a real test server.
- Add named players.
- Add scoreboard history per game.
- Add difficulty levels for computer mode.
