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

    public static void Draw(SpriteBatch spriteBatch, List<PointLight> pointLights, Texture2D normalBuffer)
    {
        spriteBatch.Begin(
            effect: Core.PointLightMaterial.Effect,
            blendState: BlendState.Additive
            );
        
        foreach (var light in pointLights)
        {
            var diameter = light.Radius * 2;
            var rect = new Rectangle((int)(light.Position.X - light.Radius), (int)(light.Position.Y - light.Radius), diameter, diameter);
            spriteBatch.Draw(normalBuffer, rect, light.Color);
        }
        
        spriteBatch.End();
    }
}