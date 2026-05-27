using System;
using System.Collections.Generic;

namespace BoardGameFramework
{
    // Standard Connect Four: 6 rows, 7 columns, first to 4-in-a-row wins.
    public class ConnectFour : Game
    {
        // 4-in-a-row needed to win
        private const int WinLength = 4;

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

        protected override void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  move - drop a disc into a column");
            Console.WriteLine("  undo - undo the last drop");
            Console.WriteLine("  redo - redo a drop you undid");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
            Console.WriteLine("When prompted, enter a column number from 1 to 7.");
            Console.WriteLine("Discs fall to the lowest empty space in that column.");
            Console.WriteLine();
        }
    }
}
