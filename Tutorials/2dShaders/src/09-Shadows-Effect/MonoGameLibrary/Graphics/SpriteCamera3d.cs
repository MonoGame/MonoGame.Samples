using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics;

public class SpriteCamera3d
{
    /// <summary>
    /// The field of view for the camera.
    /// </summary>
    public int Fov { get; set; } = 120;
    
    /// <summary>
    /// By default, the camera is looking at the center of the screen.
    /// This offset value can be used to "turn" the camera from the center towards the given vector value.
    /// </summary>
    public Vector2 LookOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Produce a matrix that will transform world-space coordinates into clip-space coordinates.
    /// </summary>
    /// <returns></returns>
    public Matrix CalculateMatrixTransform()
    {
        var viewport = Core.GraphicsDevice.Viewport;
        
        // start by creating the projection matrix
        var projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView: MathHelper.ToRadians(Fov),
            aspectRatio: Core.GraphicsDevice.Viewport.AspectRatio,
            nearPlaneDistance: 0.0001f,
            farPlaneDistance: 10000f
        );

        // position the camera far enough away to see the entire contents of the screen
        var cameraZ = (viewport.Height * 0.5f) / (float)Math.Tan(MathHelper.ToRadians(Fov) * 0.5f);
        
        // create a view that is centered on the screen
        var center = .5f * new Vector2(viewport.Width, viewport.Height);
        var look = center + LookOffset;
        var view = Matrix.CreateLookAt(
            cameraPosition: new Vector3(center.X, center.Y, -cameraZ),    
            cameraTarget: new Vector3(look.X, look.Y, 0),
            cameraUpVector: Vector3.Down
        );

        // the standard matrix format is world*view*projection, 
        //  but given that we are skipping the world matrix, its just view*projection
        return view * projection;
    }
}