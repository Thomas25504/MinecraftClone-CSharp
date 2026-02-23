using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace OpenTKTest;

public class Game : GameWindow
{

    Shader? shader;
    Matrix4 projection;

    float[] vertices = { // Triangle vertices
        -0.5f, -0.5f, 0.0f, //Bottom-left vertex
        0.5f, -0.5f, 0.0f, //Bottom-right vertex
        0.0f,  0.5f, 0.0f  //Top vertex
    };

    int vbo; // Vertex Buffer Object
    int vao; // Vertex Array Object

    // Constructor for the Game class, which initializes the game window with specified width, height, and title.
    public Game(int width, int height, string title)
     : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    // Override the OnLoad method to set up the OpenGL context and any necessary resources when the game starts.
    protected override void OnLoad()
    {
        base.OnLoad();

    
        // Set the clear color for the OpenGL context to a light blue color.
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // Generate and bind a Vertex Array Object (VAO) to store the vertex attribute configuration.
        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        shader = new Shader("shader.vert", "shader.frag");

        shader.Use();
        
        // Set up projection matrix to handle aspect ratio (must be after shader.Use())
        float aspectRatio = (float)ClientSize.X / ClientSize.Y;
        projection = Matrix4.CreateOrthographic(aspectRatio * 2f, 2f, -1f, 1f);
        shader.SetMatrix4("projection", projection);
        
        
    }

    // Override the OnRenderFrame method to clear the screen and swap buffers for rendering each frame.
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        // Clear the color buffer to prepare for rendering the next frame.
        GL.Clear(ClearBufferMask.ColorBufferBit);

        shader.Use();
        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        // Swap the front and back buffers to display the rendered frame on the screen.
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Update the OpenGL viewport to match the new size of the game window.
        GL.Viewport(0, 0, e.Width, e.Height);
        
        // Update projection matrix to match new aspect ratio
        float aspectRatio = (float)e.Width / e.Height;
        projection = Matrix4.CreateOrthographic(aspectRatio * 2f, 2f, -1f, 1f);
        shader?.Use();
        shader?.SetMatrix4("projection", projection);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        // Call the base class's OnUpdateFrame method to ensure any necessary updates are performed.
        base.OnUpdateFrame(e);

        // Close the game window if the Escape key is pressed.
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Console.WriteLine("Escape key pressed. Closing the game.");
            Close();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        shader.Dispose();
    }
}