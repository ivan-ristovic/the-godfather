namespace TheGodfather.Modules.Games.Common;

public sealed class MinesweeperField
{
    public int Rows { get; }
    public int Cols { get; }
    public int Bombs { get; }
    public int[,] Field { get; }


    public MinesweeperField(int rows, int cols, int bombs)
    {
        if (rows * cols <= bombs * 2)
            throw new ArgumentException("The amount of bombs is too high for given board size");
        this.Rows = rows;
        this.Cols = cols;
        this.Bombs = bombs;
        this.Field = this.GenerateField();
    }


    private int[,] GenerateField()
    {
        int[,] field = new int[this.Rows, this.Cols];

        var r = new SecureRandom();
        for (int i = 0; i < this.Bombs; i++) {
            int x = r.Next(this.Rows);
            int y = r.Next(this.Cols);
            if (field[x, y] == -1) {
                i--;
                continue;
            }
            field[x, y] = -1;
        }

        for (int i = 0; i < this.Rows; i++)
        for (int j = 0; j < this.Cols; j++) {

            if (field[i, j] == -1)
                continue;

            int count = 0;
            if (i > 0 && j > 0 && field[i - 1, j - 1] == -1)
                count++;
            if (i > 0 && field[i - 1, j] == -1)
                count++;
            if (i > 0 && j < this.Cols - 1 && field[i - 1, j + 1] == -1)
                count++;
            if (j > 0 && field[i, j - 1] == -1)
                count++;
            if (j < this.Cols - 1 && field[i, j + 1] == -1)
                count++;
            if (i < this.Rows - 1 && j > 0 && field[i + 1, j - 1] == -1)
                count++;
            if (i < this.Rows - 1 && field[i + 1, j] == -1)
                count++;
            if (i < this.Rows - 1 && j < this.Cols - 1 && field[i + 1, j + 1] == -1)
                count++;

            field[i, j] = count;
        }

        return field;
    }
}