namespace OpenTKTest;

public class Block
{
    public BlockType Type; // Type of block

    public bool IsSolid => Type != BlockType.Air; // Air is not solid, everything else is
    public bool IsTransparent => Type == BlockType.Glass;

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
        TopTexture = 2,
        SideTexture = 2,
        BottomTexture = 2
    };

    public static Block Grass => new Block
    {
        Type = BlockType.Grass,
        TopTexture = 0,
        SideTexture = 1,
        BottomTexture = 2
    };

    public static Block Stone => new Block
    {
        Type = BlockType.Stone,
        TopTexture = 3,
        SideTexture = 3,
        BottomTexture = 3
    };

    public static Block Wood => new Block
    {
        Type = BlockType.Wood,
        TopTexture = 4,
        SideTexture = 4,
        BottomTexture = 4
    };

    public static Block Sand => new Block
    {
        Type = BlockType.Sand,
        TopTexture = 5,
        SideTexture = 5,
        BottomTexture = 5
    };

    public static Block Glass => new Block
    {
        Type = BlockType.Glass,
        TopTexture = 6,
        SideTexture = 6,
        BottomTexture = 6
    };

    public static Block Pink_Heart => new Block
    {
        Type = BlockType.Pink_Heart,
        TopTexture = 7,
        SideTexture = 7,
        BottomTexture = 7
    };

        public static Block FromType(BlockType type) => type switch
    {
        BlockType.Dirt  => Dirt,
        BlockType.Grass => Grass,
        BlockType.Sand  => Sand,
        BlockType.Wood  => Wood,
        BlockType.Stone => Stone,
        BlockType.Glass => Glass,
        BlockType.Pink_Heart => Pink_Heart,
        BlockType.Air   => Air,
        _ => Air
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