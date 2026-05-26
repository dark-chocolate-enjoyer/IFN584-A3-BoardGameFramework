using System.Collections.Generic;

namespace BoardGameFramework
{
    public class GameSaveData
    {
        public string GameType { get; set; } = "";
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int CurrentPlayerId { get; set; }
        public string Player1Type { get; set; } = "Human";
        public string Player2Type { get; set; } = "Human";
        public List<BoardSaveData> Boards { get; set; } = new List<BoardSaveData>();
        public List<MoveSaveData> DoneMoves { get; set; } = new List<MoveSaveData>();
        public List<MoveSaveData> RedoMoves { get; set; } = new List<MoveSaveData>();
        public Dictionary<string, string> Extra { get; set; } = new Dictionary<string, string>();
    }

    public class BoardSaveData
    {
        public int BoardIndex { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public List<CellSaveData> Cells { get; set; } = new List<CellSaveData>();
    }

    public class CellSaveData
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int OwnerId { get; set; }
        public string Symbol { get; set; } = "";
    }

    public class MoveSaveData
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int BoardIndex { get; set; }
        public int OwnerId { get; set; }
        public string Symbol { get; set; } = "";
    }
}
