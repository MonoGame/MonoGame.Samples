using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class PointLight
{
    /// <summary>
    /// The position of the light in world space
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The color tint of the light
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// The radius of the light in pixels
    /// </summary>
    public int Radius { get; set; } = 250;

    public static Color PackVector2_SNorm(Vector2 vec)
    {
        // Clamp to [-1, 1)
        vec = Vector2.Clamp(vec, new Vector2(-1f), new Vector2(1f - 1f / 32768f));

        short xInt = (short)(vec.X * 32767f); // signed 16-bit
        short yInt = (short)(vec.Y * 32767f);

        byte r = (byte)((xInt >> 8) & 0xFF);
        byte g = (byte)(xInt & 0xFF);
        byte b = (byte)((yInt >> 8) & 0xFF);
        byte a = (byte)(yInt & 0xFF);

        return new Color(r, g, b, a);
    }
}