//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace NeonShooter
{
    class Enemy : Entity
	{
		public static Random rand = new Random();

		private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
		private int timeUntilStart = 60;
		public bool IsActive { get { return timeUntilStart <= 0; } }
		public int PointValue { get; private set; }

		public Enemy(Texture2D image, Vector2 position)
		{
			this.image = image;
			Position = position;
			Radius = image.Width / 2f;
			color = Color.Transparent;
			PointValue = 1;
		}

		public static Enemy CreateSeeker(Vector2 position)
		{
			var enemy = new Enemy(Art.Seeker, position);
			enemy.AddBehaviour(enemy.FollowPlayer(0.9f));
			enemy.PointValue = 2;

			return enemy;
		}

		public static Enemy CreateWanderer(Vector2 position)
		{
			var enemy = new Enemy(Art.Wanderer, position);
			enemy.AddBehaviour(enemy.MoveRandomly());

			return enemy;
		}

		public override void Update()
		{
			if (timeUntilStart <= 0)
				ApplyBehaviours();
			else
			{
				timeUntilStart--;
				color = Color.White * (1 - timeUntilStart / 60f);
			}

			Position += Velocity;
			Position = Vector2.Clamp(Position, Size / 2, NeonShooterGame.ScreenSize - Size / 2);

			Velocity *= 0.8f;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (timeUntilStart > 0)
			{
				// Draw an expanding, fading-out version of the sprite as part of the spawn-in effect.
				float factor = timeUntilStart / 60f;	// decreases from 1 to 0 as the enemy spawns in
				spriteBatch.Draw(image, Position, null, Color.White * factor, Orientation, Size / 2f, 2 - factor, 0, 0);
			}

			base.Draw(spriteBatch);
		}

		private void AddBehaviour(IEnumerable<int> behaviour)
		{
			behaviours.Add(behaviour.GetEnumerator());
		}

		private void ApplyBehaviours()
		{
			for (int i = 0; i < behaviours.Count; i++)
			{
				if (!behaviours[i].MoveNext())
					behaviours.RemoveAt(i--);
			}
		}

		public void HandleCollision(Enemy other)
		{
			var d = Position - other.Position;
			Velocity += 10 * d / (d.LengthSquared() + 1);
		}

		public void WasShot()
		{
			IsExpired = true;
			PlayerStatus.AddPoints(PointValue);
			PlayerStatus.IncreaseMultiplier();

			float hue1 = rand.NextFloat(0, 6);
			float hue2 = (hue1 + rand.NextFloat(0, 2)) % 6f;
			Color color1 = ColorUtil.HSVToColor(hue1, 0.5f, 1);
			Color color2 = ColorUtil.HSVToColor(hue2, 0.5f, 1);

			for (int i = 0; i < 120; i++)
			{
				float speed = 18f * (1f - 1 / rand.NextFloat(1, 10));
				var state = new ParticleState() 
				{ 
					Velocity = rand.NextVector2(speed, speed), 
					Type = ParticleType.Enemy, 
					LengthMultiplier = 1 
				};

				Color color = Color.Lerp(color1, color2, rand.NextFloat(0, 1));
				NeonShooterGame.ParticleManager.CreateParticle(Art.LineParticle, Position, color, 190, 1.5f, state);
			}

			Sound.Explosion.Play(0.5f, rand.NextFloat(-0.2f, 0.2f), 0);
		}

		#region Behaviours
		IEnumerable<int> FollowPlayer(float acceleration)
		{
			while (true)
			{
				if (!PlayerShip.Instance.IsDead)
					Velocity += (PlayerShip.Instance.Position - Position).ScaleTo(acceleration);

				if (Velocity != Vector2.Zero)
					Orientation = Velocity.ToAngle();

				yield return 0;
			}
		}

		IEnumerable<int> MoveRandomly()
		{
			float direction = rand.NextFloat(0, MathHelper.TwoPi);

			while (true)
			{
				direction += rand.NextFloat(-0.1f, 0.1f);
				direction = MathHelper.WrapAngle(direction);

				for (int i = 0; i < 6; i++)
				{
					Velocity += MathUtil.FromPolar(direction, 0.4f);
					Orientation -= 0.05f;

					var bounds = NeonShooterGame.Viewport.Bounds;
					bounds.Inflate(-image.Width / 2 - 1, -image.Height / 2 - 1);

					// if the enemy is outside the bounds, make it move away from the edge
					if (!bounds.Contains(Position.ToPoint()))
						direction = (NeonShooterGame.ScreenSize / 2 - Position).ToAngle() + rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

					yield return 0;
				}
			}
		}
		#endregion
	}
}