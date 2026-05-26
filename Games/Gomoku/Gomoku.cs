using System;

namespace BoardGameFramework
{
    
    public class Gomoku : Game
    {
        private const int WinLength = 5;
        private const int BoardSize = 15;

        public Gomoku(Player player1, Player player2)
            : base("Gomoku", new Board(BoardSize, BoardSize), player1, player2)
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
                winner = null;
                return true;
            }

            winner = null;
            return false;
        }

        private bool HasLineFrom(int row, int col, int dRow, int dCol)
        {
            Piece? first = Board.GetPiece(row, col);
            if (first == null) return false;

            int endRow = row + (WinLength - 1) * dRow;
            int endCol = col + (WinLength - 1) * dCol;
            if (!Board.IsInBounds(endRow, endCol)) return false;

            for (int i = 1; i < WinLength; i++)
            {
                Piece? p = Board.GetPiece(row + i * dRow, col + i * dCol);
                if (p == null || p.OwnerId != first.OwnerId)
                {
                    return false;
                }
            }

            return true;
        }

        public override Move ReadHumanMove(Player player)
        {
            while (true)
            {
                Console.Write($"{player.Name}, enter row (1-{Board.Rows}): ");
                string? rowInput = Console.ReadLine();

                Console.Write($"{player.Name}, enter column (1-{Board.Cols}): ");
                string? colInput = Console.ReadLine();

                if (rowInput == null || colInput == null)
                {
                    Console.WriteLine("Invalid input. Try again.");
                    continue;
                }

                if (int.TryParse(rowInput.Trim(), out int row) &&
                    int.TryParse(colInput.Trim(), out int col))
                {
                    int row0 = row - 1;
                    int col0 = col - 1;

                    Piece piece = CreatePieceFor(player);
                    Move move = new Move(row0, col0, piece);

                    if (IsValidMove(move))
                    {
                        return move;
                    }

                    Console.WriteLine("That position is out of range or already occupied. Try again.");
                }
                else
                {
                    Console.WriteLine($"Please enter numbers from 1 to {Board.Rows} and 1 to {Board.Cols}.");
                }
            }
        }

        public override Piece CreatePieceFor(Player player)
        {
            return new Stone(player.Id);
        }

        public override string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed {move.Piece.Symbol} at ({move.Row + 1}, {move.Col + 1}).";
        }

        protected override void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  move - place a stone on the board");
            Console.WriteLine("  undo - undo the last move");
            Console.WriteLine("  redo - redo a move you undid");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
            Console.WriteLine($"When prompted, enter a row and column from 1 to {Board.Rows}.");
            Console.WriteLine("The first player to make 5 in a row horizontally, vertically, or diagonally wins.");
            Console.WriteLine();
        }
    }
}
