using System;
using System.Collections.Generic;

public class NotaktoGame
{
    private Board[] boards;
    private int currentPlayer;

    public NotaktoGame()
    {
        boards = new Board[3];
        boards[0] = new Board();
        boards[1] = new Board();
        boards[2] = new Board();
        currentPlayer = 1;
    }

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

    private bool AllBoardsDead()
    {
        return boards[0].IsDead &&
               boards[1].IsDead &&
               boards[2].IsDead;
    }

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

    // computer picks best move
    private (int board, int position) ComputerMove()
    {
        Random random = new Random();
        List<(int, int)> safeMoves = new List<(int, int)>();
        List<(int, int)> allMoves = new List<(int, int)>();

        for (int b = 0; b < 3; b++)
        {
            if (!boards[b].IsDead)
            {
                for (int p = 1; p <= 9; p++)
                {
                    if (boards[b].IsEmpty(p))
                    {
                        allMoves.Add((b + 1, p));
                        if (!boards[b].WouldKill(p))
                        {
                            safeMoves.Add((b + 1, p));
                        }
                    }
                }
            }
        }

        // pick safe move if available
        if (safeMoves.Count > 0)
        {
            int index = random.Next(safeMoves.Count);
            return safeMoves[index];
        }

        // otherwise pick any random move
        int randomIndex = random.Next(allMoves.Count);
        return allMoves[randomIndex];
    }

    public void Play()
    {
        Console.WriteLine("Welcome to Notakto!");
        Console.WriteLine("Both players place X");
        Console.WriteLine("Whoever finishes the last board LOSES!");
        Console.WriteLine();

        // ask game mode
        Console.WriteLine("Select game mode:");
        Console.WriteLine("1. Human vs Human");
        Console.WriteLine("2. Human vs Computer");
        Console.Write("Enter choice (1 or 2): ");
        string modeInput = Console.ReadLine();
        bool vsComputer = modeInput == "2";
        Console.WriteLine();

        while (true)
        {
            DisplayBoards();
            Console.WriteLine("Player " + currentPlayer + " turn");

            int boardChoice = 0;
            int posChoice = 0;

            // computer turn
            if (vsComputer && currentPlayer == 2)
            {
                Console.WriteLine("Computer is thinking...");
                var move = ComputerMove();
                boardChoice = move.board;
                posChoice = move.position;
                Console.WriteLine("Computer picks Board " + boardChoice + " Position " + posChoice);
            }
            else
            {
                // human turn - pick board
                while (boardChoice < 1 || boardChoice > 3)
                {
                    Console.Write("Pick a board (1, 2 or 3): ");
                    string input = Console.ReadLine();
                    int.TryParse(input, out boardChoice);

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

                // human turn - pick position
                while (posChoice < 1 || posChoice > 9)
                {
                    Console.Write("Pick a position (1-9): ");
                    string input = Console.ReadLine();
                    int.TryParse(input, out posChoice);

                    if (posChoice >= 1 && posChoice <= 9)
                    {
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
            }

            // place the X
            boards[boardChoice - 1].PlaceX(posChoice);

            if (boards[boardChoice - 1].IsDead)
            {
                Console.WriteLine("Board " + boardChoice + " is now DEAD!");
            }

            // check if all boards dead
            if (AllBoardsDead())
            {
                DisplayBoards();
                Console.WriteLine("All boards are dead!");
                Console.WriteLine("Player " + currentPlayer + " placed the last X!");
                Console.WriteLine("Player " + currentPlayer + " LOSES!");

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

            SwitchPlayer();
            Console.WriteLine();
        }
    }
}