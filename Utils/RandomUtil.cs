using UnityEngine;
using Random = UnityEngine.Random;
class RandomUtils
{
    private static Vector3[] _v = new Vector3[6] {
        new (45,223.42f,0),
        new (216,72.72f,0),
        new (171,-150.72f,0),
        new (-45,-223.42f,0),
        new (-216,-72.72f,0),
        new (-171,150.72f,0),
    };
    private static float Line(float x, float gradient, Vector3 a)
    {
        return gradient * (x - a.x) + a.y;
    }
    private static bool IsHigherThanLine(Vector3 pos, Vector3 a, Vector3 b)
    {
        float grad = (b.y - a.y) / (b.x - a.x);
        if (b.x < a.x) return pos.x > b.x && pos.x < a.x && pos.y > Line(pos.x, grad, a);
        else return pos.x < b.x && pos.x > a.x && pos.y > Line(pos.x, grad, a);
    }
    private static bool IsLowerThanLine(Vector3 pos, Vector3 a, Vector3 b)
    {
        float grad = (b.y - a.y) / (b.x - a.x);
        if (!IsHigherThanLine(pos, a, b)) return pos.y != Line(pos.x, grad, a);
        else return false;
    }

    private static bool IsInHexagon(Vector3 pos)
    {
        return IsLowerThanLine(pos, _v[0], _v[1]) &&
               IsHigherThanLine(pos, _v[1], _v[2]) &&
               IsHigherThanLine(pos, _v[2], _v[3]) &&
               IsHigherThanLine(pos, _v[3], _v[4]) &&
               IsLowerThanLine(pos, _v[4], _v[5]) &&
               IsLowerThanLine(pos, _v[5], _v[0]);
    }

    public static Vector3 GetRandomPosition()
    {
        Vector3 pos = new(Random.Range(-200, 200), Random.Range(-200, 200), 0);
        while (!IsInHexagon(pos))
        {
            pos = new(Random.Range(-200, 200), Random.Range(-200, 200), 0);
        }
        return pos;
    }
}