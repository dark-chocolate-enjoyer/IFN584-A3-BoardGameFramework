namespace BoardGameFramework
{
    public class NotaktoPiece : Piece
    {
        public NotaktoPiece(int ownerId) : base(ownerId) { }
        public override string Symbol => "X";
    }
}