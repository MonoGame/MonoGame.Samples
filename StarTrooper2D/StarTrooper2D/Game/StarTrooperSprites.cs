#region Using directives

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;

#endregion



namespace StarTrooper2D
{
  public class Trooper: Sprite
  {
    public Trooper(Texture2D Texture, int Frames, bool Loop)
          : base(Texture, Frames, Loop)
    { }
    public void HandleInput(GameTime gameTime, InputState input, PlayerIndex? ControllingPlayer)
    {
        Vector2 movement = Vector2.Zero;
            PlayerIndex player;

            if (input.IsKeyPressed(Keys.Left, ControllingPlayer, out player))
                movement.X--;

            if (input.IsKeyPressed(Keys.Right, ControllingPlayer, out player))
                movement.X++;

            if (input.IsKeyPressed(Keys.Up, ControllingPlayer, out player))
                movement.Y--;

            if (input.IsKeyPressed(Keys.Down, ControllingPlayer, out player))
                movement.Y++;

            if (input.IsNewKeyPress(Keys.Space, ControllingPlayer, out player))
                TrooperFire();

            Vector2 thumbstick = input.CurrentGamePadStates[(int)ControllingPlayer.Value].ThumbSticks.Left;

            movement.X += thumbstick.X;
            movement.Y -= thumbstick.Y;

            if (input.TouchState.Count > 0)
            {
                foreach (TouchLocation t in input.TouchState)
                {
                    switch (t.State)
                    {
                        case TouchLocationState.Pressed:
                            TrooperFire();
                            break;
                        case TouchLocationState.Moved:
                            //movement = new Vector2(t.Position.X - Position.X, t.Position.Y - Position.Y);
                            Vector2 touchPosition = input.TouchState[0].Position;
                            Vector2 direction = touchPosition - Position;
                            direction.Normalize();
                            movement += direction;
                            break;
                        case TouchLocationState.Released:
                            break;

                        default:
                            break;
                    }
                }
            }

        if (Position.Y > 50 && Input.MoveUp())
            movement.Y += -2; // if trooper is under y=50 then go upward
        if (Position.Y < GameConstants.BackBufferHeight - 30 && Input.MoveDown())
            movement.Y += 2; // if trooper is over y=450 then go upward
        if (Position.X > 30 && Input.MoveLeft())
        {
            movement.X += -2; // go to the left
        }
        if (Position.X < GameConstants.BackBufferWidth - 70 && Input.MoveRight())
        {
            movement.X = 2; // go to the right
        }
        if (movement != Vector2.Zero) movement.Normalize();

        if (movement.X > 0)
        {
            SpriteEffect = SpriteEffects.None; // right flip trooper
        }
        else if (movement.X < 0)
        {
            SpriteEffect = SpriteEffects.FlipHorizontally; // left flip trooper
        }

        Velocity = movement * Speed; // set new velocity for Trooper
    }

    public override void Update() { }

    void TrooperFire()
    {
        // dynamically create a new sprite
        Fire fire = (Fire)GameConstants.Fire.Clone();
        fire.Position = new Vector2(Position.X, Position.Y - 35);
        fire.Velocity = new Vector2(0, -4);
        GameFunctions.Add(fire); // set the fire sprite active
        FireballLaunch(new Vector2(Position.X, Position.Y - 35), new Vector2(0, -40), new Vector2(0, -0.5f));
        AudioEngine.PlayShoot();
        GameState.shots++;
    }

    void FireballLaunch(Vector2 position, Vector2 velocity, Vector2 accel)
    {

        FireballSmokeParticleEmitter smokeemitter = new FireballSmokeParticleEmitter();
        smokeemitter.Initialize("smoke", 10);
        smokeemitter.EmitterPosition = position;
        smokeemitter.EmitterVelocity = velocity;
        smokeemitter.EmitterAcceleration = accel;
        smokeemitter.ParticleCycleTime = 0f;
        GameConstants.ParticleManager.Add(smokeemitter);

        FireballParticleEmitter fireballemitter = new FireballParticleEmitter();
        fireballemitter.Initialize("explosion", 10);
        fireballemitter.EmitterPosition = position;
        fireballemitter.EmitterVelocity = velocity;
        fireballemitter.EmitterAcceleration = accel;
        fireballemitter.ParticleCycleTime = 0f;
        GameConstants.ParticleManager.Add(fireballemitter);

    }
  }

  public class Condor : Sprite
  {
    public Condor()
    {
    }

    protected Condor(Condor condor): base(condor)
    {
    }

    public override Object Clone()
    {
      return new Condor(this);
    }

    public override void Update()
    {
        if (m_CollisionWithTrooper)
        {
            CollisionWithTrooper();
            return;
        }

        Trooper b = GameState.Trooper;
        if (AnimationIndex != 1)
        {


            Vector2 v = new Vector2(b.Position.X - Position.X, b.Position.Y - Position.Y);
            v.Normalize();

            Velocity = v;

            if (v.X >= 0)
                SpriteEffect = SpriteEffects.None;
            else
                SpriteEffect = SpriteEffects.FlipHorizontally;

            List<Sprite> collidedSprites = GameFunctions.GetCollidedSprites(this);
            if (collidedSprites != null)
            {
                foreach (Sprite s in collidedSprites)
                {
                    if (s is Fire)
                    {
                        AudioEngine.PlayDie();
                        AnimationIndex = 1;
                        GameState.score++;
                        GameConstants.ScoreText.Text = "Score: " + GameState.score.ToString();
                        GameFunctions.Remove(s);
                        break;
                    }
                    else if (s is Trooper)
                    {
                        m_CollisionWithTrooper = true;
                        AudioEngine.PlayDie();
                        Animation.Stop();
                        GameState.score--;
                        GameConstants.ScoreText.Text = "Score: " + GameState.score.ToString();
                        break;
                    }
                }
            }
        }
        else
        {
            if (this.Animation.isPlayingLastFrame)
                GameFunctions.Remove(this);
        }

    }
    private void CollisionWithTrooper()
    {
        Opacity -= 1;
        if (ScaleX < 0)
        {
            ScaleX += 0.01f;
            Rotation -= 0.03f;
        }
        else
        {
            ScaleX -= 0.01f;
            Rotation += 0.03f;
        }
        ScaleY -= 0.01f;
        if (Opacity <= 0)
            GameFunctions.Remove(this);
    }

    bool m_CollisionWithTrooper = false;

  }
  public class Fire : Sprite
  {
      public Fire()
      {
      }

      protected Fire(Fire Fire)
          : base(Fire)
      {
      }
      public Fire(Texture2D Texture, int Frames, bool Loop)
          : base(Texture, Frames, Loop)
      { }

      public override Object Clone()
      {
          return new Fire(this);
      }

      public override void Update()
      {
          float y = Position.Y;

          //Type the code here remove the Fire sprite
          #region FireSprite_code
          if (y < -100)
              GameFunctions.Remove(this);
          #endregion
          ScaleX = ScaleY = 1 + Math.Abs(0.001f * (480 - Position.Y));
      }
  }
    
}
