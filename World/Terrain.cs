namespace OpenTKTest;

public static class Terrain
{
    public static int GetHeight(int worldX, int worldZ)
    {
        float scale = 0.05f;
        float amplitude = 10f;
        float baseHeight = 8f;

        float noise = Noise.Perlin(
            worldX * scale,
            worldZ * scale,
            seed: 1337
        );

        return (int)(baseHeight + noise * amplitude);
    }

    // Returns true if a tree should spawn at this world position
    public static bool ShouldSpawnTree(int worldX, int worldZ)
    {
        // Use a different seed so trees don't align with terrain
        int hash = worldX * 374761393 ^ worldZ * 668265263 ^ 9999;
        hash = (hash ^ (hash >> 13)) * 1274126177;
        float value = (float)(hash & 0xFFFF) / 0xFFFF;

        // Roughly 1 in 20 surface blocks gets a tree
        return value < 0.01f;
    }
}