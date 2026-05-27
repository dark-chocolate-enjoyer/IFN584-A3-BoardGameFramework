using System;

namespace BoardGameFramework
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine();
                Console.WriteLine("1. Tic-Tac-Toe");
                Console.WriteLine("2. Numerical Tic-Tac-Toe");
                Console.WriteLine("3. Notakto");
                Console.WriteLine("4. Gomoku");
                Console.WriteLine("5. Connect Four");
                Console.WriteLine("6. Exit");
                Console.Write("Choose a game: ");

                string? gameChoice = Console.ReadLine();

                if (gameChoice == "6")
                {
                    Console.WriteLine("Hope you enjoyed :)");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Choose mode:");
                Console.WriteLine("1. HumanVSHuman");
                Console.WriteLine("2. HumanVSComputer");
                Console.Write("Mode: ");

                string? modeChoice = Console.ReadLine();

                Player player1 = new HumanPlayer(1, "Player 1");
                Player player2;

                if (modeChoice == "2")
                {
                    player2 = new ComputerPlayer(2, "Computer");
                }
                else
                {
                    player2 = new HumanPlayer(2, "Player 2");
                }

                Game? game = CreateGame(gameChoice, player1, player2);

                if (game == null)
                {
                    Console.WriteLine("Invalid game choice.");
                    Console.WriteLine();
                    continue;
                }

                game.Play();

                Console.WriteLine();
                Console.WriteLine("Game finished.");

                Console.WriteLine();
            }
        }

        private static Game? CreateGame(string? choice, Player player1, Player player2)
        {
            if (choice == "1")
            {
                return new TicTacToe(player1, player2);
            }
            else if (choice == "2")
            // Default size for NumericalTicTacToe is 3
            {
                return new NumericalTicTacToe(3, player1, player2);
            }
            else if (choice == "3")
            {
                return new NotaktoGame(player1, player2);
            }
            else if (choice == "4")
            {
                return new Gomoku(player1, player2);
            }
            else if (choice == "5")
            {
                return new ConnectFour(player1, player2);
            }

            return null;
        }
    }
}