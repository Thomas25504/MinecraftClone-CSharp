using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTKTest;

public class Chunk
{   // Reference to the world (used for neighbor checks)
    private World world;

    // Size of the chunk in blocks (16x16x16)
    public const int Size = 16;

    public Block[,,] Blocks = new Block[Size, Size, Size]; // 3D array to hold block data
    public Vector3 Position; // World position of the chunk (in block coordinates)

    private int vao; // OpenGL Vertex Array Object
    private int vbo; // OpenGL Vertex Buffer Object
    private int vertexCount; // Number of vertices in the mesh

    // Constructor generates blocks and builds the mesh
    public Chunk(World world, Vector3 position)
     {
        this.world = world;
        this.Position = position;
        GenerateBlocks();
        BuildMesh();
     }
    

    public void RebuildMesh()
    {
        if (vao != 0)
        {
            GL.DeleteVertexArray(vao);
            vao = 0;
        }

        if (vbo != 0)
        {
            GL.DeleteBuffer(vbo);
            vbo = 0;
        }

        BuildMesh();
    }

    #region Block Generation 

    private void GenerateBlocks()
{
    for (int x = 0; x < Size; x++)
    for (int z = 0; z < Size; z++)
    {
        int worldX = (int)Position.X + x;
        int worldZ = (int)Position.Z + z;

        int height = Terrain.GetHeight(worldX, worldZ);

        for (int y = 0; y < Size; y++)
        {
            int worldY = (int)Position.Y + y;

            if (worldY > height)
            {
                Blocks[x, y, z] = Block.Air;
            }
            else if (worldY == height)
            {
                Blocks[x, y, z] = Block.Dirt; // later: Grass
            }
            else
            {
                Blocks[x, y, z] = Block.Dirt;
            }
        }
    }
}

    #endregion

    #region Mesh Generation

    // Predefined vertices for each face of a cube (6 faces, 2 triangles per face)
    static readonly Vector3[][] Faces =
    {
        // Front (looking from +Z)
        new[]
        {
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
        },

        // Back (looking from -Z)
        new[]
        {
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
        },

        // Top (looking from +Y)
        new[]
        {
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
        },

        // Bottom (looking from -Y)
        new[]
        {
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
        },

        // Right (looking from +X)
        new[]
        {
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
        },

        // Left (looking from -X)
        new[]
        {
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
        }
    };

    static readonly Vector2[] FaceUVs =
    {
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0),
        new Vector2(1, 0),
        new Vector2(0, 0),
        new Vector2(0, 1)
    };

    // Neighbor offsets for each face direction (used to check if a face should be rendered)
    static readonly Vector3i[] Neighbors =
    {
        new(0, 0, 1),
        new(0, 0,-1),
        new(0, 1, 0),
        new(0,-1, 0),
        new(1, 0, 0),
        new(-1,0, 0),
    };

     static readonly float[] FaceBrightness =
        {
            0.8f,  // Front
            0.8f,  // Back
            1.0f,  // Top    (brightest)
            0.3f,  // Bottom (darkest)
            0.6f,  // Right
            0.6f,  // Left
        };

    private void BuildMesh()
    {
        List<float> verts = new();

        for (int x = 0; x < Size; x++)
        for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            Block block = Blocks[x, y, z];
            if (!block.IsSolid)
                continue;

            for (int f = 0; f < 6; f++)
            {
                if (!IsFaceVisible(x, y, z, f))
                    continue;

                Vector2[] uvs = TextureAtlas.GetUVs(block.GetTextureForFace(f));

                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = Faces[f][i] + new Vector3(x, y, z);
                    Vector2 uv  = uvs[i];

                    verts.Add(pos.X);
                    verts.Add(pos.Y);
                    verts.Add(pos.Z);
                    verts.Add(uv.X);
                    verts.Add(uv.Y);
                    verts.Add(FaceBrightness[f]);
                }
            }
        }

        UploadMesh(verts);
    }

    private bool IsFaceVisible(int x, int y, int z, int face)
    {
        // Neighbor block in local coordinates
        Vector3i neighborLocal = new(
            x + Neighbors[face].X,
            y + Neighbors[face].Y,
            z + Neighbors[face].Z
        );

        // Convert to world coordinates
        Vector3i neighborWorld = LocalToWorld(
            neighborLocal.X,
            neighborLocal.Y,
            neighborLocal.Z
        );

        // Only render face if neighbor is air or chunk not loaded
        return !world.IsBlockSolid(neighborWorld);
    }

    private void UploadMesh(List<float> verts)
    {
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            verts.Count * sizeof(float),
            verts.ToArray(),
            BufferUsageHint.StaticDraw
        );

        // Position
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // UV
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Brightness
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), 5 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        vertexCount = verts.Count / 6;
    }

    public void Render(Shader shader, TextureAtlas atlas)
    {
        shader.Use();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, atlas.Handle);

        shader.SetInt("atlas", 0);
        shader.SetMatrix4("model", Matrix4.CreateTranslation(Position));

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
    }

    private Vector3i LocalToWorld(int x, int y, int z)
    {
        return new Vector3i(
            (int)Position.X + x,
            (int)Position.Y + y,
            (int)Position.Z + z
        );
    }

}
#endregion