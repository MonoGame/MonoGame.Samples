#region Using directives

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace StarTrooper2D
{
    public class Background: Sprite
    {
        public Background()
        {

        }

        public Background(Texture2D Texture)
            : base(Texture)
        { }

        protected Background(Background background): base(background)
        {

        }

        public override Object Clone()
        {
            return new Background(this);
        }

        public override void Update()
        {
            Vector2 NewLocation = Location;
            if (NewLocation.Y == GameConstants.BackBufferHeight)
            {
                NewLocation.Y = -GameConstants.BackBufferHeight;
                Location = NewLocation;
            }
        }
    }
}
