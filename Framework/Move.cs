namespace BoardGameFramework
{
    // A Move places a piece on a board and can undo itself by removing it
    // Most games use this class directly. Connect Four needs gravity, so it
    // has its own ConnectFourMove subclass that overrides Execute.
    public class Move
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Piece Piece { get; }

        // Should be useful for Notakto? which needs to index which of the subboards its using out of 3, default is just 0 for other games.
        public int BoardIndex { get; }

        public Move(int row, int col, Piece piece, int boardIndex = 0)
        {
            Row = row;
            Col = col;
            Piece = piece;
            // again, default is 0, only needs to be changed for Notakto game (maybe, whoever is doing Notakto can decide if its useful or not)
            BoardIndex = boardIndex;
        }

        public virtual void Execute(Board board)
        {
            board.PlacePiece(Row, Col, Piece);
        }

        public virtual void Undo(Board board)
        {
            board.RemovePiece(Row, Col);
        }

        // returns a copy of a move so we can test it (apply then undo) without touching the original. I used this in my assignment as a
        // helper for Connect 4, it may or may not be useful for whoever is doing conntect 4, if you dont wanna use it or if it doesnt
        // make sense, just ignore it.
        public virtual Move Clone()
        {
            return new Move(Row, Col, Piece, BoardIndex);
        }
    }
}