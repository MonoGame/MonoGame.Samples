using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics;

public class ShadowCaster
{
    /// <summary>
    /// The position of the shadow caster
    /// </summary>
    public Vector2 Position;
    
    /// <summary>
    /// A list of at least 2 points that will be used to create a closed loop shape.
    /// The points are relative to the position.
    /// </summary>
    public List<Vector2> Points;
    
    public static ShadowCaster SimplePolygon(Point position, float radius, int sides)
    {
        var anglePerSide = MathHelper.TwoPi / sides;
        var caster = new ShadowCaster
        {
            Position = position.ToVector2(),
            Points = new List<Vector2>(sides)
        };
        for (var angle = 0f; angle < MathHelper.TwoPi; angle += anglePerSide)
        {
            var pt = radius * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            caster.Points.Add(pt);
        }

        return caster;
    }
}