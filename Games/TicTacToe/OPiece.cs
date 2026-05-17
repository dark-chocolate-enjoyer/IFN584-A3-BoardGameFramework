namespace BoardGameFramework
{
    public class OPiece : Piece
    {
        public OPiece(int ownerId) : base(ownerId) { }

        public override string Symbol => "O";
    }
}