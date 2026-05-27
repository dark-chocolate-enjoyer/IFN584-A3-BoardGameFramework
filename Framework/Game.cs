using System;
using System.Collections.Generic;

namespace BoardGameFramework
{
    public abstract class Game
    {
        public string GameType { get; }
        public Board Board { get; protected set; }
        public Player Player1 { get; }
        public Player Player2 { get; }
        public Player CurrentPlayer { get; protected set; }

        // Stacks for undo and redo. Done holds moves already applied;
        // Redo holds moves that have been undone and can be replayed.
        protected Stack<Move> done = new Stack<Move>();
        protected Stack<Move> redo = new Stack<Move>();

        protected Game(string gameType, Board board, Player player1, Player player2)
        {
            GameType = gameType;
            Board = board;
            Player1 = player1;
            Player2 = player2;
            CurrentPlayer = player1;
        }

        // Main game loop. Keeps running until someone wins or the board is full.
        public void Play()
        {
            while (true)
            {
                Board.Display();

                if (IsGameOver(out Player? winner))
                {
                    AnnounceResult(winner);
                    return;
                }

                Console.WriteLine($"{CurrentPlayer.Name}'s turn.");

                if (CurrentPlayer is ComputerPlayer)
                {
                    Move move = CurrentPlayer.GetMove(this);
                    ApplyMove(move);
                    continue;
                }

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
                else
                {
                    Console.WriteLine("Unknown command. Type help to see available commands.");
                }
            }
        }

        // Apply a move and switch turn. Clears redo because a new branch starts here.
        public virtual void ApplyMove(Move move)
        {
            move.Execute(Board);
            Console.WriteLine(GetMoveDescription(move));
            done.Push(move);
            redo.Clear();
            SwitchPlayer();
        }

        public virtual void UndoMove()
        {
            if (done.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }
            Move last = done.Pop();
            last.Undo(Board);
            redo.Push(last);
            SwitchPlayer();
        }

        public virtual void RedoMove()
        {
            if (redo.Count == 0)
            {
                Console.WriteLine("Nothing to redo.");
                return;
            }
            Move next = redo.Pop();
            next.Execute(Board);
            done.Push(next);
            SwitchPlayer();
        }

        // default check: try every empty cell as a possible move for this player.
        // Games with different rules (number pieces, gravity) override this.
        public virtual IEnumerable<Move> GetValidMoves(Player player)
        {
            List<Move> moves = new List<Move>();
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    if (Board.IsEmpty(r, c))
                    {
                        Piece piece = CreatePieceFor(player);
                        moves.Add(new Move(r, c, piece));
                    }
                }
            }
            return moves;
        }

        // Simulate the move, check if it wins, then take it back. Most games shouldnt need to override this I thin.
        public virtual bool IsWinningMove(Move move, Player player)
        {
            Move test = move.Clone();
            test.Execute(Board);

            bool wins = IsGameOver(out Player? winner) && winner == player;

            test.Undo(Board);
            return wins;
        }

        // Default printout. Games can override for nicer messages.
        public virtual string GetMoveDescription(Move move)
        {
            return $"{CurrentPlayer.Name} placed {move.Piece.Symbol} at ({move.Row + 1}, {move.Col + 1}).";
        }

        protected void SwitchPlayer()
        {
            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
            }
            else
            {
                CurrentPlayer = Player1;
            }
        }

        protected virtual void AnnounceResult(Player? winner)
        {
            if (winner == null)
            {
                Console.WriteLine("Game over. It's a draw.");
            }
            else
            {
                Console.WriteLine($"Game over. {winner.Name} wins!");
            }
        }

        // Each game does its own prompt: pick a basic command first. Individual games can override for specific rules and such.
        protected virtual string ReadCommand()
        {
            Console.Write("Enter command (move / undo / redo / help / quit): ");
            string? input = Console.ReadLine();
            if (input == null) return "";
            return input.Trim().ToLower();
        }

        protected virtual void ShowHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  move - make a move");
            Console.WriteLine("  undo - undo the last move");
            Console.WriteLine("  redo - redo a move you undo'd");
            Console.WriteLine("  help - show this menu");
            Console.WriteLine("  quit - end the game");
            Console.WriteLine();
        }

        // Each game defines its own rules for the following functions, you can add more if you see fit.
        public abstract bool IsValidMove(Move move);
        public abstract bool IsGameOver(out Player? winner);
        public abstract Move ReadHumanMove(Player player);
        public abstract Piece CreatePieceFor(Player player);
    }
}
