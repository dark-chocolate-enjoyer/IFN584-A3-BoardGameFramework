namespace BoardGameFramework
{
    public class XPiece : Piece
    {
        public XPiece(int ownerId) : base(ownerId) { }

        public override string Symbol => "X";
    }
}