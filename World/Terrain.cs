namespace OpenTKTest;

public static class Terrain
{
    public static int GetHeight(int worldX, int worldZ)
    {
        float scale = 0.05f;     // bigger = smoother terrain
        float amplitude = 10f;   // max height variation
        float baseHeight = 8f;   // sea level

        float noise = Noise.Perlin(
            worldX * scale,
            worldZ * scale,
            seed: 1337
        );

        return (int)(baseHeight + noise * amplitude);
    }
}