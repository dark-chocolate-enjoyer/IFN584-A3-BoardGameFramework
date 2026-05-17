using System;

namespace BoardGameFramework
{
    // A general 2D board of pieces. Indexed [row, col] with row 0 at the
    // top of the display and column 0 on the left.
    // made to be used directly by TicTacToe, Numerical TicTacToe, Gomoku, and Connect Four. Notakto holds three Board objects for its sub-boards.
    public class Board
    {
        public int Rows { get; }
        public int Cols { get; }

        private Piece?[,] grid;

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            grid = new Piece?[rows, cols];
        }

        public Piece? GetPiece(int row, int col)
        {
            return grid[row, col];
        }

        public void PlacePiece(int row, int col, Piece piece)
        {
            grid[row, col] = piece;
        }

        public void RemovePiece(int row, int col)
        {
            grid[row, col] = null;
        }

        public bool IsEmpty(int row, int col)
        {
            return grid[row, col] == null;
        }

        public bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < Rows && col >= 0 && col < Cols;
        }

        public bool IsFull()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (grid[r, c] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Default console display, the different games can override for custom layouts
        // Numerical Tic-Tac-Toe needs wider cells for 2-digit numbers, Notakto prints three boards together.
        public virtual void Display()
        {
            Console.WriteLine();

            // Column headers (1-indexed for the user).
            Console.Write("   ");
            for (int c = 0; c < Cols; c++)
            {
                Console.Write($" {c + 1,2} ");
            }
            Console.WriteLine();

            PrintRowSeparator();

            for (int r = 0; r < Rows; r++)
            {
                Console.Write($"{r + 1,2} |");
                for (int c = 0; c < Cols; c++)
                {
                    Piece? piece = grid[r, c];
                    string symbol = piece == null ? " " : piece.Symbol;
                    Console.Write($" {symbol} |");
                }
                Console.WriteLine();
                PrintRowSeparator();
            }

            Console.WriteLine();
        }

        private void PrintRowSeparator()
        {
            Console.Write("   +");
            for (int c = 0; c < Cols; c++)
            {
                Console.Write("---+");
            }
            Console.WriteLine();
        }
    }
}