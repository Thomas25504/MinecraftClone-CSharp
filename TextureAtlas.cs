using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using System.IO;

namespace OpenTKTest;

public class TextureAtlas
{
    public int Handle { get; private set; }
    public const int TilesPerRow = 16; // 16x16 atlas

    public TextureAtlas(string path)
    {
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);

        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data
        );

        // IMPORTANT for voxel games
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public static Vector2[] GetUVs(int tileIndex)
    {
        int x = tileIndex % TilesPerRow;
        int y = tileIndex / TilesPerRow;

        float size = 1f / TilesPerRow;

        float u = x * size;
        float v = y * size;

        return new[]
        {
            new Vector2(u,        v + size),
            new Vector2(u + size, v + size),
            new Vector2(u + size, v),

            new Vector2(u + size, v),
            new Vector2(u,        v),
            new Vector2(u,        v + size)
        };
    }
}