#region Using directives

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace StarTrooper2D
{
    public class Sprite
    {
        public Sprite()
        {
            m_Id = m_Counter++;
        }

        public Sprite(Texture2D Texture)
            : base()
        {
            AddAnimation(new Animation(Texture));
        }

        public Sprite(Texture2D Texture,int Frames, bool Loop)
            : base()
        {
            Animation animation = new Animation(Texture,Frames);
            animation.Loop = Loop;
            animation.Play();

            AddAnimation(animation);
        }

        protected Sprite(Sprite sprite):this()
        {

            foreach (Animation animation in sprite.m_Animations)
                m_Animations.Add((Animation)animation.Clone());

            m_CurrentAnimationIndex = sprite.m_CurrentAnimationIndex;
            m_Name = sprite.m_Name;

            m_CollisionRectangle = sprite.m_CollisionRectangle;
           
            m_Velocity = sprite.m_Velocity;
            m_Position = sprite.m_Position;
            m_Scale = sprite.m_Scale;
            m_Rotation = sprite.m_Rotation;
            m_Visible = sprite.m_Visible;
            m_Active = sprite.m_Active;
            m_ShowAllPixels = sprite.m_ShowAllPixels;
            m_Opacity = sprite.m_Opacity;
            m_ZOrder = sprite.m_ZOrder;
            m_LocalMatrix = sprite.m_LocalMatrix;
            m_SpriteEffect = sprite.m_SpriteEffect;
            m_Origin = sprite.m_Origin;
        }

        public void AddAnimation(Animation animation)
        {
            m_Animations.Add(animation);
            m_Origin = new Vector2((float)Animation.FrameWidth / 2, (float)Animation.FrameHeight / 2);

        }
        
  
        public void InternalUpdate()
        {
            if(!m_Active)
                return;

            if (m_Animations.Count != 0)
                m_Animations[m_CurrentAnimationIndex].Update();
            if (m_Velocity != Vector2.Zero)
            {
                m_Position.X += m_Velocity.X;
                m_Position.Y += m_Velocity.Y;
            }
            UpdateCollisionRectangle();

         //   Update();
        }
        private void UpdateCollisionRectangle()
        {
            m_CollisionRectangle.X = 0;
            m_CollisionRectangle.Y = 0;
            m_CollisionRectangle.Width = Animation.FrameWidth;
            m_CollisionRectangle.Height = Animation.FrameHeight; 
            m_CollisionRectangle.Offset((int)(-m_CollisionRectangle.Width / 2 + m_Position.X), (int)(-m_CollisionRectangle.Height / 2 + m_Position.Y));
        }

        public virtual void Update()
        {

        }

        public void Draw(GameTime gametime, SpriteBatch spritebatch)
        {
            if (!m_Visible || !m_Active || m_Opacity < 0)
                return;
            
            spritebatch.Begin(SpriteSortMode.Texture,BlendState.AlphaBlend);

            m_Animations[m_CurrentAnimationIndex].Draw(gametime, spritebatch, this);
            spritebatch.End();


        }

        public bool CollidesWith(Sprite sprite)
        {
            return m_CollisionRectangle.Intersects(sprite.m_CollisionRectangle);
        }

        public static int ComparisonZOrder(Sprite sprite1, Sprite sprite2)
        {
            int r = sprite2.m_ZOrder - sprite1.m_ZOrder;
            if (r == 0)
                return sprite2.m_Id - sprite1.m_Id;
            else
                return r;
        }

        public virtual Object Clone()
        {
            return new Sprite(this);
        }


        #region Properties

        public int Count
        {
            get
            {
                return m_Animations.Count;
            }
        }

        public int AnimationIndex
        {
            set
            {
                if (value >= m_Animations.Count)
                    value = m_Animations.Count - 1;
                else if (AnimationIndex < 0)
                    value = 0;

                m_CurrentAnimationIndex = value;
            }
            get
            {
                return m_CurrentAnimationIndex;
            }
        }

        public Animation Animation
        {
            get
            {
                return m_Animations[m_CurrentAnimationIndex];
            }
        }

        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                m_Visible = value;
            }
        }

        public bool Active
        {
            set
            {
                m_Active = value;
            }
            get
            {
                return m_Active;
            }
        }

        public Vector2 Position
        {
            set
            {
                m_Position = value;

                //UpdateCollisionRectangle();
            }
            get
            {
                return m_Position;// += m_Animations[m_CurrentAnimationIndex].Origin;
            }
        }

        public Vector2 Location
        {
            set
            {
                m_Position.X = value.X;
                m_Position.Y = value.Y;

                //  UpdateCollisionRectangle();
            }
            get
            {
                return m_Position;
            }
        }

        public Vector2 Velocity
        {
            set
            {
                m_Velocity = value;
            }
            get
            {
                return m_Velocity;
            }
        }

        public Vector2 Scale
        {
            get
            {
                return m_Scale;
            }
        }

        public float ScaleX
        {
            set
            {
                m_Scale.X = value;
            }
            get
            {
                return m_Scale.X;
            }
        }

        public float ScaleY
        {
            set
            {
                m_Scale.Y = value;
            }
            get
            {
                return m_Scale.Y;
            }
        }

        public float Rotation
        {
            set
            {
                m_Rotation = value;
            }
            get
            {
                return m_Rotation;
            }
        }

        public int ZOrder
        {
            set
            {
                m_ZOrder = value;
            }
            get
            {
                return m_ZOrder;
            }
        }

        public int Opacity
        {
            set
            {
                m_Opacity = value;
            }
            get
            {
                return m_Opacity;
            }
        }

        public bool ShowAllPixels
        {
            set
            {
                m_ShowAllPixels = value;
            }
            get
            {
                return m_ShowAllPixels;
            }
        }

        public SpriteEffects SpriteEffect
        {
            set
            {
                m_SpriteEffect = value;
            }
            get
            {
                return m_SpriteEffect;
            }
        }

        public int Speed
        {
            set
            {
                m_Speed = value;
            }
            get
            {
                return m_Speed;
            }
        }

        public Vector2 Origin
        {
            set
            {
                m_Origin = value;
            }
            get
            {
                return m_Origin;
            }
        }

        #endregion
        
        #region PrivateData

        List<Animation> m_Animations = new List<Animation>();
        int m_CurrentAnimationIndex;
        string m_Name = "";
        Rectangle m_CollisionRectangle; 
        Vector2 m_Velocity = Vector2.Zero;
        Vector2 m_Position;
        Vector2 m_Scale = new Vector2(1, 1);
        float m_Rotation = 0;
        bool m_Visible = true;
        bool m_Active = true;
        bool m_ShowAllPixels = false;
        int m_Opacity = 100;
        int m_ZOrder = 0;
        Matrix m_LocalMatrix = Matrix.Identity;
        int m_Id;
        static int m_Counter;
        SpriteEffects m_SpriteEffect = SpriteEffects.None;
        int m_Speed = 2;
        Vector2 m_Origin = Vector2.Zero;

        #endregion PrivateData
    }
}
