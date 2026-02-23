using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKTest;

public class Game : GameWindow
{
    // Constructor for the Game class, which initializes the game window with specified width, height, and title.
    public Game(int width, int height, string title)
     : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

    //
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
}