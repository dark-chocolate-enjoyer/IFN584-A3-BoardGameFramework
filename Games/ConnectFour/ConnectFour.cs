using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BoardGameFramework
{
    // Standard Connect Four: 6 rows, 7 columns, first to 4-in-a-row wins.
    public class ConnectFour : Game
    {
        // 4-in-a-row needed to win
        private const int WinLength = 4;
        private const char Delim = ',';
        private const string EmptyCell = ".";
        private const string DefaultTextFile = "connectfour_save.txt";
        private const string DefaultJsonFile = "connectfour_save.json";

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
                        HasLineFrom(r, c, 1, 1) ||   // diagonal \
                        HasLineFrom(r, c, 1, -1))    // diagonal /
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
                Console.Write($"{player.Name}, choose a column (1-{Board.Cols}): ");
                string? input = Console.ReadLine();
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

        // only return one move per column that still has room. this is what
        // stops the computer trying to play into the middle of the board.
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
            // nicer wording than the default since the user only picked a column
            return $"{CurrentPlayer.Name} dropped a disc into column {move.Col + 1}.";
        }

        protected override void SaveText()
        {
            string fileName = AskFileName("Text save file", DefaultTextFile);

            try
            {
                SaveTextToFile(fileName);
                Console.WriteLine($"Connect Four saved to {fileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not save file: {ex.Message}");
            }
        }

        protected override void LoadText()
        {
            string fileName = AskFileName("Text load file", DefaultTextFile);

            try
            {
                LoadTextFromFile(fileName);
                Console.WriteLine($"Connect Four loaded from {fileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load file: {ex.Message}");
            }
        }

        protected override void SaveJson()
        {
            string fileName = AskFileName("JSON save file", DefaultJsonFile);

            try
            {
                SaveJsonToFile(fileName);
                Console.WriteLine($"Connect Four saved to {fileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not save JSON file: {ex.Message}");
            }
        }

        protected override void LoadJson()
        {
            string fileName = AskFileName("JSON load file", DefaultJsonFile);

            try
            {
                LoadJsonFromFile(fileName);
                Console.WriteLine($"Connect Four loaded from {fileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load JSON file: {ex.Message}");
            }
        }

        // Public versions used by the smoke test. These avoid console input.
        public void SaveTextToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine($"GameType={GameType}");
                writer.WriteLine($"CurrentPlayerId={CurrentPlayer.Id}");
                writer.WriteLine($"Rows={Board.Rows}");
                writer.WriteLine($"Cols={Board.Cols}");

                writer.WriteLine("Board");
                for (int r = 0; r < Board.Rows; r++)
                {
                    writer.WriteLine(GetBoardRowString(r));
                }

                writer.WriteLine("Moves");
                foreach (Move move in GetDoneMovesOldestFirst())
                {
                    writer.WriteLine($"{move.Piece.OwnerId}{Delim}{move.Row}{Delim}{move.Col}{Delim}{move.Piece.Symbol}");
                }
            }
        }

        public void LoadTextFromFile(string fileName)
        {
            List<string> lines = new List<string>();

            using (StreamReader reader = new StreamReader(fileName))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            ConnectFourSaveData data = ParseTextSave(lines);
            RestoreFromSaveData(data);
        }

        public void SaveJsonToFile(string fileName)
        {
            ConnectFourSaveData data = CreateSaveData();
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(fileName, json);
        }

        public void LoadJsonFromFile(string fileName)
        {
            string json = File.ReadAllText(fileName);
            ConnectFourSaveData? data = JsonSerializer.Deserialize<ConnectFourSaveData>(json);

            if (data == null)
            {
                throw new Exception("Save file was empty or invalid.");
            }

            RestoreFromSaveData(data);
        }

        private string AskFileName(string prompt, string defaultFileName)
        {
            Console.Write($"{prompt} [{defaultFileName}]: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultFileName;
            }

            return input.Trim();
        }

        private string GetBoardRowString(int row)
        {
            List<string> cells = new List<string>();
            for (int c = 0; c < Board.Cols; c++)
            {
                Piece? piece = Board.GetPiece(row, c);
                cells.Add(piece == null ? EmptyCell : piece.Symbol);
            }
            return string.Join(Delim, cells);
        }

        private ConnectFourSaveData CreateSaveData()
        {
            ConnectFourSaveData data = new ConnectFourSaveData();
            data.GameType = GameType;
            data.CurrentPlayerId = CurrentPlayer.Id;
            data.Rows = Board.Rows;
            data.Cols = Board.Cols;

            for (int r = 0; r < Board.Rows; r++)
            {
                data.BoardRows.Add(GetBoardRowString(r));
            }

            foreach (Move move in GetDoneMovesOldestFirst())
            {
                SavedMoveData savedMove = new SavedMoveData();
                savedMove.PlayerId = move.Piece.OwnerId;
                savedMove.Row = move.Row;
                savedMove.Col = move.Col;
                savedMove.Symbol = move.Piece.Symbol;
                data.DoneMoves.Add(savedMove);
            }

            return data;
        }

        private ConnectFourSaveData ParseTextSave(List<string> lines)
        {
            ConnectFourSaveData data = new ConnectFourSaveData();
            int index = 0;

            while (index < lines.Count)
            {
                string line = lines[index].Trim();

                if (line.StartsWith("GameType="))
                {
                    data.GameType = line.Substring("GameType=".Length);
                }
                else if (line.StartsWith("CurrentPlayerId="))
                {
                    int.TryParse(line.Substring("CurrentPlayerId=".Length), out int currentPlayerId);
                    data.CurrentPlayerId = currentPlayerId;
                }
                else if (line.StartsWith("Rows="))
                {
                    int.TryParse(line.Substring("Rows=".Length), out int rows);
                    data.Rows = rows;
                }
                else if (line.StartsWith("Cols="))
                {
                    int.TryParse(line.Substring("Cols=".Length), out int cols);
                    data.Cols = cols;
                }
                else if (line == "Board")
                {
                    index++;
                    for (int r = 0; r < data.Rows && index < lines.Count; r++)
                    {
                        data.BoardRows.Add(lines[index].Trim());
                        index++;
                    }
                    continue;
                }
                else if (line == "Moves")
                {
                    index++;
                    while (index < lines.Count)
                    {
                        string moveLine = lines[index].Trim();
                        if (moveLine.Length > 0)
                        {
                            SavedMoveData? move = ParseMoveLine(moveLine);
                            if (move != null)
                            {
                                data.DoneMoves.Add(move);
                            }
                        }
                        index++;
                    }
                    continue;
                }

                index++;
            }

            return data;
        }

        private SavedMoveData? ParseMoveLine(string line)
        {
            string[] parts = line.Split(Delim);
            if (parts.Length < 4) return null;

            if (!int.TryParse(parts[0], out int playerId)) return null;
            if (!int.TryParse(parts[1], out int row)) return null;
            if (!int.TryParse(parts[2], out int col)) return null;

            SavedMoveData move = new SavedMoveData();
            move.PlayerId = playerId;
            move.Row = row;
            move.Col = col;
            move.Symbol = parts[3].Trim();
            return move;
        }


        private void RestoreFromSaveData(ConnectFourSaveData data)
        {
            if (data.GameType != GameType)
            {
                Console.WriteLine("This save file is not for Connect Four.");
                return;
            }

            if (data.Rows != Board.Rows || data.Cols != Board.Cols)
            {
                Console.WriteLine("Rows and Columns do not match for this game.");
                return;
            }

            ClearBoard();
            RestoreBoard(data.BoardRows);
            RestoreCurrentPlayer(data.CurrentPlayerId);
            RestoreDoneMoves(data.DoneMoves);
            redo.Clear();
        }

        private void ClearBoard()
        {
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    
                    Board.RemovePiece(r, c);
                }
            }
        }

        private void RestoreBoard(List<string> boardRows)
        {
            for (int r = 0; r < Board.Rows; r++)
            {
                string[] cells = boardRows[r].Split(Delim);
                if (cells.Length != Board.Cols)
                {
                    throw new Exception("Saved board row has the wrong number of columns.");
                }

                for (int c = 0; c < Board.Cols; c++)
                {
                    string symbol = cells[c].Trim();
                    if (symbol == EmptyCell || symbol == "")
                    {
                        continue;
                    }

                    Piece? piece = CreateDiscFromSymbol(symbol);
                    if (piece != null)
                    {
                        Board.PlacePiece(r, c, piece);
                    }
                }
            }
        }

        private Piece? CreateDiscFromSymbol(string symbol)
        {
            if (symbol == "R") return new Disc(Player1.Id);
            if (symbol == "Y") return new Disc(Player2.Id);
            return null;
        }

        private void RestoreCurrentPlayer(int playerId)
        {
            if (playerId == Player1.Id)
            {
                CurrentPlayer = Player1;
            }
            else if (playerId == Player2.Id)
            {
                CurrentPlayer = Player2;
            }
            else
            {
                CurrentPlayer = Player1;
            }
        }

        private void RestoreDoneMoves(List<SavedMoveData> savedMoves)
        {
            done.Clear();
            redo.Clear();

            foreach (SavedMoveData savedMove in savedMoves)
            {
                Piece piece = new Disc(savedMove.PlayerId);
                // Use a normal Move here, not ConnectFourMove. The saved row is
                // already the actual landed row, so undo/redo should restore that
                // exact board state rather than recalculating gravity.
                Move move = new Move(savedMove.Row, savedMove.Col, piece);
                done.Push(move);
            }
        }

        private IEnumerable<Move> GetDoneMovesOldestFirst()
        {
            Move[] moves = done.ToArray(); // Stack gives newest first
            for (int i = moves.Length - 1; i >= 0; i--)
            {
                yield return moves[i];
            }
        }

        protected override void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  move - drop a disc into a column");
            Console.WriteLine("  undo - undo the last drop");
            Console.WriteLine("  redo - redo a drop you undid");
            Console.WriteLine("  save or savetxt - save this Connect Four game as a text file");
            Console.WriteLine("  load or loadtxt - load this Connect Four game from a text file");
            Console.WriteLine("  savejson - save this Connect Four game as a JSON file");
            Console.WriteLine("  loadjson - load this Connect Four game from a JSON file");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
            Console.WriteLine("When prompted for a move, enter a column number from 1 to 7.");
            Console.WriteLine("Discs fall to the lowest empty space in that column.");
            Console.WriteLine("Loading a Connect Four save restores the board, current player and undo history.");
            Console.WriteLine("Redo history is cleared when a saved game is loaded.");
            Console.WriteLine();
        }

        private class ConnectFourSaveData
        {
            public ConnectFourSaveData() { }

            public string GameType { get; set; } = "Connect Four";
            public int CurrentPlayerId { get; set; }
            public int Rows { get; set; }
            public int Cols { get; set; }
            public List<string> BoardRows { get; set; } = new List<string>();
            public List<SavedMoveData> DoneMoves { get; set; } = new List<SavedMoveData>();
        }

        private class SavedMoveData
        {
            public SavedMoveData() { }

            public int PlayerId { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
            public string Symbol { get; set; } = "";
        }
    }
}
