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

    private int crosshairVao, crosshairVbo;
    private Shader crosshairShader;
    
    private int outlineVao, outlineVbo;
    private Shader outlineShader;

    private bool firstMove = true; // For mouse look initialization
    private Vector2 lastMousePos; // Last mouse position for calculating deltas

    private float speed = 10f; // Movement speed in units per second
    private float sensitivity = 0.2f; // Mouse sensitivity for looking around

    private bool cursorLocked = true; // Whether the cursor is currently locked for mouse look
    private bool wireframe = false; // Whether to render in wireframe mode (for debugging)

    private BlockType selectedBlock = BlockType.Dirt;
    private bool leftMouseWasUp = true;
    private bool rightMouseWasUp = true;

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

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        CursorState = CursorState.Grabbed; // Start with cursor locked for mouse look

        // Camera setup
        float aspect = Size.X / (float)Size.Y;
        camera = new Camera(new Vector3(8, 12, 30), aspect);

        crosshairShader = new Shader("Shaders/crosshair.vert", "Shaders/crosshair.frag");
        InitCrosshair();

        // Shader setup
        shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        outlineShader = new Shader("Shaders/outline.vert", "Shaders/outline.frag");
        InitOutline();

        // World + chunk setup
        atlas = new TextureAtlas("Assets/Textures/Atlas.png");
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
            chunk.Render(shader, atlas, transparent: false); // Render opaque blocks first
        }

    
        RenderOutline();
        

        GL.DepthMask(false); // Disable depth writing for transparent blocks
        foreach (var chunk in world.Chunks.Values)
        {
            chunk.Render(shader, atlas, transparent: true); // Render transparent blocks last
        }
        GL.DepthMask(true); // Re-enable depth writing

        // Draw crosshair
        GL.Disable(EnableCap.DepthTest);
        crosshairShader.Use();
        GL.BindVertexArray(crosshairVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 12);
        GL.Enable(EnableCap.DepthTest);

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

        //Block Selection
        // Block selection with number keys
        if (input.IsKeyPressed(Keys.D1)) selectedBlock = BlockType.Dirt;
        if (input.IsKeyPressed(Keys.D2)) selectedBlock = BlockType.Stone;
        if (input.IsKeyPressed(Keys.D3)) selectedBlock = BlockType.Glass;
        if (input.IsKeyPressed(Keys.D4)) selectedBlock = BlockType.Wood;
        if (input.IsKeyPressed(Keys.D5)) selectedBlock = BlockType.Sand;
        if (input.IsKeyPressed(Keys.D6)) selectedBlock = BlockType.Pink_Heart;

         // Handle mouse input for placing/removing blocks
        // Left click - remove block
        if (MouseState.IsButtonDown(MouseButton.Left) && leftMouseWasUp)
        {
            leftMouseWasUp = false;
            if (Raycast(out Vector3i hitPos, out _))
            {
                world.SetBlock(hitPos, Block.Air);
            }
        }
        if (!MouseState.IsButtonDown(MouseButton.Left)) leftMouseWasUp = true;

        // Right click - place block
        if (MouseState.IsButtonDown(MouseButton.Right) && rightMouseWasUp)
        {
            rightMouseWasUp = false;
            if (Raycast(out Vector3i hitPos, out Vector3i placePos))
            {
                world.SetBlock(placePos, Block.FromType(selectedBlock));
            }
        }
        if (!MouseState.IsButtonDown(MouseButton.Right)) rightMouseWasUp = true;

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
        crosshairShader.Dispose();
        GL.DeleteVertexArray(crosshairVao);
        GL.DeleteBuffer(crosshairVbo);
        outlineShader.Dispose();
        GL.DeleteVertexArray(outlineVao);
        GL.DeleteBuffer(outlineVbo);
    }


    private const float EyeHeight = 0f;

    private bool Raycast(out Vector3i hitPos, out Vector3i placePos)
    {
        hitPos = Vector3i.Zero;
        placePos = Vector3i.Zero;

        float reach = 6f;
        Vector3 pos = new Vector3(
            camera.Position.X,
            camera.Position.Y + EyeHeight,
            camera.Position.Z
        ) + camera.Front * 0.1f;
        Vector3 dir = camera.Front;

        Vector3i block = new(
            (int)MathF.Floor(pos.X),
            (int)MathF.Floor(pos.Y),
            (int)MathF.Floor(pos.Z)
        );

        // Remember starting block 
        Vector3i startBlock = block;

        int stepX = dir.X >= 0 ? 1 : -1;
        int stepY = dir.Y >= 0 ? 1 : -1;
        int stepZ = dir.Z >= 0 ? 1 : -1;

        float tDeltaX = MathF.Abs(1f / dir.X);
        float tDeltaY = MathF.Abs(1f / dir.Y);
        float tDeltaZ = MathF.Abs(1f / dir.Z);

        float tMaxX = (stepX > 0 ? (MathF.Floor(pos.X) + 1 - pos.X) : (pos.X - MathF.Floor(pos.X))) * tDeltaX;
        float tMaxY = (stepY > 0 ? (MathF.Floor(pos.Y) + 1 - pos.Y) : (pos.Y - MathF.Floor(pos.Y))) * tDeltaY;
        float tMaxZ = (stepZ > 0 ? (MathF.Floor(pos.Z) + 1 - pos.Z) : (pos.Z - MathF.Floor(pos.Z))) * tDeltaZ;

        Vector3i last = block;

        while (true)
        {
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                if (tMaxX > reach) break;
                last = block;
                block.X += stepX;
                tMaxX += tDeltaX;
            }
            else if (tMaxY < tMaxZ)
            {
                if (tMaxY > reach) break;
                last = block;
                block.Y += stepY;
                tMaxY += tDeltaY;
            }
            else
            {
                if (tMaxZ > reach) break;
                last = block;
                block.Z += stepZ;
                tMaxZ += tDeltaZ;
            }

            if (world.IsBlockSolid(block) && block != startBlock)
            {
                hitPos = block;
                placePos = last;
                return true;
            }
        }

        return false;
    }

    private void InitCrosshair()
    {
        float size = 0.02f;
        float thickness = 0.003f;

        float[] verts =
        {
            // Horizontal bar
            -size, -thickness,
            size, -thickness,
            size,  thickness,
            size,  thickness,
            -size,  thickness,
            -size, -thickness,

            // Vertical bar
            -thickness, -size,
            thickness, -size,
            thickness,  size,
            thickness,  size,
            -thickness,  size,
            -thickness, -size,
        };

        crosshairVao = GL.GenVertexArray();
        crosshairVbo = GL.GenBuffer();

        GL.BindVertexArray(crosshairVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, crosshairVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    private void InitOutline()
    {
        float s = 0.501f; // Slightly larger than 0.5 to avoid z-fighting

        float[] verts =
        {
            // Bottom face
            0,0,0,  1,0,0,
            1,0,0,  1,0,1,
            1,0,1,  0,0,1,
            0,0,1,  0,0,0,

            // Top face
            0,1,0,  1,1,0,
            1,1,0,  1,1,1,
            1,1,1,  0,1,1,
            0,1,1,  0,1,0,

            // Verticals
            0,0,0,  0,1,0,
            1,0,0,  1,1,0,
            1,0,1,  1,1,1,
            0,0,1,  0,1,1,
        };

        outlineVao = GL.GenVertexArray();
        outlineVbo = GL.GenBuffer();

        GL.BindVertexArray(outlineVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, outlineVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    private void RenderOutline()
    {
        if (!Raycast(out Vector3i hitPos, out _))
            return;

        // Block vertices are centered at integer coords after chunk translation
        // so outline just needs to sit at hitPos with no offset
        Matrix4 model = Matrix4.CreateTranslation(hitPos.X, hitPos.Y, hitPos.Z);

        outlineShader.Use();
        outlineShader.SetMatrix4("model", model);
        outlineShader.SetMatrix4("view", camera.GetViewMatrix());
        outlineShader.SetMatrix4("projection", camera.GetProjectionMatrix());

        GL.DepthMask(false);
        GL.BindVertexArray(outlineVao);
        GL.LineWidth(2f);
        GL.DrawArrays(PrimitiveType.Lines, 0, 24);
        GL.DepthMask(true);
    }
}