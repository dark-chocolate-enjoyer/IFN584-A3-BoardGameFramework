public class Board
{
    // The 3x3 grid - stores X or empty
    private char[] cells;
    public bool IsDead { get; private set; }

    // Constructor - creates empty board
    public Board()
    {
        cells = new char[9];
        for (int i = 0; i < 9; i++)
        {
            cells[i] = ' ';
        }
        IsDead = false;
    }

    // Check if a position is empty
    public bool IsEmpty(int position)
    {
        return cells[position - 1] == ' ';
    }

    // Place X on the board
    public void PlaceX(int position)
    {
        cells[position - 1] = 'X';
        CheckIfDead();
    }

    // Check if board has 3 in a row
    private void CheckIfDead()
    {
        // Check horizontal
        if (cells[0] == 'X' && cells[1] == 'X' && cells[2] == 'X') IsDead = true;
        if (cells[3] == 'X' && cells[4] == 'X' && cells[5] == 'X') IsDead = true;
        if (cells[6] == 'X' && cells[7] == 'X' && cells[8] == 'X') IsDead = true;

        // Check vertical
        if (cells[0] == 'X' && cells[3] == 'X' && cells[6] == 'X') IsDead = true;
        if (cells[1] == 'X' && cells[4] == 'X' && cells[7] == 'X') IsDead = true;
        if (cells[2] == 'X' && cells[5] == 'X' && cells[8] == 'X') IsDead = true;

        // Check diagonal
        if (cells[0] == 'X' && cells[4] == 'X' && cells[8] == 'X') IsDead = true;
        if (cells[2] == 'X' && cells[4] == 'X' && cells[6] == 'X') IsDead = true;
    }

    // Display the board on screen
    public void Display()
    {
        Console.WriteLine(" {0} | {1} | {2} ", cells[0], cells[1], cells[2]);
        Console.WriteLine("---+---+---");
        Console.WriteLine(" {0} | {1} | {2} ", cells[3], cells[4], cells[5]);
        Console.WriteLine("---+---+---");
        Console.WriteLine(" {0} | {1} | {2} ", cells[6], cells[7], cells[8]);
    }
}