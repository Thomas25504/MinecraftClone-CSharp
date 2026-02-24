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

    private int vaoSolid, vboSolid, vertexCountSolid;
    private int vaoTransparent, vboTransparent, vertexCountTransparent;

    // Constructor generates blocks and builds the mesh
    public Chunk(World world, Vector3 position)
     {
        this.world = world;
        this.Position = position;
        GenerateBlocks();
        BuildMesh();
     }
    
    //Rebuilds the mesh (called when blocks are modified)    
    public void RebuildMesh()
    {
        if (vaoSolid != 0) { GL.DeleteVertexArray(vaoSolid); vaoSolid = 0; }
        if (vboSolid != 0) { GL.DeleteBuffer(vboSolid); vboSolid = 0; }
        if (vaoTransparent != 0) { GL.DeleteVertexArray(vaoTransparent); vaoTransparent = 0; }
        if (vboTransparent != 0) { GL.DeleteBuffer(vboTransparent); vboTransparent = 0; }

        BuildMesh();
    }

    #region Block Generation 

    // Simple terrain generation based on heightmap
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
                    Blocks[x, y, z] = Block.Grass; // later: Grass
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

    // Builds the mesh by iterating over all blocks and adding vertices for visible faces
    private void BuildMesh()
    {
        List<float> solidVerts = new();
        List<float> transparentVerts = new();

        for (int x = 0; x < Size; x++)
        for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            Block block = Blocks[x, y, z];
            if (!block.IsSolid) continue;

            List<float> target = block.IsTransparent ? transparentVerts : solidVerts;

            for (int f = 0; f < 6; f++)
            {
                if (!IsFaceVisible(x, y, z, f))
                    continue;

                Vector2[] uvs = TextureAtlas.GetUVs(block.GetTextureForFace(f));

                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = Faces[f][i] + new Vector3(x, y, z);
                    Vector2 uv  = uvs[i];

                    target.Add(pos.X);
                    target.Add(pos.Y);
                    target.Add(pos.Z);
                    target.Add(uv.X);
                    target.Add(uv.Y);
                    target.Add(FaceBrightness[f]);
                }
            }
        }

            UploadMesh(solidVerts, ref vaoSolid, ref vboSolid, ref vertexCountSolid);
            UploadMesh(transparentVerts, ref vaoTransparent, ref vboTransparent, ref vertexCountTransparent);
    }

    // Checks if a face of a block should be rendered by looking at the neighboring block
    private bool IsFaceVisible(int x, int y, int z, int face)
    {
        Vector3i neighborLocal = new(
            x + Neighbors[face].X,
            y + Neighbors[face].Y,
            z + Neighbors[face].Z
        );

        Vector3i neighborWorld = LocalToWorld(
            neighborLocal.X,
            neighborLocal.Y,
            neighborLocal.Z
        );

        // Also show face if neighbor is transparent
        Block neighbor = world.GetBlock(neighborWorld);
        return !neighbor.IsSolid || neighbor.IsTransparent;
    }

    // Uploads the vertex data to the GPU and sets up the vertex attributes
    private void UploadMesh(List<float> verts, ref int vao, ref int vbo, ref int vertexCount)
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

    // Renders the chunk by binding the VAO and drawing the triangles
    public void Render(Shader shader, TextureAtlas atlas, bool transparent)
    {
        shader.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, atlas.Handle);
        shader.SetInt("atlas", 0);
        shader.SetMatrix4("model", Matrix4.CreateTranslation(Position));

        if (transparent)
        {
            GL.BindVertexArray(vaoTransparent);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCountTransparent);
        }
        else
        {
            GL.BindVertexArray(vaoSolid);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCountSolid);
        }
    }

    // Converts local block coordinates to world coordinates by adding the chunk's position
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