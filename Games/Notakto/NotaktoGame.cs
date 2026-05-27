using System;
using System.Collections.Generic;
using System.IO;

namespace BoardGameFramework
{
    public class NotaktoGame : Game
    {
        private Board[] subBoards;
        private bool[] boardDead;
        private Player? loser;
        private NotaktoBoard displayBoard;

        public NotaktoGame(Player player1, Player player2)
            : base("Notakto", new NotaktoBoard(), player1, player2)
        {
            subBoards = new Board[3];
            boardDead = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                subBoards[i] = new Board(3, 3);
                boardDead[i] = false;
            }
            displayBoard = (NotaktoBoard)Board;
            displayBoard.SetGame(this);
            loser = null;
        }

        public void DisplayAllBoards()
        {
            Console.WriteLine();
            for (int i = 0; i < 3; i++)
            {
                if (boardDead[i])
                    Console.WriteLine($"Board {i + 1}: DEAD");
                else
                {
                    Console.WriteLine($"Board {i + 1}:");
                    subBoards[i].Display();
                }
            }
        }

        private bool CheckThreeInARow(Board board)
        {
            for (int r = 0; r < 3; r++)
                if (board.GetPiece(r, 0) != null &&
                    board.GetPiece(r, 1) != null &&
                    board.GetPiece(r, 2) != null)
                    return true;
            for (int c = 0; c < 3; c++)
                if (board.GetPiece(0, c) != null &&
                    board.GetPiece(1, c) != null &&
                    board.GetPiece(2, c) != null)
                    return true;
            if (board.GetPiece(0, 0) != null &&
                board.GetPiece(1, 1) != null &&
                board.GetPiece(2, 2) != null)
                return true;
            if (board.GetPiece(0, 2) != null &&
                board.GetPiece(1, 1) != null &&
                board.GetPiece(2, 0) != null)
                return true;
            return false;
        }

        public override bool IsValidMove(Move move)
        {
            int bi = move.BoardIndex;
            if (bi < 0 || bi > 2) return false;
            if (boardDead[bi]) return false;
            if (!subBoards[bi].IsInBounds(move.Row, move.Col)) return false;
            if (!subBoards[bi].IsEmpty(move.Row, move.Col)) return false;
            return true;
        }

        public override bool IsGameOver(out Player? winner)
        {
            if (boardDead[0] && boardDead[1] && boardDead[2])
            {
                winner = loser == Player1 ? Player2 : Player1;
                return true;
            }
            winner = null;
            return false;
        }

        public override Move ReadHumanMove(Player player)
        {
            int boardIndex = -1;
            while (boardIndex < 0)
            {
                Console.Write("Pick a board (1, 2 or 3): ");
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int bi))
                {
                    bi = bi - 1;
                    if (bi >= 0 && bi <= 2)
                    {
                        if (boardDead[bi])
                            Console.WriteLine("That board is dead! Pick another.");
                        else
                            boardIndex = bi;
                    }
                    else
                        Console.WriteLine("Please enter 1, 2 or 3!");
                }
                else
                    Console.WriteLine("Please enter a number!");
            }

            int row = -1, col = -1;
            while (row < 0)
            {
                Console.Write("Pick a position (1-9): ");
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int pos))
                {
                    pos = pos - 1;
                    if (pos >= 0 && pos <= 8)
                    {
                        row = pos / 3;
                        col = pos % 3;
                        if (!subBoards[boardIndex].IsEmpty(row, col))
                        {
                            Console.WriteLine("That spot is taken!");
                            row = -1;
                        }
                    }
                    else
                        Console.WriteLine("Please enter 1 to 9!");
                }
                else
                    Console.WriteLine("Please enter a number!");
            }
            return new NotaktoMove(row, col, CreatePieceFor(player), boardIndex);
        }

        public override Piece CreatePieceFor(Player player)
        {
            return new NotaktoPiece(player.Id);
        }

        public override IEnumerable<Move> GetValidMoves(Player player)
        {
            List<Move> moves = new List<Move>();
            for (int bi = 0; bi < 3; bi++)
            {
                if (boardDead[bi]) continue;
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        if (subBoards[bi].IsEmpty(r, c))
                            moves.Add(new NotaktoMove(r, c,
                                CreatePieceFor(player), bi));
            }
            return moves;
        }

        public override string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed X on Board {move.BoardIndex + 1} at position ({move.Row + 1}, {move.Col + 1})";
        }

        public override void ApplyMove(Move move)
        {
            int bi = move.BoardIndex;
            subBoards[bi].PlacePiece(move.Row, move.Col, move.Piece);
            Console.WriteLine(GetMoveDescription(move));
            DisplayAllBoards();
            if (CheckThreeInARow(subBoards[bi]))
            {
                boardDead[bi] = true;
                loser = CurrentPlayer;
                Console.WriteLine($"Board {bi + 1} is now DEAD!");
            }
            done.Push(move);
            redo.Clear();
            SwitchPlayer();
        }

        public override void UndoMove()
        {
            if (done.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }
            Move last = done.Pop();
            int bi = last.BoardIndex;
            subBoards[bi].RemovePiece(last.Row, last.Col);
            boardDead[bi] = CheckThreeInARow(subBoards[bi]);
            redo.Push(last);
            SwitchPlayer();
            DisplayAllBoards();
        }

        public override void RedoMove()
        {
            if (redo.Count == 0)
            {
                Console.WriteLine("Nothing to redo.");
                return;
            }
            Move next = redo.Pop();
            ApplyMove(next);
        }

        protected override void AnnounceResult(Player? winner)
        {
            DisplayAllBoards();
            Console.WriteLine("All boards are dead!");
            if (winner != null)
                Console.WriteLine($"Game over. {winner.Name} WINS!");
            else
                Console.WriteLine("Game over. It is a draw.");
        }

        protected override void ShowHelp()
        {
            base.ShowHelp();
            Console.WriteLine("Notakto rules:");
            Console.WriteLine("  Both players place X on any of 3 boards");
            Console.WriteLine("  When a board gets 3 in a row it is DEAD");
            Console.WriteLine("  The player who finishes the LAST board LOSES!");
            Console.WriteLine();
        }

        protected override void SaveText()
        {
            Console.Write("Enter filename to save (press Enter for 'notakto_save.txt'): ");
            string? input = Console.ReadLine();
            string path = string.IsNullOrWhiteSpace(input) ? "notakto_save.txt" : input.Trim();

            try
            {
                List<string> lines = new List<string>();
                lines.Add("Notakto");
                lines.Add(CurrentPlayer.Id.ToString());
                lines.Add($"{boardDead[0]} {boardDead[1]} {boardDead[2]}");

                for (int bi = 0; bi < 3; bi++)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        string[] cells = new string[3];
                        for (int c = 0; c < 3; c++)
                        {
                            Piece? p = subBoards[bi].GetPiece(r, c);
                            cells[c] = p == null ? "." : "X";
                        }
                        lines.Add(string.Join(" ", cells));
                    }
                }

                File.WriteAllLines(path, lines);
                Console.WriteLine($"Game saved to '{path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not save: {ex.Message}");
            }
        }

        protected override void LoadText()
        {
            Console.Write("Enter filename to load (press Enter for 'notakto_save.txt'): ");
            string? input = Console.ReadLine();
            string path = string.IsNullOrWhiteSpace(input) ? "notakto_save.txt" : input.Trim();

            if (!File.Exists(path))
            {
                Console.WriteLine($"No save file found at '{path}'.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(path);

                if (lines[0].Trim() != "Notakto")
                {
                    Console.WriteLine("That is not a valid Notakto save file.");
                    return;
                }

                int turnId = int.Parse(lines[1].Trim());
                CurrentPlayer = turnId == Player1.Id ? Player1 : Player2;

                string[] deadFlags = lines[2].Trim().Split(' ');
                for (int bi = 0; bi < 3; bi++)
                    boardDead[bi] = bool.Parse(deadFlags[bi]);

                int lineIndex = 3;
                for (int bi = 0; bi < 3; bi++)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        string[] cells = lines[lineIndex].Trim().Split(' ');
                        for (int c = 0; c < 3; c++)
                        {
                            subBoards[bi].RemovePiece(r, c);
                            if (cells[c] == "X")
                                subBoards[bi].PlacePiece(r, c, CreatePieceFor(Player1));
                        }
                        lineIndex++;
                    }
                }

                done.Clear();
                redo.Clear();
                Console.WriteLine($"Game loaded from '{path}'. It's {CurrentPlayer.Name}'s turn.");
                DisplayAllBoards();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load: {ex.Message}");
            }
        }

        private void DisplayAllBoards_Internal() { }
    }

    public class NotaktoBoard : Board
    {
        private NotaktoGame? game;

        public NotaktoBoard() : base(3, 3) { }

        public void SetGame(NotaktoGame g) { game = g; }

        public override void Display()
        {
            game?.DisplayAllBoards();
        }
    }
}