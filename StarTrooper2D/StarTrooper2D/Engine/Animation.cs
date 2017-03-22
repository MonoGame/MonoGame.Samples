#region Using directives

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace StarTrooper2D
{

    public struct Frame
    {
        public Frame(Rectangle frame, int delay)
        {
            m_frame = frame;
            m_Delay = delay;
        }
        public Rectangle BoundingFrame
        {
            get
            {
                return m_frame;
            }
        }
        public int Delay
        {
            get
            {
                return m_Delay;
            }
        }

        #region privatedate

        Rectangle m_frame;
        int m_Delay;

        #endregion
    }

    public class Animation
    {

        public Animation(Texture2D SpritesheetTexture)
        {
            int[] Delay = { 1 };
            m_SpritesheetTexture = SpritesheetTexture;
            GenerateFrames(this, 1,Delay);                
        }

        public Animation(Texture2D SpritesheetTexture, int FrameCount)
            : this(SpritesheetTexture)
        {
            int[] Delay = new int[FrameCount];
            for (int i=0;i<FrameCount;i++)
                Delay[i] = 5;
            GenerateFrames(this, FrameCount, Delay);
        }

        public Animation(Texture2D SpritesheetTexture, int FrameCount, int[] Delay)
            : this(SpritesheetTexture)
        {
            GenerateFrames(this, FrameCount, Delay);
        }

        protected Animation(Animation animation)
        {
            m_CurrentFrame = animation.m_CurrentFrame;
            m_CurrentDelay = animation.m_CurrentDelay;
            m_StartingFrame = animation.m_StartingFrame;
            m_EndingFrame = animation.m_EndingFrame;
            m_Loop = animation.m_Loop;
            m_isPlaying = animation.m_isPlaying;
            m_isPaused = animation.m_isPaused;
            m_Frames = animation.m_Frames;

            m_SpritesheetTexture = animation.m_SpritesheetTexture;
            m_Width = animation.m_Width;
            m_Height = animation.m_Height;
            m_Colour = animation.m_Colour;

        }

        private void GenerateFrames(Animation animation, int FrameCount, int[] Delay)
        {

            m_Width = m_SpritesheetTexture.Width / FrameCount;
            m_Height = m_SpritesheetTexture.Height;

            animation.m_Frames = new Frame[FrameCount];

            for (int i = m_StartingFrame; i < FrameCount; i++)
            {
                animation.m_Frames[i] = new Frame(new Rectangle(i * m_Width,
                    0, m_Width, m_Height), Delay[i]);
            }
            m_EndingFrame = m_Frames.Length - 1;
        }

        public void Play()
        {
            if (!m_isPlaying && !m_isPaused)
            {
                m_CurrentFrame = m_StartingFrame;
                m_CurrentDelay = m_Frames[m_StartingFrame].Delay;
            }
            m_isPlaying = true;
        }

        public void Stop()
        {
            m_isPlaying = false;
            m_isPaused = false;
        }

        public void Pause()
        {
            m_isPaused = true;
        }

        public void Update()
        {
            if (m_Frames.Length == 0 || !m_isPlaying || m_isPaused)
                return;

            m_CurrentDelay--;
            if (m_CurrentDelay < 0)
            {
                m_CurrentFrame++;
                if (m_CurrentFrame > m_EndingFrame)
                {
                    if (m_Loop)
                        m_CurrentFrame = m_StartingFrame;
                    else
                        m_CurrentFrame = m_EndingFrame;
                }
                m_CurrentDelay = m_Frames[m_CurrentFrame].Delay;
            }
         }

        public void Draw(GameTime gametime, SpriteBatch spritebatch,Sprite sprite)
        {
            m_Colour.A = (byte)((sprite.Opacity * 255) / 100);
            spritebatch.Draw(m_SpritesheetTexture, sprite.Position, CurrentFrame, m_Colour, sprite.Rotation, sprite.Origin,
sprite.Scale, sprite.SpriteEffect, 0);

        }


        public virtual Object Clone()
        {
            return new Animation(this);
        }

        #region Properties

        public bool isPlaying
        {
            get
            {
                return m_isPlaying;
            }
        }

        public bool isPaused
        {
            get
            {
                return m_isPaused;
            }
        }

        public bool Loop
        {
            set
            {
                m_Loop = value;
            }
            get
            {
                return m_Loop;
            }
        }

        public int FirstFrame
        {
            set
            {
                if (value > m_EndingFrame)
                    value = m_EndingFrame;
                else if (value < 0)
                    value = 0;
                m_StartingFrame = value;
            }
            get
            {
                return m_StartingFrame;
            }
        }

        public int LastFrame
        {
            set
            {
                if (value < m_StartingFrame)
                    value = m_StartingFrame;
                else if (value >= m_Frames.Length)
                    value = m_Frames.Length - 1;

                m_EndingFrame = value;
            }
            get
            {
                return m_EndingFrame;
            }
        }

        public int RemainingFrameDelay
        {
            get
            {
                return m_Frames[m_CurrentFrame].Delay - m_CurrentDelay;
            }
        }

        public int CurrentFrameIndex
        {
            set
            {
                if (value < m_StartingFrame)
                    value = m_StartingFrame;
                else if (value > m_EndingFrame)
                    value = m_EndingFrame;
                m_CurrentFrame = value;
            }
            get
            {
                return m_CurrentFrame;
            }
        }

        public Rectangle CurrentFrame
        {
            get
            {
                return m_Frames[m_CurrentFrame].BoundingFrame;
            }
        }

        public int Count
        {
            get
            {
                return m_Frames.Length;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return m_SpritesheetTexture;
            }
        }

        public bool isPlayingFirstFrame
        {
            get
            {
                return m_Frames[m_StartingFrame].Delay == m_CurrentDelay && m_StartingFrame == m_CurrentFrame;
            }
        }

        public bool isPlayingLastFrame
        {
            get
            {
                return m_CurrentDelay == 0 && m_CurrentFrame == m_EndingFrame;
            }
        }

        public int FrameWidth
        {
            get
            {
                return m_Width;
            }
        }

        public int FrameHeight
        {
            get
            {
                return m_Height;
            }
        }

        #endregion


        #region privatedata

        Texture2D m_SpritesheetTexture;
        int m_Width;
        int m_Height;
        Color m_Colour = new Color(255,255,255);

        

        Frame[] m_Frames;
        int m_CurrentFrame;
        int m_CurrentDelay;
        int m_StartingFrame;
        int m_EndingFrame;
        bool m_Loop;
        bool m_isPlaying;
        bool m_isPaused;

        #endregion
    }
}
