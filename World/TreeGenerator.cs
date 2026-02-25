namespace OpenTKTest;

public static class TreeGenerator
{
    // Returns a list of block positions and types to place for a tree
    public static List<(int x, int y, int z, Block block)> GenerateTree(int worldX, int baseY, int worldZ)
    {
        var blocks = new List<(int, int, int, Block)>();

        int trunkHeight = 4;

        // Trunk - store in world coordinates
        for (int i = 0; i < trunkHeight; i++)
        {
            blocks.Add((worldX, baseY + i, worldZ, Block.Log));
        }

        int leafTop = baseY + trunkHeight;

        // Top two layers - 3x3
        for (int layer = 0; layer <= 1; layer++)
        {
            for (int lx = -1; lx <= 1; lx++)
            for (int lz = -1; lz <= 1; lz++)
            {
                blocks.Add((worldX + lx, leafTop + layer, worldZ + lz, Block.Leaves));
            }
        }

        // Bottom two leaf layers - 5x5 with corners cut
        for (int layer = -2; layer <= -1; layer++)
        {
            for (int lx = -2; lx <= 2; lx++)
            for (int lz = -2; lz <= 2; lz++)
            {
                if (MathF.Abs(lx) == 2 && MathF.Abs(lz) == 2)
                    continue;

                blocks.Add((worldX + lx, leafTop + layer, worldZ + lz, Block.Leaves));
            }
        }

        return blocks;
    }
}