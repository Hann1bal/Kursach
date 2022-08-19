using System.Numerics;
using Kursach.Common;
using Silk.NET.Input;

namespace Kursach.ControlsUtils;

public class EngineMouseEvents
{
    private static Vector2 LastMousePosition;

    public static void Events(Camera aCamera, IMouse mouse, Vector2 position)
    {
        const float lookSensitivity = 0.1f;
        if (LastMousePosition == default)
        {
            LastMousePosition = position;
        }
        else
        {
            var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
            var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
            LastMousePosition = position;

            aCamera.ModifyDirection(xOffset, yOffset);
        }
    }
}