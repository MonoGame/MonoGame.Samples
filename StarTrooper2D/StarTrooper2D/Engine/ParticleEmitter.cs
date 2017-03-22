using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarTrooper2D
{
    public class ParticleEmitter
    {
        // Position, Velocity, and Acceleration represent exactly what their names
        // indicate. They are public fields rather than properties so that users
        // can directly access their .X and .Y properties.
        public Vector2 EmitterPosition = Vector2.Zero;
        public Vector2 EmitterVelocity = Vector2.Zero;
        public Vector2 EmitterAcceleration = Vector2.Zero;
        public float ParticleCycleTime = 0;

        /// <summary>
        /// Constructs a new ParticleEmitter.
        /// </summary>
        /// <remarks>As this is intended to be used inside a resource pool,
        /// it needs to have a parameterless constructor, for initialising 
        /// we have a seperate initilise class</remarks>
        public ParticleEmitter()
        {            
        }

        /// <summary>
        /// override the base class's Initialize to do some additional work; we want to
        /// call InitializeConstants to let subclasses set the constants that we'll use.
        /// 
        /// also, the particle array and freeParticles queue are set up here.
        /// </summary>
        public void Initialize(string texture, int howManyEffects)
        {

            Texture2D tex = GameConstants.ParticleManager.LoadTexture(texture);
            m_texture = texture;
            m_howManyEffects = howManyEffects;
            // Calculate the center. this'll be used in the draw call, we
            // always want to rotate and scale around this point.
            m_origin.X = tex.Width / 2;
            m_origin.Y = tex.Height / 2;

            InitializeConstants();

            // Create a pool contiaining the maximum number of particles we will ever need for this effect.
            particles = new SwampLib.Pool<Particle>(m_howManyEffects * maxNumParticles);

            m_active = true;
        }

        /// <summary>
        /// this abstract function must be overriden by subclasses of ParticleEmitter.
        /// It's here that they should set all the constants marked in the region
        /// "constants to be set by subclasses", which give each ParticleEmitter its
        /// specific flavor.
        /// </summary>
        protected virtual void InitializeConstants() { }

        /// <summary>
        /// AddParticles's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles will have no effect.
        /// </summary>
        /// <param name="where">where the particle effect should be created</param>
        public void AddParticles(Vector2 where)
        {
            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles =
                GameFunctions.Random.Next(minNumParticles, maxNumParticles);
            AddParticles(where, numParticles);

        }

        /// <summary>
        /// AddParticles's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles will have no effect.
        /// </summary>
        /// <param name="where">where the particle effect should be created</param>
        public void AddParticles(Vector2 where, int numParticles)
        {
            // Create the desired number of particles, up to the number of available
            // particles in the pool.
            numParticles = Math.Min(numParticles, particles.AvailableCount);
            while (numParticles-- > 0)
            {
                SwampLib.Pool<Particle>.Node p = particles.Get();
                InitializeParticle(p.Item, where);
            }
        }

        /// <summary>
        /// InitializeParticle randomizes some properties for a particle, then
        /// calls initialize on it. It can be overriden by subclasses if they 
        /// want to modify the way particles are created. For example, 
        /// SmokePlumeParticleSystem overrides this function make all particles
        /// accelerate to the right, simulating wind.
        /// </summary>
        /// <param name="p">the particle to initialize</param>
        /// <param name="where">the position on the screen that the particle should be
        /// </param>
        protected virtual void InitializeParticle(Particle p, Vector2 where)
        {
            // first, call PickRandomDirection to figure out which way the particle
            // will be moving. velocity and acceleration's values will come from this.
            Vector2 direction;
             if (EmitterVelocity == Vector2.Zero)
                direction = PickRandomDirection();
            else
                direction = EmitterVelocity += EmitterAcceleration;

            // pick some random values for our particle
            float velocity =
                GameFunctions.RandomBetween(minInitialSpeed, maxInitialSpeed);
            float acceleration =
                GameFunctions.RandomBetween(minAcceleration, maxAcceleration);
            float lifetime =
                GameFunctions.RandomBetween(minLifetime, maxLifetime);
            float scale =
                GameFunctions.RandomBetween(minScale, maxScale);
            float rotationSpeed =
                GameFunctions.RandomBetween(minRotationSpeed, maxRotationSpeed);

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(
                where, velocity * direction, acceleration * direction,
                lifetime, scale, rotationSpeed);
        }

        // update is called by the Emmitter on every frame. This is where the
        // particle's position and that kind of thing get updated.
        public virtual void Update(float dt)
        {
            EmitterVelocity += EmitterAcceleration * dt;
            EmitterPosition += EmitterVelocity * dt;
            

            foreach (SwampLib.Pool<Particle>.Node node in particles.ActiveNodes)
            {
                Particle p = node.Item;

                if (p.Active)
                {
                    // ... and if they're active, update them.
                    p.Update(dt);
                    // if that update finishes them, return them to the pool.
                    if (!p.Active)
                    {
                        particles.Return(node);
                    }
                }

            }

            
        }

        /// <summary>
        /// PickRandomDirection is used by InitializeParticles to decide which direction
        /// particles will move. The default implementation is a random vector in a
        /// circular pattern.
        /// </summary>
        protected virtual Vector2 PickRandomDirection()
        {
            float angle = (float)GameFunctions.Random.NextDouble() * -MathHelper.TwoPi;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        #region properties

        // the texture this particle system will use.
        private String m_texture;
        public String Texture
        {
            get { return m_texture; }
        }

        // the origin when we're drawing textures. this will be the middle of the
        // texture.
        private Vector2 m_origin;
        public Vector2 Origin
        {
            get { return m_origin; }
            set { m_origin = value; }
        }
        // this number represents the maximum number of effects this particle system
        // will be expected to draw at one time. this is set in the constructor and is
        // used to calculate how many particles we will need.
        private int m_howManyEffects;

        // is this emitter still alive? if not then particle should no longer be drawn or updated.
        private bool m_active = false;
        public bool Active
        {
            get { return m_active; }
            set { m_active = value; }
        }

        // The pool of particles used by this system.  
        // The pool automatically manages one-time allocation of particles, and reuses
        // particles when they are returned to the pool.
        public SwampLib.Pool<Particle> particles;

        /// <summary>
        /// returns the number of particles that are available for a new effect.
        /// </summary>
        public int FreeParticleCount
        {
            get
            {
                // Get the number of particles in the pool that are available for use.
                return particles.AvailableCount;
            }
        }

#endregion

        // This region of values control the "look" of the particle system, and should 
        // be set by deriving particle systems in the InitializeConstants method. The
        // values are then used by the virtual function InitializeParticle. Subclasses
        // can override InitializeParticle for further
        // customization.
        #region constants to be set by subclasses

        /// <summary>
        /// minNumParticles and maxNumParticles control the number of particles that are
        /// added when AddParticles is called. The number of particles will be a random
        /// number between minNumParticles and maxNumParticles.
        /// </summary>
        protected int minNumParticles = 0;
        protected int maxNumParticles = 0;

        /// <summary>
        /// minInitialSpeed and maxInitialSpeed are used to control the initial velocity
        /// of the particles. The particle's initial speed will be a random number 
        /// between these two. The direction is determined by the function 
        /// PickRandomDirection, which can be overriden.
        /// </summary>
        protected float minInitialSpeed = 0;
        protected float maxInitialSpeed = 0;

        /// <summary>
        /// minAcceleration and maxAcceleration are used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// these two. By default, the direction of acceleration is the same as the
        /// direction of the initial velocity.
        /// </summary>
        protected float minAcceleration = 0;
        protected float maxAcceleration = 0;

        /// <summary>
        /// minRotationSpeed and maxRotationSpeed control the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between minRotationSpeed and maxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        protected float minRotationSpeed = 0;
        protected float maxRotationSpeed = 0;

        /// <summary>
        /// minLifetime and maxLifetime are used to control the lifetime. Each
        /// particle's lifetime will be a random number between these two. Lifetime
        /// is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        protected float minLifetime = 0;
        protected float maxLifetime = 0;

        /// <summary>
        /// to get some additional variance in the appearance of the particles, we give
        /// them all random scales. the scale is a value between minScale and maxScale,
        /// and is additionally affected by the particle's lifetime to avoid particles
        /// "popping" into view.
        /// </summary>
        protected float minScale = 0;
        protected float maxScale = 0;

        /// <summary>
        /// different effects can use different blend modes. fire and explosions work
        /// well with additive blending, for example.
        /// </summary>
        protected BlendState spriteBlendMode = BlendState.Opaque;
        public BlendState SpriteBlendMode
        {
            get { return spriteBlendMode; }
        }
        #endregion
    }


}
