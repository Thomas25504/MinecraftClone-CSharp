namespace OpenTKTest;

public class Block
{
    public BlockType Type; // Type of block

    public bool IsSolid => Type != BlockType.Air; // Air is not solid, everything else is

    // Atlas tile indices
    public int TopTexture;
    public int SideTexture;
    public int BottomTexture;

    // Predefined blocks for easy access
    public static Block Air => new Block { Type = BlockType.Air };

    // Grass block with specific textures for top, sides, and bottom
    public static Block Dirt => new Block
    {
        Type = BlockType.Dirt,
        TopTexture = 0,
        SideTexture = 1,
        BottomTexture = 2
    };

    //Get the correct texture index based on the face being rendered
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