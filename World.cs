using OpenTK.Mathematics;

namespace OpenTKTest;

public class World
{
    public const int RenderDistance = 4;

    private static readonly Vector2i[] NeighborOffsets =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    };

    public Dictionary<Vector2i, Chunk> Chunks = new();

    public void Update(Vector3 playerPosition)
    {
        Vector2i playerChunk = WorldToChunk(playerPosition);

        LoadChunksAround(playerChunk);
        UnloadFarChunks(playerChunk);
    }

    #region Chunk Loading

    private void LoadChunksAround(Vector2i center)
    {
        for (int x = -RenderDistance; x <= RenderDistance; x++)
        for (int z = -RenderDistance; z <= RenderDistance; z++)
        {
            Vector2i coord = new(center.X + x, center.Y + z);

            if (!Chunks.ContainsKey(coord))
            {
                Vector3 worldPos = new Vector3(
                    coord.X * Chunk.Size,
                    0,
                    coord.Y * Chunk.Size
                );

                Chunk chunk = new Chunk(this, worldPos);
                Chunks.Add(coord, chunk);
                chunk.RebuildMesh();

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
            Chunks.Remove(key);
        }
    }

    public bool IsBlockSolid(Vector3i worldPos)
{
    int chunkX = (int)MathF.Floor(worldPos.X / (float)Chunk.Size);
    int chunkZ = (int)MathF.Floor(worldPos.Z / (float)Chunk.Size);

    Vector2i chunkCoord = new(chunkX, chunkZ);

    if (!Chunks.TryGetValue(chunkCoord, out Chunk chunk))
        return false; // chunk not loaded = air

    int localX = worldPos.X - chunkX * Chunk.Size;
    int localY = worldPos.Y;
    int localZ = worldPos.Z - chunkZ * Chunk.Size;

    // Bounds check
    if (localX < 0 || localX >= Chunk.Size ||
        localY < 0 || localY >= Chunk.Size ||
        localZ < 0 || localZ >= Chunk.Size)
        return false;

    return chunk.Blocks[localX, localY, localZ].IsSolid;
}

    #endregion

    #region Helpers

    private Vector2i WorldToChunk(Vector3 pos)
    {
        return new Vector2i(
            (int)MathF.Floor(pos.X / Chunk.Size),
            (int)MathF.Floor(pos.Z / Chunk.Size)
        );
    }

    #endregion
}