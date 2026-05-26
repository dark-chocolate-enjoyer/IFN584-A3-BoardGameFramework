using BoardGameFramework;

bool exitProgram = false;

while (!exitProgram)
{
    Console.Clear();
    Console.WriteLine("==================================");
    Console.WriteLine("      Board Game Framework");
    Console.WriteLine("==================================");
    Console.WriteLine("1. Start new game");
    Console.WriteLine("2. Load saved game");
    Console.WriteLine("3. Exit");
    Console.Write("Enter your choice: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            StartNewGame();
            break;

        case "2":
            LoadExistingGame();
            break;

        case "3":
            exitProgram = true;
            Console.WriteLine("Goodbye!");
            break;

        default:
            Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
            Pause();
            break;
    }
}

static void StartNewGame()
{
    Console.Clear();
    Console.WriteLine("Select a game:");
    Console.WriteLine("1. Tic-Tac-Toe");
    Console.WriteLine("2. Notakto");
    Console.WriteLine("3. Connect Four");
    Console.WriteLine("4. Numerical Tic-Tac-Toe");
    Console.WriteLine("5. Gomoku");
    Console.WriteLine("6. Back to main menu");
    Console.Write("Enter your choice: ");

    string? gameChoice = Console.ReadLine();

    switch (gameChoice)
    {
        case "1":
            StartTicTacToe();
            break;

        case "2":
            StartNotakto();
            break;

        case "3":
            StartConnectFour();
            break;

        case "4":
            Console.WriteLine("Numerical Tic-Tac-Toe is not connected yet.");
            Pause();
            break;

        case "5":
            Console.WriteLine("Gomoku is not connected yet.");
            Pause();
            break;

        case "6":
            return;

        default:
            Console.WriteLine("Invalid game choice.");
            Pause();
            break;
    }
}

static Player[] CreatePlayers()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("Select game mode:");
        Console.WriteLine("1. Human vs Human");
        Console.WriteLine("2. Human vs Computer");
        Console.Write("Enter your choice: ");

        string? modeChoice = Console.ReadLine();

        if (modeChoice == "1")
        {
            return new Player[]
            {
                new HumanPlayer(1, "Player 1"),
                new HumanPlayer(2, "Player 2")
            };
        }

        if (modeChoice == "2")
        {
            return new Player[]
            {
                new HumanPlayer(1, "Player 1"),
                new ComputerPlayer(2, "Computer")
            };
        }

        Console.WriteLine("Invalid mode choice. Please enter 1 or 2.");
        Pause();
    }
}

static void StartTicTacToe()
{
    Player[] players = CreatePlayers();
    TicTacToe game = new TicTacToe(players[0], players[1]);
    game.Play();
    Pause();
}

static void StartConnectFour()
{
    Player[] players = CreatePlayers();
    ConnectFour game = new ConnectFour(players[0], players[1]);
    game.Play();
    Pause();
}

static void StartNotakto()
{
    Player[] players = CreatePlayers();
    NotaktoGame game = new NotaktoGame(players[0], players[1]);
    game.Play();
    Pause();
}

static void LoadExistingGame()
{
    Console.Clear();
    Game? game = SaveLoadManager.LoadGameFromMenu();
    if (game != null)
    {
        game.Play();
    }
    Pause();
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}
