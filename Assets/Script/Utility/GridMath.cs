using UnityEngine;

public static class GridMath
{
    public const int Columns = 9;
    public const int Rows = 10;

    public static float WorldX(int col, float cellSize)
    {
        return (col - (Columns - 1) / 2f) * cellSize;
    }

    public static int ColFromLocalX(float x, float cellSize)
    {
        return Mathf.RoundToInt(x / cellSize) + (Columns - 1) / 2;
    }

    public static int LocalRowFromLocalY(float y, float cellSize)
    {
        return Mathf.RoundToInt(-y / cellSize);
    }

    public static int GlobalRow(int chunkIndex, int localRow)
    {
        return chunkIndex * Rows + localRow;
    }

    public static float OrthoSizeForColumns(int columns, float cellSize, float aspect)
    {
        return (columns * cellSize) / (2f * aspect);
    }
}
