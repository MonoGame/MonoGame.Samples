using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Core
{
    /// <summary>
    /// Represents the visual appearance and collision behavior of a tile.
    /// </summary>
    struct Tile
    {
        /// <summary>
        /// The texture that represents the tile's visual appearance.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Specifies the color tint to apply to the Texture when drawing.
        /// </summary>
        public Color TintColor = Color.White;

        /// <summary>
        /// The type of collision behavior this tile exhibits.
        /// </summary>
        public TileCollision Collision;

        /// <summary>
        /// The standard width of a tile, measured in pixels.
        /// </summary>
        public const int Width = 40;

        /// <summary>
        /// The standard height of a tile, measured in pixels.
        /// </summary>
        public const int Height = 32;

        /// <summary>
        /// The size of a tile as a <see cref="Vector2"/> for convenience.
        /// </summary>
        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> struct.
        /// </summary>
        /// <param name="texture">The texture representing the tile's appearance.<see cref="Texture2D"/></param>
        /// <param name="collision">The collision type that defines the tile's behavior.<see cref="TileCollision"/></param>
        public Tile(Texture2D texture, TileCollision collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }
}