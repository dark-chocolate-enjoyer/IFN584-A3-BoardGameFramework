public class Board
{
    private char[] cells;
    public bool IsDead { get; private set; }

    public Board()
    {
        cells = new char[9];
        for (int i = 0; i < 9; i++)
        {
            cells[i] = ' ';
        }
        IsDead = false;
    }

    public bool IsEmpty(int position)
    {
        return cells[position - 1] == ' ';
    }

    public void PlaceX(int position)
    {
        cells[position - 1] = 'X';
        CheckIfDead();
    }

    // check if placing X would kill this board
    public bool WouldKill(int position)
    {
        char[] temp = (char[])cells.Clone();
        temp[position - 1] = 'X';

        if (temp[0] == 'X' && temp[1] == 'X' && temp[2] == 'X') return true;
        if (temp[3] == 'X' && temp[4] == 'X' && temp[5] == 'X') return true;
        if (temp[6] == 'X' && temp[7] == 'X' && temp[8] == 'X') return true;
        if (temp[0] == 'X' && temp[3] == 'X' && temp[6] == 'X') return true;
        if (temp[1] == 'X' && temp[4] == 'X' && temp[7] == 'X') return true;
        if (temp[2] == 'X' && temp[5] == 'X' && temp[8] == 'X') return true;
        if (temp[0] == 'X' && temp[4] == 'X' && temp[8] == 'X') return true;
        if (temp[2] == 'X' && temp[4] == 'X' && temp[6] == 'X') return true;

        return false;
    }

    private void CheckIfDead()
    {
        if (cells[0] == 'X' && cells[1] == 'X' && cells[2] == 'X') IsDead = true;
        if (cells[3] == 'X' && cells[4] == 'X' && cells[5] == 'X') IsDead = true;
        if (cells[6] == 'X' && cells[7] == 'X' && cells[8] == 'X') IsDead = true;
        if (cells[0] == 'X' && cells[3] == 'X' && cells[6] == 'X') IsDead = true;
        if (cells[1] == 'X' && cells[4] == 'X' && cells[7] == 'X') IsDead = true;
        if (cells[2] == 'X' && cells[5] == 'X' && cells[8] == 'X') IsDead = true;
        if (cells[0] == 'X' && cells[4] == 'X' && cells[8] == 'X') IsDead = true;
        if (cells[2] == 'X' && cells[4] == 'X' && cells[6] == 'X') IsDead = true;
    }

    public void Display()
    {
        Console.WriteLine(" {0} | {1} | {2} ", cells[0], cells[1], cells[2]);
        Console.WriteLine("---+---+---");
        Console.WriteLine(" {0} | {1} | {2} ", cells[3], cells[4], cells[5]);
        Console.WriteLine("---+---+---");
        Console.WriteLine(" {0} | {1} | {2} ", cells[6], cells[7], cells[8]);
    }
}