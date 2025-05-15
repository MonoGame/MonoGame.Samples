using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class Sprite
{
    /// <summary>
    /// Gets or Sets the source texture region represented by this sprite.
    /// </summary>
    public TextureRegion Region { get; set; }

    /// <summary>
    /// Gets or Sets the color mask to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Color.White
    /// </remarks>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or Sets the amount of rotation, in radians, to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float Rotation { get; set; } = 0.0f;

    /// <summary>
    /// Gets or Sets the scale factor to apply to the x- and y-axes when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.One
    /// </remarks>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or Sets the xy-coordinate origin point, relative to the top-left corner, of this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.Zero
    /// </remarks>
    public Vector2 Origin { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or Sets the sprite effects to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is SpriteEffects.None
    /// </remarks>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;

    /// <summary>
    /// Gets or Sets the layer depth to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float LayerDepth { get; set; } = 0.0f;

    /// <summary>
    /// Gets the width, in pixels, of this sprite.
    /// </summary>
    /// <remarks>
    /// Width is calculated by multiplying the width of the source texture region by the x-axis scale factor.
    /// </remarks>
    public float Width => Region.Width * Scale.X;

    /// <summary>
    /// Gets the height, in pixels, of this sprite.
    /// </summary>
    /// <remarks>
    /// Height is calculated by multiplying the height of the source texture region by the y-axis scale factor.
    /// </remarks>
    public float Height => Region.Height * Scale.Y;

    /// <summary>
    /// Creates a new sprite.
    /// </summary>
    public Sprite() { }

    /// <summary>
    /// Creates a new sprite using the specified source texture region.
    /// </summary>
    /// <param name="region">The texture region to use as the source texture region for this sprite.</param>
    public Sprite(TextureRegion region)
    {
        Region = region;
    }

    /// <summary>
    /// Sets the origin of this sprite to the center
    /// </summary>
    public void CenterOrigin()
    {
        Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
    }

    /// <summary>
    /// Submit this sprite for drawing to the current batch.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch instance used for batching draw calls.</param>
    /// <param name="position">The xy-coordinate position to render this sprite at.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        Region.Draw(spriteBatch, position, Color, Rotation, Origin, Scale, Effects, LayerDepth);
    }
}
