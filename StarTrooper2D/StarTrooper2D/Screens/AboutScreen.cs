using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;

namespace StarTrooper2D
{
    class AboutScreen : MenuScreen
    {
        #region fields
        ContentManager content;
        Texture2D Avatarimage;
        Texture2D ZenithMoonLogoImage;
        SpriteFont AboutTextFontHeader;
        SpriteFont AboutTextFont;

        Rectangle LogoLocation = new Rectangle(20, 120, 150, 150);
        Rectangle AvatarLocation = new Rectangle(310, 120, 150, 150);
        #endregion

        #region Initialisation
        public AboutScreen()
            : base("AboutScreen")
        {
            MenuOffset = new Vector2(0, 400);
            MenuEntry VisitDarkGenesis = new MenuEntry("Visit Dark Genesis Blog");
            MenuEntry VisitCodeplex = new MenuEntry("Goto Codeplex Source site");
            MenuEntry VisitDDReaper = new MenuEntry("View DDReaper on Twitter");
            MenuEntry VisitZenithMoon = new MenuEntry("Visit ZenithMoon Studios");

            VisitDarkGenesis.Selected += VisitDarkGenesis_Selected;
            VisitCodeplex.Selected += VisitCodeplex_Selected;
            VisitDDReaper.Selected += VisitDDReaper_Selected;
            VisitZenithMoon.Selected += VisitZenithMoon_Selected;

            MenuEntries.Add(VisitDarkGenesis);
            MenuEntries.Add(VisitCodeplex);
            MenuEntries.Add(VisitDDReaper);
            MenuEntries.Add(VisitZenithMoon);
        }

        #endregion

        #region Content Load/Unload

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                Avatarimage = content.Load<Texture2D>(@"MenuAssets\Avatar");
                ZenithMoonLogoImage = content.Load<Texture2D>(@"MenuAssets\ZenithMoonLogo");
                AboutTextFontHeader = content.Load<SpriteFont>(@"MenuAssets\AboutFont");
                AboutTextFont = content.Load<SpriteFont>(@"MenuAssets\menufontsm");

                base.Activate(instancePreserved);
            }
        }

        public override void Unload()
        {
            content.Unload();
            content.Dispose();
            base.Unload();
        }
        #endregion

        #region Handle Input
        void VisitZenithMoon_Selected(object sender, PlayerIndexEventArgs e)
        {
            NavigatetoWebPage(@"http://zenithmoon.com/");
        }

        void VisitDDReaper_Selected(object sender, PlayerIndexEventArgs e)
        {
            NavigatetoWebPage(@"http://twitter.com/#!/DDReaper");
        }

        void VisitCodeplex_Selected(object sender, PlayerIndexEventArgs e)
        {
            NavigatetoWebPage(@"http://starter3dxna.codeplex.com/");
        }

        void VisitDarkGenesis_Selected(object sender, PlayerIndexEventArgs e)
        {
            NavigatetoWebPage(@"http://xna-uk.net/blogs/darkgenesis/archive/2011/04/05/recap-video-for-the-xna-2d-at-amp-t-webcast.aspx");
        }

        private static void NavigatetoWebPage(string website)
        {
            //try
            //{
            //    WebBrowserTask wb = new WebBrowserTask();
            //    wb.Uri = new Uri(website, UriKind.Absolute);
            //    wb.Show();
            //}
            //catch { }

        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            foreach (TouchLocation touch in input.TouchState)
            {
                if (touch.State == TouchLocationState.Moved)
                {
                    // convert the position to a Point that we can test against a Rectangle
                    if (AvatarLocation.Contains((int)touch.Position.X,(int)touch.Position.Y))
                    {
                        NavigatetoWebPage(@"http://twitter.com/#!/DDReaper");
                    }
                    if (LogoLocation.Contains((int)touch.Position.X, (int)touch.Position.Y))
                    {
                        NavigatetoWebPage(@"http://zenithmoon.com/");
                    }
                }

            }

            base.HandleInput(gameTime,input);

        }

        #endregion

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(ZenithMoonLogoImage, LogoLocation, Color.White);
            spriteBatch.Draw(Avatarimage, AvatarLocation, Color.White);

            spriteBatch.DrawString(AboutTextFontHeader, "XNA Starter TwoD", 
                new Vector2(110,320), 
                Color.White);
            spriteBatch.DrawString(AboutTextFontHeader, "tutorial project",
                new Vector2(130, 350),
                Color.White);
            spriteBatch.DrawString(AboutTextFont, "Sample project for the XNA 2D Tutorial",
                new Vector2(10, 420),
                Color.White);
            spriteBatch.DrawString(AboutTextFont, "Read more on how to get started in XNA",
                new Vector2(10, 460),
                Color.White);
            spriteBatch.DrawString(AboutTextFont, "on the Dark Genesis Blog and XNA-UK.NET",
                new Vector2(10, 480),
                Color.White);


            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);

            base.Draw(gameTime);
        }
    }
}
