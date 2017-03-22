using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace StarTrooper2D
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ParticleManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SwampLib.Pool<ParticleEmitter> m_emitters;
        Dictionary<String, Texture2D> m_textures;

        SpriteBatch m_spritebatch;

        public ParticleManager(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.m_spritebatch = spriteBatch;
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            m_emitters = new SwampLib.Pool<ParticleEmitter>(1000);
            m_textures = new Dictionary<string, Texture2D>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
#if TESTPERFORMANCE
            // TODO: Add your update code here
            StarTrooperGame.PerformanceTimer.AddUpdateDisplayInfo(-1,"Total Number of Emitters: ",m_emitters.ActiveCount);
#endif
            // calculate dt, the change in the since the last frame. the particle
            // updates will use this value.
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Go through all of the active particles for each emitter.
            // Note that because we will return any particle that has finished,
            // we need to use the iterator that returns nodes instead of particles.
            foreach (SwampLib.Pool<ParticleEmitter>.Node penode in m_emitters.ActiveNodes)
            {

                if (penode.Item.Active)
                {
#if TESTPERFORMANCE
                    //Monitoring
                    StarTrooperGame.PerformanceTimer.AddUpdateDisplayInfo(penode.NodeIndex,"Particle Emitter:" + penode.Item.GetType().ToString() + " - Particle Count: ",penode.Item.particles.ActiveCount);
#endif
                    penode.Item.Update(dt);

                    if (!penode.Item.Active)
                    {
                        m_emitters.Return(penode);
#if TESTPERFORMANCE 
                        StarTrooperGame.PerformanceTimer.RemoveDisplayInfo(penode.NodeIndex);
#endif
                    }
                }
                
            }
            base.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
#if TESTPERFORMANCE
            StarTrooperGame.PerformanceTimer.StartTimer("Time to draw full particle manager in ticks: ");
#endif
            foreach (ParticleEmitter pe in m_emitters)
            {
#if TESTPERFORMANCE
                StarTrooperGame.PerformanceTimer.StartTimer("Time to draw emitter particles in ticks: ");
#endif
                m_spritebatch.Begin(SpriteSortMode.Texture, pe.SpriteBlendMode);
                foreach (Particle p in pe.particles)
                {
                    // normalized lifetime is a value from 0 to 1 and represents how far
                    // a particle is through its life. 0 means it just started, .5 is half
                    // way through, and 1.0 means it's just about to be finished.
                    // this value will be used to calculate alpha and scale, to avoid 
                    // having particles suddenly appear or disappear.
                    float normalizedLifetime = p.TimeSinceStart / p.Lifetime;

                    // we want particles to fade in and fade out, so we'll calculate alpha
                    // to be (normalizedLifetime) * (1-normalizedLifetime). this way, when
                    // normalizedLifetime is 0 or 1, alpha is 0. the maximum value is at
                    // normalizedLifetime = .5, and is
                    // (normalizedLifetime) * (1-normalizedLifetime)
                    // (.5)                 * (1-.5)
                    // .25
                    // since we want the maximum alpha to be 1, not .25, we'll scale the 
                    // entire equation by 4.
                    float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
                    Color color = new Color(new Vector4(1, 1, 1, alpha));

                    // make particles grow as they age. they'll start at 75% of their size,
                    // and increase to 100% once they're finished.
                    float scale = p.Scale * (.75f + .25f * normalizedLifetime);

                    m_spritebatch.Draw(m_textures[pe.Texture], p.Position, null, color,
                        p.Rotation, pe.Origin, scale, SpriteEffects.None, 0.0f);

                }
                m_spritebatch.End();
#if TESTPERFORMANCE
                    StarTrooperGame.PerformanceTimer.StopTimer("Time to draw emitter particles in ticks: ");
#endif
            }

#if TESTPERFORMANCE
            StarTrooperGame.PerformanceTimer.StopTimer("Time to draw full particle manager in ticks: ");
#endif
            base.Draw(gameTime);
        }

        public void Add(ParticleEmitter emitter)
        {
            m_emitters.Add(emitter);
        }

        public Texture2D LoadTexture(String texture)
        {
            Texture2D tex;
            try { tex = m_textures[texture]; }
            catch 
            {
                tex = Game.Content.Load<Texture2D>(@"Particles\" + texture);
                m_textures.Add(texture, tex);
            }

            return tex;
        }
    }
}