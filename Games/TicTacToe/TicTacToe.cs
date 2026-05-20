using System;

namespace BoardGameFramework
{   

    // Methods need to be implemented, constructor is inheriting from superclass.
    public class TicTacToe : Game
    {
        public TicTacToe(Player player1, Player player2)
            : base("Tic-Tac-Toe", new Board(3, 3), player1, player2)
        {
        }

        public override bool IsValidMove(Move move)
        {
            throw new NotImplementedException();
        }

        public override bool IsGameOver(out Player? winner)
        {
            throw new NotImplementedException();
        }

        public override Move ReadHumanMove(Player player)
        {
            throw new NotImplementedException();
        }

        public override Piece CreatePieceFor(Player player)
        {
            throw new NotImplementedException();
        }
    }
}