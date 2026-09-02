using System.Collections.Generic;

public class MonsterRegistry
{
    private readonly Dictionary<(int row, int col), Monster> occupied = new Dictionary<(int, int), Monster>();

    public bool TryGet(int row, int col, out Monster monster)
    {
        return occupied.TryGetValue((row, col), out monster);
    }

    public void Register(int row, int col, Monster monster)
    {
        occupied[(row, col)] = monster;
    }

    public void Unregister(int row, int col, Monster monster)
    {
        if (occupied.TryGetValue((row, col), out Monster occupant) && occupant == monster)
        {
            occupied.Remove((row, col));
        }
    }

    public int Count => occupied.Count;

    public void Clear()
    {
        occupied.Clear();
    }
}
