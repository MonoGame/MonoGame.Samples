using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace TouchExample;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // this is the list of supported gestures.
        TouchPanel.EnabledGestures = GestureType.DoubleTap |
                                     GestureType.DragComplete |
                                     GestureType.Flick |
                                     GestureType.FreeDrag |
                                     GestureType.Hold |
                                     GestureType.HorizontalDrag |
                                     GestureType.Pinch |
                                     GestureType.PinchComplete |
                                     GestureType.Tap |
                                     GestureType.VerticalDrag;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Arial");
    }

    protected override void Update(GameTime gameTime)
    {
        while (TouchPanel.IsGestureAvailable)
        {
            GestureSample gesture = TouchPanel.ReadGesture();

            switch (gesture.GestureType)
            {
                case GestureType.DoubleTap:
                    _gestures.Add(new GestureText{ Text="Double Tap", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.Flick:
                    _gestures.Add(new GestureText{ Text="Flick", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.FreeDrag:
                    _gestures.Add(new GestureText{ Text="Free Drag", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.Hold:
                    _gestures.Add(new GestureText{ Text="Hold", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.HorizontalDrag:
                    _gestures.Add(new GestureText{ Text="Horizontal Drag", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.Pinch:
                    _gestures.Add(new GestureText{ Text="Pinch", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.Tap:
                    _gestures.Add(new GestureText{ Text="Tap", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
                
                case GestureType.VerticalDrag:
                    _gestures.Add(new GestureText{ Text="Vertical Drag", X=gesture.Position.X, Y=gesture.Position.Y });
                    break;
            }
        }

        List<GestureText> itemsToRemove = new();
        
        foreach (GestureText gesture in _gestures)
        {
            gesture.Lifetime -= gameTime.ElapsedGameTime;
            gesture.Y -= (float)(gameTime.ElapsedGameTime.TotalMilliseconds * 0.5f);
            
            if (gesture.Lifetime < TimeSpan.Zero)
            {
                itemsToRemove.Add(gesture);
            }
        }

        foreach (GestureText item in itemsToRemove)
        {
            _gestures.Remove(item);
        }
        
        base.Update(gameTime);
    }

    readonly List<GestureText> _gestures = new();

    class GestureText
    {
        public string Text { get; set; }
        
        public float X { get; set; }
        
        public float Y { get; set; }
        
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromSeconds(1);

        public float Alpha => (float)(Lifetime.TotalSeconds / 1.0);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (GestureText text in _gestures)
        {
            _spriteBatch.Begin();
            Color textColor = Color.White * text.Alpha;
            _spriteBatch.DrawString(_font, text.Text, new Vector2(text.X, text.Y), textColor);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}
