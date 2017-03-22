#region Using directives

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace StarTrooper2D
{
    public class Text2D
    {
        public Text2D(SpriteFont font)
        {
            m_Font = font;
        }

        public Vector2 Position
        {
            set
            {
                m_Position = value;
            }
            get
            {
                return m_Position;
            }
        }

        public Color Color
        {
            set
            {
                m_Color = value;
            }
            get
            {
                return m_Color;
            }
        }

        public string Text
        {
            set
            {
                m_Text = value;
            }
            get
            {
                return m_Text;
            }
        }

        public bool Visible
        {
            set
            {
                m_Visible = value;
            }
            get
            {
                return m_Visible;
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

        public void Draw(GameTime gametime, SpriteBatch spritebatch)
        {
            if (m_Visible)
                spritebatch.Begin();
                spritebatch.DrawString(m_Font, m_Text, new Vector2(m_Position.X, m_Position.Y), m_Color);
                spritebatch.End();
        }

        public void InternalUpdate()
        {
            if (!m_Active)
                return;
             Update();
        }

        public virtual void Update()
        {

        }

        #region PrivateData

        string m_Text = "";
        Color m_Color = Color.Black;
        Vector2 m_Position;

        bool m_Visible = true;
        bool m_Active = true;

        SpriteFont m_Font;

        #endregion PrivateData
    }
}
