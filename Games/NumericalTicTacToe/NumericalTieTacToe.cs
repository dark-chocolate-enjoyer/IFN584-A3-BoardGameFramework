using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGameFramework
{
    public class NumericalTicTacToe : Game
    {
        private int size;
        private int targetSum;

        private HashSet<int> usedNumbers = new HashSet<int>();

        public NumericalTicTacToe(
            int size,
            Player player1,
            Player player2)
            : base(
                  "Numerical Tic-Tac-Toe",
                  new Board(size, size),
                  player1,
                  player2)
        {
            this.size = size;

            targetSum = size * (size * size + 1) / 2;
        }

        public override Piece CreatePieceFor(Player player)
        {
            return new NumberPiece(player.Id, 0);
        }

        public override Move ReadHumanMove(Player player)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine();

                    Console.Write($"{player.Name} enter row col number: ");

                    string? input = Console.ReadLine();

                    if (input == null)
                        continue;

                    string[] parts = input.Split(' ');

                    if (parts.Length != 3)
                    {
                        Console.WriteLine("Invalid input.");
                        continue;
                    }

                    int row = int.Parse(parts[0]) - 1;
                    int col = int.Parse(parts[1]) - 1;
                    int number = int.Parse(parts[2]);

                    Move move = new Move(
                        row,
                        col,
                        new NumberPiece(player.Id, number));

                    if (IsValidMove(move))
                    {
                        return move;
                    }

                    Console.WriteLine("Invalid move.");
                }
                catch
                {
                    Console.WriteLine("Invalid input.");
                }
            }
        }

        public override bool IsValidMove(Move move)
        {
            if (!Board.IsInBounds(move.Row, move.Col))
            {
                return false;
            }

            if (!Board.IsEmpty(move.Row, move.Col))
            {
                return false;
            }

            NumberPiece piece = (NumberPiece)move.Piece;

            int number = piece.Value;

            if (number < 1 || number > size * size)
            {
                return false;
            }

            if (usedNumbers.Contains(number))
            {
                return false;
            }

            // Player 1 -> odd
            if (move.Piece.OwnerId == 1 && number % 2 == 0)
            {
                return false;
            }

            // Player 2 -> even
            if (move.Piece.OwnerId == 2 && number % 2 != 0)
            {
                return false;
            }

            return true;
        }

        public override void ApplyMove(Move move)
        {
            NumberPiece piece = (NumberPiece)move.Piece;

            usedNumbers.Add(piece.Value);

            base.ApplyMove(move);
        }

        public override void UndoMove()
        {
            if (done.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }

            Move last = done.Pop();

            NumberPiece piece = (NumberPiece)last.Piece;

            usedNumbers.Remove(piece.Value);

            last.Undo(Board);

            redo.Push(last);

            SwitchPlayer();
        }

        public override bool IsGameOver(out Player? winner)
        {
            winner = null;

            // Rows
            for (int r = 0; r < size; r++)
            {
                if (CheckLine(GetRow(r), out int owner))
                {
                    winner = GetPlayer(owner);
                    return true;
                }
            }

            // Cols
            for (int c = 0; c < size; c++)
            {
                if (CheckLine(GetCol(c), out int owner))
                {
                    winner = GetPlayer(owner);
                    return true;
                }
            }

            // Main diagonal
            List<Piece> diag1 = new List<Piece>();

            for (int i = 0; i < size; i++)
            {
                Piece? p = Board.GetPiece(i, i);

                if (p == null)
                    break;

                diag1.Add(p);
            }

            if (diag1.Count == size &&
                CheckLine(diag1, out int diagOwner1))
            {
                winner = GetPlayer(diagOwner1);
                return true;
            }

            // Anti diagonal
            List<Piece> diag2 = new List<Piece>();

            for (int i = 0; i < size; i++)
            {
                Piece? p = Board.GetPiece(i, size - 1 - i);

                if (p == null)
                    break;

                diag2.Add(p);
            }

            if (diag2.Count == size &&
                CheckLine(diag2, out int diagOwner2))
            {
                winner = GetPlayer(diagOwner2);
                return true;
            }

            if (Board.IsFull())
            {
                return true;
            }

            return false;
        }

        private bool CheckLine(
            List<Piece> pieces,
            out int owner)
        {
            owner = 0;

            if (pieces.Count != size)
            {
                return false;
            }

            int sum = 0;

            foreach (NumberPiece p in pieces)
            {
                sum += p.Value;
            }

            if (sum == targetSum)
            {
                owner = pieces[0].OwnerId;
                return true;
            }

            return false;
        }

        private List<Piece> GetRow(int row)
        {
            List<Piece> list = new List<Piece>();

            for (int c = 0; c < size; c++)
            {
                Piece? p = Board.GetPiece(row, c);

                if (p == null)
                {
                    return new List<Piece>();
                }

                list.Add(p);
            }

            return list;
        }

        private List<Piece> GetCol(int col)
        {
            List<Piece> list = new List<Piece>();

            for (int r = 0; r < size; r++)
            {
                Piece? p = Board.GetPiece(r, col);

                if (p == null)
                {
                    return new List<Piece>();
                }

                list.Add(p);
            }

            return list;
        }

        private Player GetPlayer(int id)
        {
            return Player1.Id == id ? Player1 : Player2;
        }

        public override string GetMoveDescription(Move move)
        {
            NumberPiece piece = (NumberPiece)move.Piece;

            return $"{CurrentPlayer.Name} placed {piece.Value} at ({move.Row + 1}, {move.Col + 1})";
        }
    }
}
