namespace BoardGameFramework
{
    // Base class for any player. Subclasses decide how the move is produced.
    public abstract class Player
    {
        public int Id { get; }
        public string Name { get; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public abstract Move GetMove(Game game);
    }
}