using System.Numerics;
using Kursach.Common;
using Silk.NET.Input;

namespace Kursach.ControlsUtils;

public static class EngineKeyboardEvents
{
    public static void Events(double deltaTime, Camera aCamera, IKeyboard primaryKeyboard)
    {
        var moveSpeed = 2.5f * (float) deltaTime;

        if (primaryKeyboard.IsKeyPressed(Key.W))
        {
            //Move forwards
            aCamera.Position += moveSpeed * aCamera.Front;
        }

        if (primaryKeyboard.IsKeyPressed(Key.S))
        {
            //Move backwards
            aCamera.Position -= moveSpeed * aCamera.Front;
        }

        if (primaryKeyboard.IsKeyPressed(Key.A))
        {
            //Move left
            aCamera.Position -= Vector3.Normalize(Vector3.Cross(aCamera.Front, aCamera.Up)) * moveSpeed;
        }

        if (primaryKeyboard.IsKeyPressed(Key.D))
        {
            //Move right
            aCamera.Position += Vector3.Normalize(Vector3.Cross(aCamera.Front, aCamera.Up)) * moveSpeed;
        }
    }
}