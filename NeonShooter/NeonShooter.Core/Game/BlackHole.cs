//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NeonShooter
{
    class BlackHole : Entity
	{
		private static Random rand = new Random();

		private int hitpoints = 10;
		private float sprayAngle = 0;

		public BlackHole(Vector2 position)
		{
			image = Art.BlackHole;
			Position = position;
			Radius = image.Width / 2f;
		}

		public override void Update()
		{
			var entities = EntityManager.GetNearbyEntities(Position, 250);

			foreach (var entity in entities)
			{
				if (entity is Enemy && !(entity as Enemy).IsActive)
					continue;

				// bullets are repelled by black holes and everything else is attracted
				if (entity is Bullet)
					entity.Velocity += (entity.Position - Position).ScaleTo(0.3f);
				else
				{
					var dPos = Position - entity.Position;
					var length = dPos.Length();

					entity.Velocity += dPos.ScaleTo(MathHelper.Lerp(2, 0, length / 250f));
				}
			}

			// The black holes spray some orbiting particles. The spray toggles on and off every quarter second.
			if ((NeonShooterGame.GameTime.TotalGameTime.Milliseconds / 250) % 2 == 0)
			{
				Vector2 sprayVel = MathUtil.FromPolar(sprayAngle, rand.NextFloat(12, 15));
				Color color = ColorUtil.HSVToColor(5, 0.5f, 0.8f);	// light purple
				Vector2 pos = Position + 2f * new Vector2(sprayVel.Y, -sprayVel.X) + rand.NextVector2(4, 8);
				var state = new ParticleState() 
				{ 
					Velocity = sprayVel, 
					LengthMultiplier = 1, 
					Type = ParticleType.Enemy 
				};

				NeonShooterGame.ParticleManager.CreateParticle(Art.LineParticle, pos, color, 190, 1.5f, state);
			}
			
			// rotate the spray direction
			sprayAngle -= MathHelper.TwoPi / 50f;

			NeonShooterGame.Grid.ApplyImplosiveForce((float)Math.Sin(sprayAngle / 2) * 10 + 20, Position, 200);
		}

		public void WasShot()
		{
			hitpoints--;
			if (hitpoints <= 0)
			{
				IsExpired = true;
				PlayerStatus.AddPoints(5);
				PlayerStatus.IncreaseMultiplier();
			}

			
			float hue = (float)((3 * NeonShooterGame.GameTime.TotalGameTime.TotalSeconds) % 6);
			Color color = ColorUtil.HSVToColor(hue, 0.25f, 1);
			const int numParticles = 150;
			float startOffset = rand.NextFloat(0, MathHelper.TwoPi / numParticles);

			for (int i = 0; i < numParticles; i++)
			{
				Vector2 sprayVel = MathUtil.FromPolar(MathHelper.TwoPi * i / numParticles + startOffset, rand.NextFloat(8, 16));
				Vector2 pos = Position + 2f * sprayVel;
				var state = new ParticleState() 
				{ 
					Velocity = sprayVel, 
					LengthMultiplier = 1, 
					Type = ParticleType.IgnoreGravity 
				};

				NeonShooterGame.ParticleManager.CreateParticle(Art.LineParticle, pos, color, 90, 1.5f, state);
			}

			Sound.Explosion.Play(0.5f, rand.NextFloat(-0.2f, 0.2f), 0);
		}

		public void Kill()
		{
			hitpoints = 0;
			WasShot();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// make the size of the black hole pulsate
			float scale = 1 + 0.1f * (float)Math.Sin(10 * NeonShooterGame.GameTime.TotalGameTime.TotalSeconds);
			spriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, scale, 0, 0);
		}
	}
}