// See https://aka.ms/new-console-template for more information

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Kursach;

internal class Programm
{
    private static void Main(string[] args)
    {
        var nativeWindowSettings = new NativeWindowSettings
        {
            Size = new Vector2i(1000, 1000),
            Title = "FirstGameEngineTry",
            Flags = ContextFlags.Offscreen
        };
        using (var game = new OpenGLEngine(GameWindowSettings.Default, nativeWindowSettings))
        {
            game.VSync = VSyncMode.Adaptive;
            game.Run();
        }
    }
}