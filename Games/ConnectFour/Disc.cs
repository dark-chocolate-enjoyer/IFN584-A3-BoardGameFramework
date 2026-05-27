namespace BoardGameFramework
{
    // Ordinary Connect Four disc. Two colours, one per player.
    public class Disc : Piece
    {
        public Disc(int ownerId) : base(ownerId) { }

        public override string Symbol
        {
            get
            {
                // R for player 1, Y for player 2
                if (OwnerId == 1) return "R";
                return "Y";
            }
        }
    }
}
