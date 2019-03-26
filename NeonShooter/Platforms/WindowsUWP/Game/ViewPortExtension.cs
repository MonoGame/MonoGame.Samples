namespace NeonShooter
{
    public static class ViewPortExtension 
    {
        public static Microsoft.Xna.Framework.Rectangle TitleSafeViewPort(this Microsoft.Xna.Framework.Graphics.Viewport input)
        {
            int w = input.Width;
            int h = input.Height;
            int x = input.X;
            int y = input.Y;

            int safeW = (w + 19) / 20;
            int safeH = (h + 19) / 20;

            x += safeW;
            y += safeH;

            w -= safeW * 2;
            h -= safeH * 2;
            return new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
        }
    }
}
