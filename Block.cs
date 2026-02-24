namespace OpenTKTest;
// Simple struct to represent a block in the world
public struct Block
{
    public BlockType Type;

    public bool IsSolid => Type != BlockType.Air;
}