using System;
using System.Collections.Generic;

namespace BoardGameFramework
{
    public class ComputerPlayer : Player
    {
        private Random rng = new Random();

        public ComputerPlayer(int id, string name) : base(id, name) { }

        public override Move GetMove(Game game)
        {
            List<Move> moves = new List<Move>(game.GetValidMoves(this));

            // Take an instant win if there is one.
            foreach (Move m in moves)
            {
                if (game.IsWinningMove(m, this))
                {
                    return m;
                }
            }

            // Otherwise just play something random
            int index = rng.Next(moves.Count);
            return moves[index];
        }
    }
}
