namespace BoardGameFramework
{
    public class NotaktoMove : Move
    {
        public NotaktoMove(int row, int col, Piece piece, int boardIndex) 
            : base(row, col, piece, boardIndex) { }

        public override void Execute(Board board) { }
        public override void Undo(Board board) { }

        public override Move Clone()
        {
            return new NotaktoMove(Row, Col, Piece, BoardIndex);
        }
    }
}