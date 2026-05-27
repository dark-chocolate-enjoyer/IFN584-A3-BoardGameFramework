using System;
using System.Collections.Generic;
using System.IO;

namespace BoardGameFramework
{
    public class ConnectFour : Game
    {
        private const int WinLength = 4;
        private const string DefaultSaveFile = "connectfour_save.txt";

        // Used when the player types a column directly at the command prompt.
        private string? pendingColumn;

        public ConnectFour(Player player1, Player player2)
            : base("Connect Four", new Board(6, 7), player1, player2)
        {
        }

        public override bool IsValidMove(Move move)
        {
            if (move == null) return false;
            if (move.Col < 0 || move.Col >= Board.Cols) return false;
            // top cell of the column being empty means there is still room
            return Board.IsEmpty(0, move.Col);
        }

        public override bool IsGameOver(out Player? winner)
        {
            // look for 4 in a row in any of the 4 directions from every cell
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    Piece? p = Board.GetPiece(r, c);
                    if (p == null) continue;

                    if (HasLineFrom(r, c, 0, 1) ||   // horizontal
                        HasLineFrom(r, c, 1, 0) ||   // vertical
                        HasLineFrom(r, c, 1, 1) ||   // diagonal
                        HasLineFrom(r, c, 1, -1))    // other diagonal 
                    {
                        winner = p.OwnerId == Player1.Id ? Player1 : Player2;
                        return true;
                    }
                }
            }

            if (Board.IsFull())
            {
                // board is full and nobody won
                winner = null;
                return true;
            }

            winner = null;
            return false;
        }

        // returns true if (row,col) starts a run of WinLength matching pieces
        // in the direction (dRow,dCol).
        private bool HasLineFrom(int row, int col, int dRow, int dCol)
        {
            Piece? first = Board.GetPiece(row, col);
            if (first == null) return false;

            // bail out early if the line would go off the board
            int endRow = row + (WinLength - 1) * dRow;
            int endCol = col + (WinLength - 1) * dCol;
            if (!Board.IsInBounds(endRow, endCol)) return false;

            for (int i = 1; i < WinLength; i++)
            {
                Piece? p = Board.GetPiece(row + i * dRow, col + i * dCol);
                if (p == null || p.OwnerId != first.OwnerId) return false;
            }
            return true;
        }

        public override Move ReadHumanMove(Player player)
        {
            // keep asking until we get a valid column number
            while (true)
            {
                string? input;

                if (pendingColumn != null)
                {
                    input = pendingColumn;
                    pendingColumn = null;
                }
                else
                {
                    Console.Write($"{player.Name}, choose a column (1-{Board.Cols}): ");
                    input = Console.ReadLine();
                }

                if (input == null) continue;

                if (int.TryParse(input.Trim(), out int col))
                {
                    int col0 = col - 1; // user input is 1-indexed
                    Piece piece = CreatePieceFor(player);
                    ConnectFourMove move = new ConnectFourMove(col0, piece);
                    if (IsValidMove(move))
                    {
                        return move;
                    }
                    Console.WriteLine("That column is full or out of range, try another.");
                }
                else
                {
                    Console.WriteLine($"Please enter a number from 1 to {Board.Cols}.");
                }
            }
        }

        public override Piece CreatePieceFor(Player player)
        {
            return new Disc(player.Id);
        }

        // only return one move per column that still has room. this is what stops the computer trying to play into the middle of the board.
        public override IEnumerable<Move> GetValidMoves(Player player)
        {
            List<Move> moves = new List<Move>();
            for (int c = 0; c < Board.Cols; c++)
            {
                if (Board.IsEmpty(0, c))
                {
                    Piece piece = CreatePieceFor(player);
                    moves.Add(new ConnectFourMove(c, piece));
                }
            }
            return moves;
        }

        public override string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} dropped a disc into column {move.Col + 1}.";
        }

        protected override string ReadCommand()
        {
            while (true)
            {
                if (CurrentPlayer is ComputerPlayer)
                {
                    return "move";
                }

                Console.Write($"{CurrentPlayer.Name}, enter a column (1-{Board.Cols}), or type 'help' for options: ");
                string? input = Console.ReadLine();
                if (input == null) return "quit";

                string trimmed = input.Trim();
                string lower = trimmed.ToLower();

                if (lower == "help" || lower == "h")
                {
                    return "help";
                }
                if (lower == "undo" || lower == "u")
                {
                    return "undo";
                }
                if (lower == "redo" || lower == "r")
                {
                    return "redo";
                }
                if (lower == "quit" || lower == "q")
                {
                    return "quit";
                }
                if (lower == "save" || lower == "s")
                {
                    SaveGame();
                    continue;
                }
                if (lower == "load" || lower == "l")
                {
                    LoadGame();
                    Board.Display();
                    continue;
                }

                pendingColumn = trimmed;
                return "move";
            }
        }

        protected override void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  1-7  - drop a disc into that column");
            Console.WriteLine("  undo - undo the last drop");
            Console.WriteLine("  redo - redo a drop you undid");
            Console.WriteLine("  save - save this Connect Four game to a text file");
            Console.WriteLine("  load - load this Connect Four game from a text file");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
            Console.WriteLine("Discs fall to the lowest empty space in the chosen column.");
            Console.WriteLine();
        }

        private void SaveGame()
        {
            string path = AskFileName("save to");

            try
            {
                List<string> lines = new List<string>();
                lines.Add(GameType.Replace("-", "").Replace(" ", ""));
                lines.Add($"{Board.Rows} {Board.Cols}");
                lines.Add(CurrentPlayer.Id.ToString());

                for (int r = 0; r < Board.Rows; r++)
                {
                    string[] cells = new string[Board.Cols];
                    for (int c = 0; c < Board.Cols; c++)
                    {
                        Piece? piece = Board.GetPiece(r, c);
                        cells[c] = piece == null ? "." : piece.Symbol;
                    }
                    lines.Add(string.Join(" ", cells));
                }

                File.WriteAllLines(path, lines);
                Console.WriteLine($"Game saved to '{path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not save the game: {ex.Message}");
            }
        }

        private void LoadGame()
        {
            string path = AskFileName("load from");

            if (!File.Exists(path))
            {
                Console.WriteLine($"No save file found at '{path}'.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(path);
                string expected = GameType.Replace("-", "").Replace(" ", "");

                if (lines.Length < 3 || lines[0].Trim() != expected)
                {
                    Console.WriteLine("That file is not a valid Connect Four save.");
                    return;
                }

                string[] dims = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (dims.Length != 2 ||
                    !int.TryParse(dims[0], out int rows) ||
                    !int.TryParse(dims[1], out int cols) ||
                    rows != Board.Rows || cols != Board.Cols)
                {
                    Console.WriteLine("That save file does not match this board size.");
                    return;
                }

                if (!int.TryParse(lines[2].Trim(), out int turnId) ||
                    (turnId != Player1.Id && turnId != Player2.Id))
                {
                    Console.WriteLine("That save file has an invalid turn marker.");
                    return;
                }

                if (lines.Length < 3 + rows)
                {
                    Console.WriteLine("That save file is incomplete.");
                    return;
                }

                string[,] cellsToLoad = new string[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    string[] tokens = lines[3 + r].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length != cols)
                    {
                        Console.WriteLine("That save file is incomplete.");
                        return;
                    }

                    for (int c = 0; c < cols; c++)
                    {
                        string token = tokens[c];
                        if (token != "." && token != "R" && token != "Y")
                        {
                            Console.WriteLine("That save file contains an unknown symbol.");
                            return;
                        }
                        cellsToLoad[r, c] = token;
                    }
                }

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        Board.RemovePiece(r, c);

                        if (cellsToLoad[r, c] == "R")
                        {
                            Board.PlacePiece(r, c, CreatePieceFor(Player1));
                        }
                        else if (cellsToLoad[r, c] == "Y")
                        {
                            Board.PlacePiece(r, c, CreatePieceFor(Player2));
                        }
                    }
                }

                CurrentPlayer = turnId == Player1.Id ? Player1 : Player2;
                done.Clear();
                redo.Clear();
                pendingColumn = null;

                Console.WriteLine($"Game loaded from '{path}'. It's {CurrentPlayer.Name}'s turn.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load the game: {ex.Message}");
            }
        }

        private string AskFileName(string action)
        {
            Console.Write($"Enter a filename to {action} (press Enter for '{DefaultSaveFile}'): ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return DefaultSaveFile;
            return input.Trim();
        }
    }
}
