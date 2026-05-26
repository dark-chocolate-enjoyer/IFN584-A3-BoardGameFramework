namespace BoardGameFramework
{
    
    public class Stone : Piece
    {
        public Stone(int ownerId) : base(ownerId) { }

        public override string Symbol
        {
            get
            {
                if (OwnerId == 1) return "X";
                return "O";
            }
        }
    }
}
