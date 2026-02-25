namespace OpenTKTest;

public static class TreeGenerator
{
    // Returns a list of block positions and types to place for a tree
    public static List<(int x, int y, int z, Block block)> GenerateTree(int baseX, int baseY, int baseZ)
    {
        var blocks = new List<(int, int, int, Block)>();

        int trunkHeight = 4;

        // Trunk
        for (int i = 0; i < trunkHeight; i++)
        {
            blocks.Add((baseX, baseY + i, baseZ, Block.Log));
        }

        // Leaves - 3x3 at top two layers, 5x5 below
        int leafTop = baseY + trunkHeight;

        // Top two layers - 3x3
        for (int layer = 0; layer <= 1; layer++)
        {
            for (int lx = -1; lx <= 1; lx++)
            for (int lz = -1; lz <= 1; lz++)
            {
                blocks.Add((baseX + lx, leafTop + layer, baseZ + lz, Block.Leaves));
            }
        }

        // Bottom two leaf layers - 5x5 with corners cut
        for (int layer = -2; layer <= -1; layer++)
        {
            for (int lx = -2; lx <= 2; lx++)
            for (int lz = -2; lz <= 2; lz++)
            {
                

                blocks.Add((baseX + lx, leafTop + layer, baseZ + lz, Block.Leaves));
            }
        }

        return blocks;
    }
}