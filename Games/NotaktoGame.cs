public class NotaktoGame
{
    // 3 boards for the game
    private Board[] boards;
    
    // track whose turn it is
    private int currentPlayer;

    // Constructor - sets up the game
    public NotaktoGame()
    {
        boards = new Board[3];
        boards[0] = new Board();
        boards[1] = new Board();
        boards[2] = new Board();
        currentPlayer = 1;
    }

    // Show all 3 boards on screen
    public void DisplayBoards()
    {
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine("Board " + (i + 1));
            if (boards[i].IsDead)
            {
                Console.WriteLine("This board is DEAD!");
            }
            else
            {
                boards[i].Display();
            }
            Console.WriteLine();
        }
    }

    // Check if all 3 boards are dead
    private bool AllBoardsDead()
    {
        return boards[0].IsDead && 
               boards[1].IsDead && 
               boards[2].IsDead;
    }

    // Switch to other player
    private void SwitchPlayer()
    {
        if (currentPlayer == 1)
        {
            currentPlayer = 2;
        }
        else
        {
            currentPlayer = 1;
        }
    }

    // Main game loop
    public void Play()
    {
        Console.WriteLine("Welcome to Notakto!");
        Console.WriteLine("Both players place X");
        Console.WriteLine("Whoever finishes the last board LOSES!");
        Console.WriteLine();

        // keep playing until all boards dead
        while (true)
        {
            // show the boards
            DisplayBoards();
            Console.WriteLine("Player " + currentPlayer + " turn");

            // ask which board
            int boardChoice = 0;
            while (boardChoice < 1 || boardChoice > 3)
            {
                Console.Write("Pick a board (1, 2 or 3): ");
                string input = Console.ReadLine();
                int.TryParse(input, out boardChoice);

                // check if board is already dead
                if (boardChoice >= 1 && boardChoice <= 3)
                {
                    if (boards[boardChoice - 1].IsDead)
                    {
                        Console.WriteLine("That board is dead! Pick another one.");
                        boardChoice = 0;
                    }
                }
                else
                {
                    Console.WriteLine("Please enter 1, 2 or 3 only!");
                    boardChoice = 0;
                }
            }

            // ask which position
            int posChoice = 0;
            while (posChoice < 1 || posChoice > 9)
            {
                Console.Write("Pick a position (1-9): ");
                string input = Console.ReadLine();
                int.TryParse(input, out posChoice);

                if (posChoice >= 1 && posChoice <= 9)
                {
                    // check if position is empty
                    if (!boards[boardChoice - 1].IsEmpty(posChoice))
                    {
                        Console.WriteLine("That spot is taken! Pick another one.");
                        posChoice = 0;
                    }
                }
                else
                {
                    Console.WriteLine("Please enter 1 to 9 only!");
                    posChoice = 0;
                }
            }

            // place the X
            boards[boardChoice - 1].PlaceX(posChoice);

            // check if that board just died
            if (boards[boardChoice - 1].IsDead)
            {
                Console.WriteLine("Board " + boardChoice + " is now DEAD!");
            }

            // check if all boards are dead
            if (AllBoardsDead())
            {
                DisplayBoards();
                Console.WriteLine("All boards are dead!");
                Console.WriteLine("Player " + currentPlayer + " placed the last X!");
                Console.WriteLine("Player " + currentPlayer + " LOSES!");

                // winner is the other player
                if (currentPlayer == 1)
                {
                    Console.WriteLine("Player 2 WINS!");
                }
                else
                {
                    Console.WriteLine("Player 1 WINS!");
                }
                break;
            }

            // switch to other player
            SwitchPlayer();
            Console.WriteLine();
        }
    }
}