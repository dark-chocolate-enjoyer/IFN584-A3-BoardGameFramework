using System;
using System.Collections.Generic;

namespace BoardGameFramework
{
    public abstract class Game
    {
        public string GameType { get; }
        public Board Board { get; protected set; }
        public Player Player1 { get; }
        public Player Player2 { get; }
        public Player CurrentPlayer { get; protected set; }

        // Stacks for undo and redo. Done holds moves already applied;
        // Redo holds moves that have been undone and can be replayed.
        protected Stack<Move> done = new Stack<Move>();
        protected Stack<Move> redo = new Stack<Move>();

        protected Game(string gameType, Board board, Player player1, Player player2)
        {
            GameType = gameType;
            Board = board;
            Player1 = player1;
            Player2 = player2;
            CurrentPlayer = player1;
        }

        // Main game loop. Keeps running until someone wins or the board is full.
        public virtual void Play()
        {
            while (true)
            {
                Board.Display();

                if (IsGameOver(out Player? winner))
                {
                    AnnounceResult(winner);
                    return;
                }

                Console.WriteLine($"{CurrentPlayer.Name}'s turn.");
                string command = ReadCommand();

                if (command == "move")
                {
                    Move move = CurrentPlayer.GetMove(this);
                    ApplyMove(move);
                }
                else if (command == "undo")
                {
                    UndoMove();
                }
                else if (command == "redo")
                {
                    RedoMove();
                }
                else if (command == "save")
                {
                    SaveLoadManager.SaveGameFromMenu(this);
                }
                else if (command == "load")
                {
                    Game? loadedGame = SaveLoadManager.LoadGameFromMenu();
                    if (loadedGame != null)
                    {
                        loadedGame.Play();
                        return;
                    }
                }

                else if (command == "help")
                {
                    ShowHelp();
                }
                else if (command == "quit")
                {
                    Console.WriteLine("Game ended early.");
                    return;
                }

            }
        }

        // Apply a move and switch turn. Clears redo because a new branch starts here.
        public void ApplyMove(Move move)
        {
            move.Execute(Board);
            Console.WriteLine(GetMoveDescription(move));
            done.Push(move);
            redo.Clear();
            SwitchPlayer();
        }

        public void UndoMove()
        {
            if (done.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }
            Move last = done.Pop();
            last.Undo(Board);
            redo.Push(last);
            SwitchPlayer();
        }

        public void RedoMove()
        {
            if (redo.Count == 0)
            {
                Console.WriteLine("Nothing to redo.");
                return;
            }
            Move next = redo.Pop();
            next.Execute(Board);
            done.Push(next);
            SwitchPlayer();
        }

        // default check: try every empty cell as a possible move for this player.
        // Games with different rules (number pieces, gravity) override this.
        public virtual IEnumerable<Move> GetValidMoves(Player player)
        {
            List<Move> moves = new List<Move>();
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    if (Board.IsEmpty(r, c))
                    {
                        Piece piece = CreatePieceFor(player);
                        moves.Add(new Move(r, c, piece));
                    }
                }
            }
            return moves;
        }

        // Simulate the move, check if it wins, then take it back. Most games shouldnt need to override this I thin.
        public virtual bool IsWinningMove(Move move, Player player)
        {
            Move test = move.Clone();
            test.Execute(Board);

            bool wins = IsGameOver(out Player? winner) && winner == player;

            test.Undo(Board);
            return wins;
        }

        // Default printout. Games can override for nicer messages.
        public virtual string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed {move.Piece.Symbol} at ({move.Row + 1}, {move.Col + 1}).";
        }

        protected void SwitchPlayer()
        {
            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
            }
            else
            {
                CurrentPlayer = Player1;
            }
        }

        protected virtual void AnnounceResult(Player? winner)
        {
            if (winner == null)
            {
                Console.WriteLine("Game over. It's a draw.");
            }
            else
            {
                Console.WriteLine($"Game over. {winner.Name} wins!");
            }
        }

        // Each game does its own prompt: pick a basic command first. Individual games can override for specific rules and such.
        protected virtual string ReadCommand()
        {
            Console.Write("Enter command (move / undo / redo / save / load / help / quit): ");
            string? input = Console.ReadLine();
            if (input == null) return "";
            return input.Trim().ToLower();
        }

        protected virtual void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  move - make a move");
            Console.WriteLine("  undo - undo the last move");
            Console.WriteLine("  redo - redo a move you undo'd");
            Console.WriteLine("  save - save the current game as .txt or .json");
            Console.WriteLine("  load - load a saved game from .txt or .json");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
        }



        public virtual GameSaveData CreateSaveData()
        {
            return new GameSaveData
            {
                GameType = GameType,
                Rows = Board.Rows,
                Cols = Board.Cols,
                CurrentPlayerId = CurrentPlayer.Id,
                Player1Type = GetPlayerType(Player1),
                Player2Type = GetPlayerType(Player2),
                Boards = CreateBoardSaveData(),
                DoneMoves = CreateMoveSaveData(done, chronological: true),
                RedoMoves = CreateMoveSaveData(redo, chronological: false)
            };
        }

        public virtual void LoadSaveData(GameSaveData saveData)
        {
            if (saveData.Boards.Count == 0)
            {
                throw new InvalidOperationException("Save file does not contain board data.");
            }

            RestoreBoard(Board, saveData.Boards[0]);
            CurrentPlayer = saveData.CurrentPlayerId == Player2.Id ? Player2 : Player1;

            done.Clear();
            foreach (MoveSaveData moveData in saveData.DoneMoves)
            {
                done.Push(CreateMoveFromSaveData(moveData));
            }

            redo.Clear();
            for (int i = saveData.RedoMoves.Count - 1; i >= 0; i--)
            {
                redo.Push(CreateMoveFromSaveData(saveData.RedoMoves[i]));
            }
        }

        protected virtual List<BoardSaveData> CreateBoardSaveData()
        {
            return new List<BoardSaveData> { CreateSingleBoardSaveData(Board, 0) };
        }

        protected BoardSaveData CreateSingleBoardSaveData(Board board, int boardIndex)
        {
            BoardSaveData boardData = new BoardSaveData
            {
                BoardIndex = boardIndex,
                Rows = board.Rows,
                Cols = board.Cols
            };

            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    Piece? piece = board.GetPiece(r, c);
                    if (piece != null)
                    {
                        boardData.Cells.Add(new CellSaveData
                        {
                            Row = r,
                            Col = c,
                            OwnerId = piece.OwnerId,
                            Symbol = piece.Symbol
                        });
                    }
                }
            }

            return boardData;
        }

        protected void RestoreBoard(Board board, BoardSaveData boardData)
        {
            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    board.RemovePiece(r, c);
                }
            }

            foreach (CellSaveData cell in boardData.Cells)
            {
                Player owner = cell.OwnerId == Player2.Id ? Player2 : Player1;
                board.PlacePiece(cell.Row, cell.Col, CreatePieceFor(owner));
            }
        }

        protected List<MoveSaveData> CreateMoveSaveData(Stack<Move> stack, bool chronological)
        {
            List<Move> moves = new List<Move>(stack);

            if (chronological)
            {
                moves.Reverse();
            }

            List<MoveSaveData> moveData = new List<MoveSaveData>();
            foreach (Move move in moves)
            {
                moveData.Add(new MoveSaveData
                {
                    Row = move.Row,
                    Col = move.Col,
                    BoardIndex = move.BoardIndex,
                    OwnerId = move.Piece.OwnerId,
                    Symbol = move.Piece.Symbol
                });
            }

            return moveData;
        }

        protected virtual Move CreateMoveFromSaveData(MoveSaveData moveData)
        {
            Player owner = moveData.OwnerId == Player2.Id ? Player2 : Player1;
            return new Move(moveData.Row, moveData.Col, CreatePieceFor(owner), moveData.BoardIndex);
        }

        protected string GetPlayerType(Player player)
        {
            return player is ComputerPlayer ? "Computer" : "Human";
        }

        // Each game defines its own rules for the following functions, you can add more if you see fit.
        public abstract bool IsValidMove(Move move);
        public abstract bool IsGameOver(out Player? winner);
        public abstract Move ReadHumanMove(Player player);
        public abstract Piece CreatePieceFor(Player player);
    }
}