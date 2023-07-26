using UnityEngine;

public static class Vector2IntExt
{
    public static Vector2Int Rotate(this Vector2Int vec, int count = 1)
    {
        vec = new Vector2Int(vec.x, vec.y);
        count %= 4;
        if (count < 0)
        {
            count += 4;
        }

        while (count > 0)
        {
            count--;
            vec = new Vector2Int(vec.y, -vec.x);
        }
        return vec;
    }
}
