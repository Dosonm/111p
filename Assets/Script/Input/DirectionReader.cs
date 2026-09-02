using UnityEngine;

public static class DirectionReader
{
    public static Vector2Int Read(Vector2 direction, float threshold)
    {
        if (direction.magnitude < threshold)
        {
            return Vector2Int.zero;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle > -22.5f && angle <= 22.5f)
        {
            return new Vector2Int(1, 0);
        }
        if (angle > -67.5f && angle <= -22.5f)
        {
            return new Vector2Int(1, -1);
        }
        if (angle > -112.5f && angle <= -67.5f)
        {
            return new Vector2Int(0, -1);
        }
        if (angle > -157.5f && angle <= -112.5f)
        {
            return new Vector2Int(-1, -1);
        }
        if (angle > 157.5f || angle <= -157.5f)
        {
            return new Vector2Int(-1, 0);
        }

        if (angle > 22.5f && angle <= 67.5f)
        {
            return new Vector2Int(1, 0);
        }

        if (angle > 112.5f && angle <= 157.5f)
        {
            return new Vector2Int(-1, 0);
        }

        return Vector2Int.zero;
    }
}
