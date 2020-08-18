//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NeonShooter
{
    static class Art
	{
		public static Texture2D Player { get; private set; }
		public static Texture2D Seeker { get; private set; }
		public static Texture2D Wanderer { get; private set; }
		public static Texture2D Bullet { get; private set; }
		public static Texture2D Pointer { get; private set; }
		public static Texture2D BlackHole { get; private set; }

		public static Texture2D LineParticle { get; private set; }
		public static Texture2D Glow { get; private set; }
		public static Texture2D Pixel { get; private set; }		// a single white pixel

		public static SpriteFont Font { get; private set; }

		public static void Load(ContentManager content)
		{
			Player = content.Load<Texture2D>("Art/Player");
			Seeker = content.Load<Texture2D>("Art/Seeker");
			Wanderer = content.Load<Texture2D>("Art/Wanderer");
			Bullet = content.Load<Texture2D>("Art/Bullet");
			Pointer = content.Load<Texture2D>("Art/Pointer");
			BlackHole = content.Load<Texture2D>("Art/Black Hole");

			LineParticle = content.Load<Texture2D>("Art/Laser");
			Glow = content.Load<Texture2D>("Art/Glow");

			Pixel = new Texture2D(Player.GraphicsDevice, 1, 1);
			Pixel.SetData(new[] { Color.White });

			Font = content.Load<SpriteFont>("Font");
		}
	}
}