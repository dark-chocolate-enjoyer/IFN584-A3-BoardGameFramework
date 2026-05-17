namespace BoardGameFramework
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(int id, string name) : base(id, name) { }

        // The game knows what input format it wants, so just ask it.
        public override Move GetMove(Game game)
        {
            return game.ReadHumanMove(this);
        }
    }
}