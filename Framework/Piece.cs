namespace BoardGameFramework
{
    // Abstract base for any piece placed on a board.
    // Each concrete piece (XPiece, OPiece, NumberPiece, Disc, ...)
    // supplies a Symbol for display and identifies which player owns it.
    public abstract class Piece
    {
        public int OwnerId { get; }

        public Piece(int ownerId)
        {
            OwnerId = ownerId;
        }

        // Each subclass returns the text shown for this piece on the board.
        public abstract string Symbol { get; }
    }
}