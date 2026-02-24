using OpenTK.Mathematics;

namespace OpenTKTest;

public static class Noise
{
    // Simple value noise (fast + easy to tweak)
    public static float Perlin(float x, float y, int seed = 0)
    {
        int x0 = (int)MathF.Floor(x);
        int y0 = (int)MathF.Floor(y);

        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float sx = Fade(x - x0);
        float sy = Fade(y - y0);

        float n0 = DotGridGradient(x0, y0, x, y, seed);
        float n1 = DotGridGradient(x1, y0, x, y, seed);
        float ix0 = Lerp(n0, n1, sx);

        n0 = DotGridGradient(x0, y1, x, y, seed);
        n1 = DotGridGradient(x1, y1, x, y, seed);
        float ix1 = Lerp(n0, n1, sx);

        return Lerp(ix0, ix1, sy);
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private static float DotGridGradient(int ix, int iy, float x, float y, int seed)
    {
        int hash = ix * 374761393 + iy * 668265263 ^ seed;
        hash = (hash ^ (hash >> 13)) * 1274126177;

        float angle = (hash & 1023) / 1023f * MathF.Tau;
        Vector2 gradient = new(MathF.Cos(angle), MathF.Sin(angle));

        Vector2 distance = new(x - ix, y - iy);
        return Vector2.Dot(gradient, distance);
    }
}