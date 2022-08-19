﻿using System.Numerics;
using Kursach.Common;
using Kursach.Common.MathHelperMethods;
using Kursach.ControlsUtils;
using Kursach.Engine.Graphics;
using Kursach.Engine.Objects;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = Kursach.Engine.Graphics.Shader;

namespace Kursach;

public class OpenGLEngine : IDisposable
{
    private static GL Gl;
    private static IKeyboard primaryKeyboard;

    private const int Width = 800;
    private const int Height = 700;
    private static DateTime StartTime;

    private static BufferObject<float> Vbo;
    private static BufferObject<uint> Ebo;
    private static VertexArrayObject<float, uint> VaoCube;
    private static Shader LightingShader;
    private static Shader LampShader;
    private static Vector3 LampPosition = new Vector3(1.2f, 1.0f, 2.0f);
    private static Camera Camera;

    //Used to track change in mouse movement to allow for moving of the Camera

    private static readonly float[] Vertices = VertexLoader.GetVerticles();

    private static readonly uint[] Indices = VertexLoader.GetIndices();
    private static IWindow window;

    private static void Init(WindowOptions options)
    {
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;
    }

    public static void Run(WindowOptions options)
    {
        Init(options);
        window.Run();
    }

    private static void OnLoad()
    {
        StartTime = DateTime.UtcNow;
        IInputContext input = window.CreateInput();
        primaryKeyboard = input.Keyboards.FirstOrDefault();
        if (primaryKeyboard != null)
        {
            primaryKeyboard.KeyDown += KeyDown;
        }
        for (int i = 0; i < input.Mice.Count; i++)
        {
            input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            input.Mice[i].MouseMove += OnMouseMove;
            input.Mice[i].Scroll += OnMouseWheel;
        }

        Gl = GL.GetApi(window);

        Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
        Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
        VaoCube = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

        VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        VaoCube.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);

        //The lighting shader will give our main cube it's colour multiplied by the light's intensity
        LightingShader = new Shader(Gl, "Shaders/shader.vert", "Shaders/lighting.frag");
        //The Lamp shader uses a fragment shader that just colours it solid white so that we know it is the light source
        LampShader = new Shader(Gl, "Shaders/shader.vert", "Shaders/shader.frag");

        //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
        Camera = new Camera(Vector3.UnitZ * 6, Vector3.UnitZ * -1, Vector3.UnitY, Width / Height);
    }

    private static void OnUpdate(double deltaTime)
    {
        EngineKeyboardEvents.Events(deltaTime, Camera, primaryKeyboard);
    }

    private static void OnRender(double deltaTime)
    {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            VaoCube.Bind();
            LightingShader.Use();

            //Slightly rotate the cube to give it an angled face to look at
            LightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(25f)));
            LightingShader.SetUniform("uView", Camera.GetViewMatrix());
            LightingShader.SetUniform("uProjection", Camera.GetProjectionMatrix());
            LightingShader.SetUniform("viewPos", Camera.Position);
            LightingShader.SetUniform("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
            LightingShader.SetUniform("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
            LightingShader.SetUniform("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            LightingShader.SetUniform("material.shininess", 32.0f);

            //Track the difference in time so we can manipulate variables as time changes
            var difference = (float) (DateTime.UtcNow - StartTime).TotalSeconds;
            var lightColor = Vector3.Zero;
            lightColor.X = MathF.Sin(difference * 2.0f);
            lightColor.Y = MathF.Sin(difference * 0.7f);
            lightColor.Z = MathF.Sin(difference * 1.3f);

            var diffuseColor = lightColor * new Vector3(0.5f);
            var ambientColor = diffuseColor * new Vector3(0.2f);

            LightingShader.SetUniform("light.ambient", ambientColor);
            LightingShader.SetUniform("light.diffuse", diffuseColor); // darkened
            LightingShader.SetUniform("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
            LightingShader.SetUniform("light.position", LampPosition);

            //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

            LampShader.Use();

            //The Lamp cube is going to be a scaled down version of the normal cubes verticies moved to a different screen location
            var lampMatrix = Matrix4x4.Identity;
            lampMatrix *= Matrix4x4.CreateScale(0.2f);
            lampMatrix *= Matrix4x4.CreateTranslation(LampPosition);

            LampShader.SetUniform("uModel", lampMatrix);
            LampShader.SetUniform("uView", Camera.GetViewMatrix());
            LampShader.SetUniform("uProjection", Camera.GetProjectionMatrix());

            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    private static void OnMouseMove(IMouse mouse, Vector2 position)
    {
    EngineMouseEvents.Events(Camera, mouse, position);
    }

    // private static unsafe void RenderLitCube()
    // {
    //     //Use the 'lighting shader' that is capable of modifying the cubes colours based on ambient lighting and diffuse lighting
    //     LightingShader.Use();
    //
    //     //Set up the uniforms needed for the lighting shaders to be able to draw and light the coral cube
    //     LightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(25f)));
    //     LightingShader.SetUniform("uView", Camera.GetViewMatrix());
    //     LightingShader.SetUniform("uProjection", Camera.GetProjectionMatrix());
    //     LightingShader.SetUniform("objectColor", new Vector3(1.0f, 1.5f, 0.31f));
    //     LightingShader.SetUniform("lightColor", Vector3.One);
    //     LightingShader.SetUniform("lightPos", LampPosition);
    //
    //     //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
    //     Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    // }
    //
    // private static unsafe void RenderLampCube()
    // {
    //     //Use the 'main' shader that does not do any lighting calculations to just draw the cube to screen in the requested colours.
    //     LampShader.Use();
    //
    //     //The Lamp cube is going to be a scaled down version of the normal cubes verticies moved to a different screen location
    //     var lampMatrix = Matrix4x4.Identity;
    //     lampMatrix *= Matrix4x4.CreateScale(0.2f);
    //     lampMatrix *= Matrix4x4.CreateTranslation(LampPosition);
    //
    //     //Setup the uniforms needed to draw the Lamp in the correct place on screen
    //     LampShader.SetUniform("uModel", lampMatrix);
    //     LampShader.SetUniform("uView", Camera.GetViewMatrix());
    //     LampShader.SetUniform("uProjection", Camera.GetProjectionMatrix());
    //
    //     Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    // }

    private static void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        Camera.ModifyZoom(scrollWheel.Y);
    }

    private static void OnClose()
    {
        Vbo.Dispose();
        Ebo.Dispose();
        VaoCube.Dispose();
        LightingShader.Dispose();
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            window.Close();
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        OnClose();
        GC.Collect();
    }
}