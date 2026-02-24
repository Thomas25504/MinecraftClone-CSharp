using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace OpenTKTest;

public class Game : GameWindow
{

    Shader? shader;
    Matrix4 projection;

    public Camera camera;

    private int vao;
    private int vbo;
    private bool _firstMove = true;
    private Vector2 _lastMousePos;
    private float _speed = 5f;
    private float _sensitivity = 0.2f;
    
    private bool _cursorLocked = true;

    float[] vertices = { // Cube vertices (36 vertices for 6 faces)
        // Front face
        -0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,

        // Back face
        -0.5f, -0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,

        // Top face
        -0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,

        // Bottom face
        -0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f, -0.5f,

        // Right face
         0.5f, -0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f, -0.5f, -0.5f,

        // Left face
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f
    };


    // Constructor for the Game class, which initializes the game window with specified width, height, and title.
    public Game(int width, int height, string title)
     : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    // Override the OnLoad method to set up the OpenGL context and any necessary resources when the game starts.
    protected override void OnLoad()
    {
        base.OnLoad();
        

        camera = new Camera(new Vector3(0, 0, 5), Size.X / (float)Size.Y);  
        
        // Capture and hide the cursor for FPS-style camera control
        CursorState = CursorState.Grabbed;
        
        // Set the clear color for the OpenGL context to a light blue color.
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        //GL.Enable(EnableCap.DepthTest);

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
        
        // Update view matrix based on camera position
        Matrix4 view = camera.GetViewMatrix();

        shader.SetMatrix4("model", Matrix4.Identity);
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        
        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

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

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        // Call the base class's OnUpdateFrame method to ensure any necessary updates are performed.
        base.OnUpdateFrame(args);

        // Close the game window if the Escape key is pressed.
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Console.WriteLine("Escape key pressed. Closing the game.");
            Close();
        }
        
        // Toggle cursor lock with R key
        if (KeyboardState.IsKeyPressed(Keys.R))
        {
            _cursorLocked = !_cursorLocked;
            CursorState = _cursorLocked ? CursorState.Grabbed : CursorState.Normal;
            if (!_cursorLocked)
            {
                _firstMove = true; // Reset first move when unlocking to avoid camera jump
            }
        }

        float delta = (float)args.Time;
        var input = KeyboardState;

        if (input.IsKeyDown(Keys.W))
            camera.Position += camera.Front * _speed * delta;
        if (input.IsKeyDown(Keys.S))
            camera.Position -= camera.Front * _speed * delta;
        if (input.IsKeyDown(Keys.A))
            camera.Position -= camera.Right * _speed * delta;
        if (input.IsKeyDown(Keys.D))
            camera.Position += camera.Right * _speed * delta;
        if (input.IsKeyDown(Keys.Space))
            camera.Position += Vector3.UnitY * _speed * delta;
        if (input.IsKeyDown(Keys.LeftShift))
            camera.Position -= Vector3.UnitY * _speed * delta;

        // Only update camera rotation if cursor is locked
        if (_cursorLocked)
        {
            var mouse = MouseState;
            if (_firstMove)
            {
                _lastMousePos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                float dx = mouse.X - _lastMousePos.X;
                float dy = mouse.Y - _lastMousePos.Y;
                _lastMousePos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += dx * _sensitivity;
                camera.Pitch -= dy * _sensitivity;
            }
        }

    }


    protected override void OnUnload()
    {
        base.OnUnload();

        shader.Dispose();
    }
}