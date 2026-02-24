using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKTest;

public class Game : GameWindow
{
    private Shader shader;
    private Camera camera;
    private World world;

    private bool firstMove = true;
    private Vector2 lastMousePos;

    private float speed = 10f;
    private float sensitivity = 0.2f;

    private bool cursorLocked = true;
    private bool wireframe = false;

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = (width, height),
                   Title = title
               })
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // OpenGL state
        GL.ClearColor(0.1f, 0.2f, 0.3f, 1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        CursorState = CursorState.Grabbed;

        // Camera (START ABOVE THE CHUNK)
        float aspect = Size.X / (float)Size.Y;
        camera = new Camera(new Vector3(8, 12, 30), aspect);

        // Shader
        shader = new Shader("shader.vert", "shader.frag");

        // World + chunk
        world = new World();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());

        if (wireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        else
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        // Render all loaded chunks
        foreach (var chunk in world.Chunks.Values)
        {
            chunk.Render(shader);
        }

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

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

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        camera.AspectRatio = e.Width / (float)e.Height;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        shader.Dispose();
    }
}