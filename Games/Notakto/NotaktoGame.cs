using System;
using System.Collections.Generic;

namespace BoardGameFramework
{
    public class NotaktoGame : Game
    {
        private Board[] subBoards;
        private bool[] boardDead;
        private Player? loser;

        public NotaktoGame(Player player1, Player player2)
            : base("Notakto", new Board(3, 3), player1, player2)
        {
            subBoards = new Board[3];
            boardDead = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                subBoards[i] = new Board(3, 3);
                boardDead[i] = false;
            }
            loser = null;
        }

        private bool CheckThreeInARow(Board board)
        {
            // check rows
            for (int r = 0; r < 3; r++)
            {
                if (board.GetPiece(r, 0) != null &&
                    board.GetPiece(r, 1) != null &&
                    board.GetPiece(r, 2) != null)
                    return true;
            }

            // check columns
            for (int c = 0; c < 3; c++)
            {
                if (board.GetPiece(0, c) != null &&
                    board.GetPiece(1, c) != null &&
                    board.GetPiece(2, c) != null)
                    return true;
            }

            // check diagonals
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
                    {
                        Console.WriteLine("Please enter 1, 2 or 3!");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a number!");
                }
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
                    {
                        Console.WriteLine("Please enter 1 to 9!");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a number!");
                }
            }

            return new NotaktoMove(row, col, 
                CreatePieceFor(player), boardIndex);
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
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (subBoards[bi].IsEmpty(r, c))
                        {
                            moves.Add(new NotaktoMove(r, c,
                                CreatePieceFor(player), bi));
                        }
                    }
                }
            }
            return moves;
        }

        public override string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed X on Board {move.BoardIndex + 1} at position ({move.Row + 1}, {move.Col + 1})";
        }

        public new void ApplyMove(Move move)
        {
            int bi = move.BoardIndex;
            subBoards[bi].PlacePiece(move.Row, move.Col, move.Piece);

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

        public new void UndoMove()
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
        }

        public new void RedoMove()
        {
            if (redo.Count == 0)
            {
                Console.WriteLine("Nothing to redo.");
                return;
            }
            Move next = redo.Pop();
            ApplyMove(next);
        }

        public new void Play()
        {
            Console.WriteLine("Welcome to Notakto!");
            Console.WriteLine("Both players place X.");
            Console.WriteLine("Whoever finishes the last board LOSES!");
            Console.WriteLine();

            while (true)
            {
                DisplayAllBoards();

                if (IsGameOver(out Player? winner))
                {
                    Console.WriteLine("All boards are dead!");
                    Console.WriteLine($"{winner!.Name} WINS!");
                    return;
                }

                Console.WriteLine($"{CurrentPlayer.Name}'s turn.");
                string command = ReadCommand();

                if (command == "move")
                {
                    Move move = CurrentPlayer.GetMove(this);
                    ApplyMove(move);
                }
                else if (command == "undo")
                {
                    UndoMove();
                }
                else if (command == "redo")
                {
                    RedoMove();
                }
                else if (command == "help")
                {
                    ShowHelp();
                }
                else if (command == "quit")
                {
                    Console.WriteLine("Game ended early.");
                    return;
                }
            }
        }

        private void DisplayAllBoards()
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

        protected override void ShowHelp()
        {
            base.ShowHelp();
            Console.WriteLine("Notakto rules:");
            Console.WriteLine("  Both players place X on any of 3 boards");
            Console.WriteLine("  When a board gets 3 in a row it is DEAD");
            Console.WriteLine("  The player who finishes the LAST board LOSES!");
            Console.WriteLine();
        }
    }
}