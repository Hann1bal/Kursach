using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Kursach;

internal class Program
{
    private static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "LearnOpenGL with Silk.NET";
        using var engine = new OpenGLEngine();
        OpenGLEngine.Run(options);
    }
}