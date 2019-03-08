
public class BSPRect
{
    static BSPRect _zero = new BSPRect();
    public static BSPRect Zero => _zero;
    public int Row;
    public int Col;
    public int Height;
    public int Width;

    public BSPRect(int row = 0, int col = 0, int height = 0, int width = 0)
    {
        Set(row, col, height, width);
    }

    public void Set(int row = 0, int col = 0, int height = 0, int width = 0)
    {
        Row = row;
        Col = col;
        Height = height;
        Width = width;
    }

    public override string ToString()
    {
        return $"[{Row}, {Col}, {Height}, {Width}]";
    }

    public bool Equals(BSPRect other)
    {
        if (other == null) return false;
        return (other.Row == Row && other.Col == Col && other.Height == Height && other.Width == Width);
    }
}