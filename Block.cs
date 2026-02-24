namespace OpenTKTest;

public class Block
{
    public BlockType Type;

    public bool IsSolid => Type != BlockType.Air;

    // Atlas tile indices
    public int TopTexture;
    public int SideTexture;
    public int BottomTexture;

    public static Block Air => new Block { Type = BlockType.Air };

    public static Block Dirt => new Block
    {
        Type = BlockType.Dirt,
        TopTexture = 0,
        SideTexture = 1,
        BottomTexture = 2
    };

    public int GetTextureForFace(int face)
    {
        return face switch
        {
            2 => TopTexture,     // Top
            3 => BottomTexture,  // Bottom
            _ => SideTexture     // Sides
        };
    }
}