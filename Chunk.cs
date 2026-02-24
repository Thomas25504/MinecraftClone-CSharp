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
        Position = position;
        GenerateBlocks();
        RebuildMesh();
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
        for (int x = 0; x < Size; x++) // Loop through each block in the chunk
        for (int y = 0; y < Size; y++) 
        for (int z = 0; z < Size; z++)
        {
            Blocks[x, y, z] = new Block 
            {
                // Simple flat terrain: dirt up to y=4, then air
                Type = y < 4 ? BlockType.Dirt : BlockType.Air 
            };
        }
    }

    #endregion

    #region Mesh Generation

    // Predefined vertices for each face of a cube (6 faces, 2 triangles per face)
    static readonly Vector3[][] Faces =
    {
        // Front
        new[]
        {
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
        },

        // Back
        new[]
        {
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
        },

        // Top
        new[]
        {
            new Vector3(-0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3(-0.5f, 0.5f,-0.5f),
        },

        // Bottom
        new[]
        {
            new Vector3(-0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f, 0.5f),
            new Vector3(-0.5f,-0.5f,-0.5f),
        },

        // Right
        new[]
        {
            new Vector3( 0.5f,-0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f,-0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f, 0.5f),
            new Vector3( 0.5f,-0.5f,-0.5f),
        },

        // Left
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

    private void BuildMesh()
    {
        List<Vector3> verts = new();

        for (int x = 0; x < Size; x++)
        for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            if (!Blocks[x, y, z].IsSolid)
                continue;

            for (int f = 0; f < 6; f++)
            {
                Vector3i n = new(x + Neighbors[f].X, y + Neighbors[f].Y, z + Neighbors[f].Z);

                if (IsFaceVisible(x, y, z, f))
                {
                    foreach (var v in Faces[f])
                        verts.Add(v + new Vector3(x, y, z));
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

    private void UploadMesh(List<Vector3> verts)
    {
        float[] data = new float[verts.Count * 3];

        for (int i = 0; i < verts.Count; i++)
        {
            data[i * 3 + 0] = verts[i].X;
            data[i * 3 + 1] = verts[i].Y;
            data[i * 3 + 2] = verts[i].Z;
        }

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        vertexCount = data.Length / 3;
    }

    public void Render(Shader shader)
    {
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