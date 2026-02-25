namespace OpenTKTest;

public static class Terrain
{
    public const int SeaLevel = 7; // Water fills up to this Y level

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

    public static bool ShouldSpawnTree(int worldX, int worldZ)
    {
        int hash = worldX * 374761393 ^ worldZ * 668265263 ^ 9999;
        hash = (hash ^ (hash >> 13)) * 1274126177;
        float value = (float)(hash & 0xFFFF) / 0xFFFF;
        return value < 0.01f;
    }

    // Returns true if this position should be sand
    // Sand spawns within 3 blocks of sea level horizontally
    public static bool ShouldBeSand(int worldX, int worldZ)
    {
        int height = GetHeight(worldX, worldZ);

        // Must be near water to have any sand at all
        if (height > SeaLevel + 2)
            return false;

        // Use noise to create patchy sand rather than uniform sand
        float sandNoise = Noise.Perlin(
            worldX * 0.1f,
            worldZ * 0.1f,
            seed: 4242
        );

        // Closer to water = higher chance of sand
        float distanceFromWater = height - SeaLevel;

        // At sea level always sand, further away needs stronger noise
        return sandNoise > distanceFromWater * 0.4f;
    }
}