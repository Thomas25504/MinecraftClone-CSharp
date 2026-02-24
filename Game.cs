using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKTest;

public class Game : GameWindow
{
    private Shader shader; // Simple shader for rendering chunks
    private Camera camera; // First-person camera
    private World world; // Manages chunks and world data
    private TextureAtlas atlas; // Texture atlas for block textures

    private bool firstMove = true; // For mouse look initialization
    private Vector2 lastMousePos; // Last mouse position for calculating deltas

    private float speed = 10f; // Movement speed in units per second
    private float sensitivity = 0.2f; // Mouse sensitivity for looking around

    private bool cursorLocked = true; // Whether the cursor is currently locked for mouse look
    private bool wireframe = false; // Whether to render in wireframe mode (for debugging)

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = (width, height), // Set the initial window size
                   Title = title
               })
    {
    }

    // Called when the game starts
    protected override void OnLoad()
    {
        base.OnLoad();

        // OpenGL state
        GL.ClearColor(0.4f, 0.7f, 1.0f, 1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        CursorState = CursorState.Grabbed; // Start with cursor locked for mouse look

        // Camera setup
        float aspect = Size.X / (float)Size.Y;
        camera = new Camera(new Vector3(8, 12, 30), aspect);

        // Shader setup
        shader = new Shader("shader.vert", "shader.frag");

        // World + chunk setup
        atlas = new TextureAtlas("Atlas.png");
        world = new World();
    }

    // Called every frame to render the scene
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        // Clear the screen
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Set shader uniforms
        shader.Use();
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());

        // Toggle wireframe mode if enabled
        if (wireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        else
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        // Render all loaded chunks
        foreach (var chunk in world.Chunks.Values)
        {
            chunk.Render(shader, atlas);
        }

        SwapBuffers();
    }

    // Called every frame to update game logic
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        // Don't process input if the window isn't focused
        if (!IsFocused)
            return;

        var input = KeyboardState;
        float delta = (float)e.Time;

        // Exit
        if (input.IsKeyDown(Keys.Escape))
            Close();

        // Toggle cursor lock
        if (input.IsKeyPressed(Keys.R))
        {
            cursorLocked = !cursorLocked;
            CursorState = cursorLocked ? CursorState.Grabbed : CursorState.Normal;
            firstMove = true;
        }

        // Toggle wireframe
        if (input.IsKeyPressed(Keys.F))
        {
            wireframe = !wireframe;
        }

        // Movement
        if (input.IsKeyDown(Keys.W))
            camera.Position += camera.Front * speed * delta;
        if (input.IsKeyDown(Keys.S))
            camera.Position -= camera.Front * speed * delta;
        if (input.IsKeyDown(Keys.A))
            camera.Position -= camera.Right * speed * delta;
        if (input.IsKeyDown(Keys.D))
            camera.Position += camera.Right * speed * delta;
        if (input.IsKeyDown(Keys.Space))
            camera.Position += Vector3.UnitY * speed * delta;
        if (input.IsKeyDown(Keys.LeftShift))
            camera.Position -= Vector3.UnitY * speed * delta;

        // Mouse look
        if (cursorLocked)
        {
            var mouse = MouseState;

            if (firstMove)
            {
                lastMousePos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                float dx = mouse.X - lastMousePos.X;
                float dy = mouse.Y - lastMousePos.Y;
                lastMousePos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += dx * sensitivity;
                camera.Pitch -= dy * sensitivity;
            }
        }

        world.Update(camera.Position); // Load/unload chunks based on player position
    }

    // Called when the window is resized
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Update the viewport and camera aspect ratio
        GL.Viewport(0, 0, e.Width, e.Height);
        camera.AspectRatio = e.Width / (float)e.Height;
    }

    // Called when the game is closing
    protected override void OnUnload()
    {
        base.OnUnload();
        shader.Dispose(); // Clean up shader resources
    }
}