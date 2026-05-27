namespace BoardGameFramework
{
    // Move that knows about gravity. The user only chooses a column,
    public class ConnectFourMove : Move
    {
        private int landedRow = -1;

        // base row is set to 0 here, the real row is filled in once we drop
        public ConnectFourMove(int col, Piece piece) : base(0, col, piece) { }

        public override void Execute(Board board)
        {
            // scan from the bottom row upwards
            for (int r = board.Rows - 1; r >= 0; r--)
            {
                if (board.IsEmpty(r, Col))
                {
                    board.PlacePiece(r, Col, Piece);
                    Row = r;
                    landedRow = r;
                    return;
                }
            }
            // if we got here the column was full, which shouldnt happen if
            // IsValidMove was checked first
        }

        public override void Undo(Board board)
        {
            // only clear if we actually placed something
            if (landedRow >= 0)
            {
                board.RemovePiece(landedRow, Col);
                landedRow = -1;
            }
        }

        public override Move Clone()
        {
            // fresh clone, gravity recalculates when Execute runs on it
            return new ConnectFourMove(Col, Piece);
        }
    }
}
