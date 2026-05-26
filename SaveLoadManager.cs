using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BoardGameFramework
{
    public static class SaveLoadManager
    {
        public static void SaveGameFromMenu(Game game)
        {
            Console.WriteLine();
            Console.WriteLine("Choose save format:");
            Console.WriteLine("1. Plain text (.txt)");
            Console.WriteLine("2. JSON (.json)");
            Console.Write("Enter choice: ");
            string? formatChoice = Console.ReadLine();

            string defaultFileName = GetDefaultFileName(game, formatChoice == "2" ? ".json" : ".txt");
            Console.Write($"Enter filename, or press Enter for '{defaultFileName}': ");
            string? input = Console.ReadLine();
            string fileName = string.IsNullOrWhiteSpace(input) ? defaultFileName : input.Trim();

            if (formatChoice == "2")
            {
                fileName = EnsureExtension(fileName, ".json");
            }
            else
            {
                fileName = EnsureExtension(fileName, ".txt");
            }

            SaveGame(game, fileName);
        }

        public static void SaveGame(Game game, string fileName)
        {
            GameSaveData data = game.CreateSaveData();
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".json")
            {
                SaveJson(data, fileName);
            }
            else
            {
                SaveText(data, fileName);
            }

            Console.WriteLine($"Saved {data.GameType} to '{fileName}'.");
        }

        public static Game? LoadGameFromMenu()
        {
            Console.Write("Enter save filename, for example tictactoe_save.txt or connectfour_save.json: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No filename entered.");
                return null;
            }

            return LoadGame(input.Trim());
        }



        public static Game? LoadGame(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Save file '{fileName}' was not found.");
                return null;
            }

            try
            {
                string content = File.ReadAllText(fileName).TrimStart();

                GameSaveData data;

                if (content.StartsWith("{"))
                {
                    data = LoadJson(fileName);
                }
                else
                {
                    data = LoadText(fileName);
                }

                Game game = CreateGameFromSaveData(data);
                game.LoadSaveData(data);

                Console.WriteLine($"Loaded {data.GameType} from '{fileName}'.");
                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load game: {ex.Message}");
                return null;
            }
        }
        private static Game CreateGameFromSaveData(GameSaveData data)
        {
            Player player1 = CreatePlayer(1, data.Player1Type, "Player 1");
            Player player2 = CreatePlayer(2, data.Player2Type, "Player 2");
            string gameType = NormalizeGameType(data.GameType);

            return gameType switch
            {
                "tictactoe" => new TicTacToe(player1, player2),
                "connectfour" => new ConnectFour(player1, player2),
                "notakto" => new NotaktoGame(player1, player2),
                "gomoku" => new Gomoku(player1, player2),
                _ => throw new InvalidDataException($"Unknown game type '{data.GameType}'.")
            };
        }

        private static Player CreatePlayer(int id, string playerType, string defaultName)
        {
            if (playerType.Equals("Computer", StringComparison.OrdinalIgnoreCase))
            {
                return new ComputerPlayer(id, "Computer");
            }

            return new HumanPlayer(id, defaultName);
        }

        private static void SaveJson(GameSaveData data, string fileName)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(fileName, json);
        }

        private static GameSaveData LoadJson(string fileName)
        {
            string json = File.ReadAllText(fileName);
            GameSaveData? data = JsonSerializer.Deserialize<GameSaveData>(json);

            if (data == null)
            {
                throw new InvalidDataException("JSON save file is empty or invalid.");
            }

            return data;
        }

        private static void SaveText(GameSaveData data, string fileName)
        {
            List<string> lines = new List<string>
            {
                $"GameType={data.GameType}",
                $"Rows={data.Rows}",
                $"Cols={data.Cols}",
                $"CurrentPlayerId={data.CurrentPlayerId}",
                $"Player1Type={data.Player1Type}",
                $"Player2Type={data.Player2Type}",
                "[Boards]"
            };

            foreach (BoardSaveData board in data.Boards)
            {
                lines.Add($"BoardIndex={board.BoardIndex};Rows={board.Rows};Cols={board.Cols}");
                foreach (CellSaveData cell in board.Cells)
                {
                    lines.Add($"Cell={cell.Row},{cell.Col},{cell.OwnerId},{cell.Symbol}");
                }
                lines.Add("EndBoard");
            }

            lines.Add("[/Boards]");
            lines.Add("[DoneMoves]");
            AddMoves(lines, data.DoneMoves);
            lines.Add("[/DoneMoves]");
            lines.Add("[RedoMoves]");
            AddMoves(lines, data.RedoMoves);
            lines.Add("[/RedoMoves]");
            lines.Add("[Extra]");

            foreach (KeyValuePair<string, string> item in data.Extra)
            {
                lines.Add($"{item.Key}={item.Value}");
            }

            lines.Add("[/Extra]");
            File.WriteAllLines(fileName, lines);
        }

        private static void AddMoves(List<string> lines, List<MoveSaveData> moves)
        {
            foreach (MoveSaveData move in moves)
            {
                lines.Add($"Move={move.Row},{move.Col},{move.BoardIndex},{move.OwnerId},{move.Symbol}");
            }
        }

        private static GameSaveData LoadText(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            GameSaveData data = new GameSaveData
            {
                GameType = ReadValue(lines, "GameType"),
                Rows = ReadInt(lines, "Rows"),
                Cols = ReadInt(lines, "Cols"),
                CurrentPlayerId = ReadInt(lines, "CurrentPlayerId"),
                Player1Type = ReadValue(lines, "Player1Type"),
                Player2Type = ReadValue(lines, "Player2Type")
            };

            int i = 0;
            while (i < lines.Length)
            {
                string line = lines[i].Trim();

                if (line == "[Boards]")
                {
                    i++;
                    while (i < lines.Length && lines[i].Trim() != "[/Boards]")
                    {
                        if (lines[i].StartsWith("BoardIndex="))
                        {
                            BoardSaveData board = ParseBoardHeader(lines[i]);
                            i++;
                            while (i < lines.Length && lines[i].Trim() != "EndBoard")
                            {
                                if (lines[i].StartsWith("Cell="))
                                {
                                    board.Cells.Add(ParseCell(lines[i].Substring("Cell=".Length)));
                                }
                                i++;
                            }
                            data.Boards.Add(board);
                        }
                        i++;
                    }
                }
                else if (line == "[DoneMoves]")
                {
                    i++;
                    while (i < lines.Length && lines[i].Trim() != "[/DoneMoves]")
                    {
                        if (lines[i].StartsWith("Move="))
                        {
                            data.DoneMoves.Add(ParseMove(lines[i].Substring("Move=".Length)));
                        }
                        i++;
                    }
                }
                else if (line == "[RedoMoves]")
                {
                    i++;
                    while (i < lines.Length && lines[i].Trim() != "[/RedoMoves]")
                    {
                        if (lines[i].StartsWith("Move="))
                        {
                            data.RedoMoves.Add(ParseMove(lines[i].Substring("Move=".Length)));
                        }
                        i++;
                    }
                }
                else if (line == "[Extra]")
                {
                    i++;
                    while (i < lines.Length && lines[i].Trim() != "[/Extra]")
                    {
                        int equalsIndex = lines[i].IndexOf('=');
                        if (equalsIndex > 0)
                        {
                            string key = lines[i].Substring(0, equalsIndex).Trim();
                            string value = lines[i].Substring(equalsIndex + 1).Trim();
                            data.Extra[key] = value;
                        }
                        i++;
                    }
                }

                i++;
            }

            return data;
        }

        private static BoardSaveData ParseBoardHeader(string line)
        {
            BoardSaveData board = new BoardSaveData();
            string[] parts = line.Split(';');

            foreach (string part in parts)
            {
                string[] keyValue = part.Split('=', 2);
                if (keyValue.Length != 2) continue;

                if (keyValue[0] == "BoardIndex") board.BoardIndex = int.Parse(keyValue[1]);
                if (keyValue[0] == "Rows") board.Rows = int.Parse(keyValue[1]);
                if (keyValue[0] == "Cols") board.Cols = int.Parse(keyValue[1]);
            }

            return board;
        }

        private static CellSaveData ParseCell(string value)
        {
            string[] parts = value.Split(',', 4);
            if (parts.Length != 4) throw new InvalidDataException("Invalid cell data in text save file.");

            return new CellSaveData
            {
                Row = int.Parse(parts[0]),
                Col = int.Parse(parts[1]),
                OwnerId = int.Parse(parts[2]),
                Symbol = parts[3]
            };
        }

        private static MoveSaveData ParseMove(string value)
        {
            string[] parts = value.Split(',', 5);
            if (parts.Length != 5) throw new InvalidDataException("Invalid move data in text save file.");

            return new MoveSaveData
            {
                Row = int.Parse(parts[0]),
                Col = int.Parse(parts[1]),
                BoardIndex = int.Parse(parts[2]),
                OwnerId = int.Parse(parts[3]),
                Symbol = parts[4]
            };
        }

        private static string ReadValue(string[] lines, string key)
        {
            string prefix = key + "=";
            foreach (string line in lines)
            {
                if (line.StartsWith(prefix))
                {
                    return line.Substring(prefix.Length).Trim();
                }
            }

            throw new InvalidDataException($"Missing value '{key}' in text save file.");
        }

        private static int ReadInt(string[] lines, string key)
        {
            string value = ReadValue(lines, key);
            if (!int.TryParse(value, out int result))
            {
                throw new InvalidDataException($"Invalid number for '{key}' in text save file.");
            }
            return result;
        }

        private static string EnsureExtension(string fileName, string extension)
        {
            if (!Path.HasExtension(fileName))
            {
                return fileName + extension;
            }

            return fileName;
        }

        private static string GetDefaultFileName(Game game, string extension)
        {
            string cleanGameType = NormalizeGameType(game.GameType);
            return cleanGameType + "_save" + extension;
        }

        private static string NormalizeGameType(string gameType)
        {
            return gameType.Replace("-", "").Replace(" ", "").ToLowerInvariant();
        }
    }
}