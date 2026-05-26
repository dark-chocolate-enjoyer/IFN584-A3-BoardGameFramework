namespace BoardGameFramework
{
    public class NumberPiece : Piece
    {
        public int Value { get; }

        public NumberPiece(int ownerId, int value)
            : base(ownerId)
        {
            Value = value;
        }

        public override string Symbol
        {
            get
            {
                return Value.ToString();
            }
        }
    }
}
