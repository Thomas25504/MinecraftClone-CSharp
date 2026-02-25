using OpenTK.Mathematics;

namespace OpenTKTest;

public class World
{
    // How many chunks to load in each direction around the player
    public const int RenderDistance = 4;

    private string saveDirectory;

    public World(string worldName = "World1")
    {
        saveDirectory = Path.Combine("Saves", worldName);
        Directory.CreateDirectory(saveDirectory);
    }

    // Neighbor offsets for mesh updates
    private static readonly Vector2i[] NeighborOffsets =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    };

    // Loaded chunks indexed by their chunk coordinates (chunkX, chunkZ)
    public Dictionary<Vector2i, Chunk> Chunks = new();

    // Update loaded chunks based on player position
    public void Update(Vector3 playerPosition)
    {
        Vector2i playerChunk = WorldToChunk(playerPosition);

        LoadChunksAround(playerChunk);
        UnloadFarChunks(playerChunk);
    }

    #region Chunk Loading

    private void SaveChunk(Vector2i coord, Chunk chunk)
    {
        string path = Path.Combine(saveDirectory, $"{coord.X}_{coord.Y}.bin");

        using var writer = new BinaryWriter(File.Open(path, FileMode.Create));

        for (int x = 0; x < Chunk.SizeX; x++)
        for (int y = 0; y < Chunk.SizeY; y++)
        for (int z = 0; z < Chunk.SizeZ; z++)
        {
            writer.Write((byte)chunk.Blocks[x, y, z].Type);
        }
    }

    private bool TryLoadChunk(Vector2i coord, World world, Vector3 worldPos, out Chunk chunk)
    {
        string path = Path.Combine(saveDirectory, $"{coord.X}_{coord.Y}.bin");

        if (!File.Exists(path))
        {
            chunk = null;
            return false;
        }

        chunk = new Chunk(world, worldPos, skipGenerate: true); // No BuildMesh yet

        using var reader = new BinaryReader(File.Open(path, FileMode.Open));

        for (int x = 0; x < Chunk.SizeX; x++)
        for (int y = 0; y < Chunk.SizeY; y++)
        for (int z = 0; z < Chunk.SizeZ; z++)
        {
            BlockType type = (BlockType)reader.ReadByte();
            chunk.Blocks[x, y, z] = Block.FromType(type);
        }

        chunk.RebuildMesh(); // Now blocks are filled, safe to build mesh
        return true;
    }

    // Load chunks within render distance and trigger mesh rebuilds for neighbors
    private void LoadChunksAround(Vector2i center)
    {
        for (int x = -RenderDistance; x <= RenderDistance; x++)
        for (int z = -RenderDistance; z <= RenderDistance; z++)
        {
            Vector2i coord = new(center.X + x, center.Y + z);

            if (!Chunks.ContainsKey(coord))
            {
                Vector3 worldPos = new Vector3(
                    coord.X * Chunk.SizeX,
                    0,
                    coord.Y * Chunk.SizeZ
                );

                Chunk chunk;

                // Try loading from disk first, otherwise generate fresh
                if (!TryLoadChunk(coord, this, worldPos, out chunk))
                {
                    chunk = new Chunk(this, worldPos);
                }

                Chunks.Add(coord, chunk);

                foreach (var offset in NeighborOffsets)
                {
                    Vector2i neighborCoord = new(coord.X + offset.X, coord.Y + offset.Y);
                    if (Chunks.TryGetValue(neighborCoord, out Chunk neighbor))
                    {
                        neighbor.RebuildMesh();
                    }
                }
            }
        }
    }

    // Unload chunks that are outside the render distance
    private void UnloadFarChunks(Vector2i center)
    {
        List<Vector2i> toRemove = new();

        foreach (var kvp in Chunks)
        {
            int dx = kvp.Key.X - center.X;
            int dz = kvp.Key.Y - center.Y;

            if (Math.Abs(dx) > RenderDistance || Math.Abs(dz) > RenderDistance)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            SaveChunk(key, Chunks[key]); // Save before unloading
            Chunks.Remove(key);
        }
    }

    // Check if a block at world coordinates is solid
    public bool IsBlockSolid(Vector3i worldPos) => GetBlock(worldPos).IsSolid;

    public Block GetBlock(Vector3i worldPos)
    {
        int chunkX = (int)MathF.Floor(worldPos.X / (float)Chunk.SizeX);
        int chunkZ = (int)MathF.Floor(worldPos.Z / (float)Chunk.SizeZ);

        Vector2i chunkCoord = new(chunkX, chunkZ);

        if (!Chunks.TryGetValue(chunkCoord, out Chunk chunk))
            return Block.Air; // chunk not loaded = air

        int localX = worldPos.X - chunkX * Chunk.SizeX;
        int localY = worldPos.Y;
        int localZ = worldPos.Z - chunkZ * Chunk.SizeZ;

        // Bounds check
        if (localX < 0 || localX >= Chunk.SizeX ||
            localY < 0 || localY >= Chunk.SizeY ||
            localZ < 0 || localZ >= Chunk.SizeZ)
            return Block.Air;

        return chunk.Blocks[localX, localY, localZ];
    }

    public void SetBlock(Vector3i worldPos, Block block)
    {
        int chunkX = (int)MathF.Floor(worldPos.X / (float)Chunk.SizeX);
        int chunkZ = (int)MathF.Floor(worldPos.Z / (float)Chunk.SizeZ);

        Vector2i chunkCoord = new(chunkX, chunkZ);

        if (!Chunks.TryGetValue(chunkCoord, out Chunk chunk))
            return;

        int localX = worldPos.X - chunkX * Chunk.SizeX;
        int localY = worldPos.Y;
        int localZ = worldPos.Z - chunkZ * Chunk.SizeZ;

        if (localX < 0 || localX >= Chunk.SizeX ||
            localY < 0 || localY >= Chunk.SizeY ||
            localZ < 0 || localZ >= Chunk.SizeZ)
            return;

        chunk.Blocks[localX, localY, localZ] = block;
        chunk.RebuildMesh();

        // Rebuild neighbor chunks if block is on a border
        if (localX == 0) RebuildNeighbor(chunkX - 1, chunkZ);
        if (localX == Chunk.SizeX - 1) RebuildNeighbor(chunkX + 1, chunkZ);
        if (localZ == 0) RebuildNeighbor(chunkX, chunkZ - 1);
        if (localZ == Chunk.SizeZ - 1) RebuildNeighbor(chunkX, chunkZ + 1);
    }

    private void RebuildNeighbor(int chunkX, int chunkZ)
    {
        Vector2i coord = new(chunkX, chunkZ);
        if (Chunks.TryGetValue(coord, out Chunk chunk))
            chunk.RebuildMesh();
    }

    #endregion

    #region Helpers

    // Convert world coordinates to chunk coordinates
    private Vector2i WorldToChunk(Vector3 pos)
    {
        return new Vector2i(
            (int)MathF.Floor(pos.X / Chunk.SizeX),
            (int)MathF.Floor(pos.Z / Chunk.SizeZ)
        );
    }

    #endregion
}