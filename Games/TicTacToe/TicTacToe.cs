using System;
using System.Collections.Generic;
using System.IO;

namespace BoardGameFramework
{
    public class TicTacToe : Game
    {
        private static readonly int[][] WinningLines = new int[][]
        {
            new[] { 0,0, 0,1, 0,2 },
            new[] { 1,0, 1,1, 1,2 },
            new[] { 2,0, 2,1, 2,2 },
            new[] { 0,0, 1,0, 2,0 },
            new[] { 0,1, 1,1, 2,1 },
            new[] { 0,2, 1,2, 2,2 },
            new[] { 0,0, 1,1, 2,2 },
            new[] { 0,2, 1,1, 2,0 },
        };

        private const string DefaultSaveFile = "tictactoe_save.txt";

        private enum MenuResult { Continue, Quit, NewGame }

        public bool StartNewGame { get; private set; }

        private string? pendingMove;

        public TicTacToe(Player player1, Player player2)
            : base("Tic-Tac-Toe", new Board(3, 3), player1, player2)
        {
        }

        public override bool IsValidMove(Move move)
        {
            if (move == null) return false;
            if (!Board.IsInBounds(move.Row, move.Col)) return false;
            return Board.IsEmpty(move.Row, move.Col);
        }

        public override bool IsGameOver(out Player? winner)
        {
            foreach (int[] line in WinningLines)
            {
                Piece? a = Board.GetPiece(line[0], line[1]);
                Piece? b = Board.GetPiece(line[2], line[3]);
                Piece? c = Board.GetPiece(line[4], line[5]);

                if (a != null && b != null && c != null &&
                    a.OwnerId == b.OwnerId && b.OwnerId == c.OwnerId)
                {
                    winner = a.OwnerId == Player1.Id ? Player1 : Player2;
                    return true;
                }
            }

            if (Board.IsFull())
            {
                winner = null;
                return true;
            }

            winner = null;
            return false;
        }


        public override Move ReadHumanMove(Player player)
        {
            while (true)
            {
                string? input;
                if (pendingMove != null)
                {
                    input = pendingMove;
                    pendingMove = null;
                }
                else
                {
                    Console.Write($"{player.Name}, enter row and column (1-{Board.Rows}) e.g. '2 3': ");
                    input = Console.ReadLine();
                }

                if (string.IsNullOrWhiteSpace(input)) continue;

                string[] parts = input.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Please enter two numbers separated by a space or comma.");
                    continue;
                }

                if (int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
                {
                    Move move = new Move(row - 1, col - 1, CreatePieceFor(player));
                    if (IsValidMove(move))
                    {
                        return move;
                    }
                    Console.WriteLine("That square is taken or out of range, try another.");
                }
                else
                {
                    Console.WriteLine($"Please enter two numbers from 1 to {Board.Rows}.");
                }
            }
        }

        public override Piece CreatePieceFor(Player player)
        {
            if (player.Id == Player1.Id)
            {
                return new XPiece(player.Id);
            }
            return new OPiece(player.Id);
        }

        public override string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed {move.Piece.Symbol} at row {move.Row + 1}, column {move.Col + 1}.";
        }


        protected override string ReadCommand()
        {
            while (true)
            {
                if (CurrentPlayer is ComputerPlayer)
                {
                    return "move";
                }

                Console.Write($"{CurrentPlayer.Name}, enter your move as row and column (e.g. '2 3'), or type 'help' for options: ");
                string? input = Console.ReadLine();
                if (input == null) return "quit";

                string trimmed = input.Trim();
                string lower = trimmed.ToLower();

                if (lower == "undo" || lower == "u") return "undo";
                if (lower == "redo" || lower == "r") return "redo";
                if (lower == "save" || lower == "s") return "save";
                if (lower == "load" || lower == "l") return "load";
                if (lower == "quit" || lower == "q") return "quit";

                if (lower == "help" || lower == "h")
                {
                    MenuResult result = ShowGameMenu(inPlay: true);
                    if (result == MenuResult.Quit)
                    {
                        return "quit";
                    }
                    if (result == MenuResult.NewGame)
                    {

                        ResetForNewGame();
                        return "";
                    }
                    continue;
                }

                pendingMove = trimmed;
                return "move";
            }
        }

        protected override void AnnounceResult(Player? winner)
        {
            if (winner == null)
            {
                Console.WriteLine("Game over! It's a draw.");
            }
            else
            {
                Player loser = winner == Player1 ? Player2 : Player1;
                Console.WriteLine($"Game over! {winner.Name} wins! {loser.Name} loses.");
            }

            if (Player1 is HumanPlayer || Player2 is HumanPlayer)
            {
                MenuResult result = ShowGameMenu(inPlay: false);
                if (result == MenuResult.NewGame)
                {

                    StartNewGame = true;
                }
            }
        }


        private MenuResult ShowGameMenu(bool inPlay)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  (1) Save game");
                Console.WriteLine("  (2) Quit game");
                if (inPlay)
                {
                    Console.WriteLine("  (3) Load game");
                    Console.Write("Choose an option (1-3): ");
                }
                else
                {
                    Console.Write("Choose an option (1-2): ");
                }

                string? choice = Console.ReadLine();
                if (choice == null) return MenuResult.Quit;

                switch (choice.Trim().ToLower())
                {
                    case "1":
                    case "save":
                    case "s":
                        SaveLoadManager.SaveGameFromMenu(this);
                        return ShowPostSaveMenu();
                    case "2":
                    case "quit":
                    case "q":
                        return MenuResult.Quit;
                    case "3":
                    case "load":
                    case "l":
                        if (inPlay)
                        {
                            Game? loadedGame = SaveLoadManager.LoadGameFromMenu();
                            if (loadedGame != null)
                            {
                                loadedGame.Play();
                                return MenuResult.Quit;
                            }
                            Board.Display();
                            return MenuResult.Continue;
                        }
                        Console.WriteLine("Please choose 1 or 2.");
                        break;
                    default:
                        Console.WriteLine(inPlay ? "Please choose 1, 2, or 3." : "Please choose 1 or 2.");
                        break;
                }
            }
        }

        private MenuResult ShowPostSaveMenu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  (1) New game");
                Console.WriteLine("  (2) Quit game");
                Console.Write("Choose an option (1-2): ");

                string? choice = Console.ReadLine();
                if (choice == null) return MenuResult.Quit;

                switch (choice.Trim().ToLower())
                {
                    case "1":
                    case "new":
                    case "n":
                        return MenuResult.NewGame;
                    case "2":
                    case "quit":
                    case "q":
                        return MenuResult.Quit;
                    default:
                        Console.WriteLine("Please choose 1 or 2.");
                        break;
                }
            }
        }


        private void ResetForNewGame()
        {
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    Board.RemovePiece(r, c);
                }
            }
            CurrentPlayer = Player1;
            done.Clear();
            redo.Clear();
            pendingMove = null;
            StartNewGame = false;
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
                    Console.WriteLine("That file is not a valid Tic-Tac-Toe save.");
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
                        if (token != "." && token != "X" && token != "O")
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
                        if (cellsToLoad[r, c] == "X")
                        {
                            Board.PlacePiece(r, c, CreatePieceFor(Player1));
                        }
                        else if (cellsToLoad[r, c] == "O")
                        {
                            Board.PlacePiece(r, c, CreatePieceFor(Player2));
                        }
                    }
                }

                CurrentPlayer = turnId == Player1.Id ? Player1 : Player2;
                done.Clear();
                redo.Clear();

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
