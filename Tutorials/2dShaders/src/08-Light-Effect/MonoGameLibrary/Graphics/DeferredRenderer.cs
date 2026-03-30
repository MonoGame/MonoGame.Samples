using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class DeferredRenderer
{
    /// <summary>
    /// A texture that holds the unlit sprite drawings
    /// </summary>
    public RenderTarget2D ColorBuffer { get; set; }
    
    /// <summary>
    /// A texture that holds the normal sprite drawins
    /// </summary>
    public RenderTarget2D NormalBuffer { get; set; }

    /// <summary>
    /// A texture that holds the drawn lights
    /// </summary>
    public RenderTarget2D LightBuffer { get; set; }

    public DeferredRenderer()
    {
        var viewport = Core.GraphicsDevice.Viewport;
        
        ColorBuffer = new RenderTarget2D(
            graphicsDevice: Core.GraphicsDevice, 
            width: viewport.Width,
            height: viewport.Height,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color, 
            preferredDepthFormat: DepthFormat.None);
        
        NormalBuffer = new RenderTarget2D(
            graphicsDevice: Core.GraphicsDevice, 
            width: viewport.Width,
            height: viewport.Height,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color, 
            preferredDepthFormat: DepthFormat.None);

        LightBuffer = new RenderTarget2D(
            graphicsDevice: Core.GraphicsDevice, 
            width: viewport.Width,
            height: viewport.Height,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color, 
            preferredDepthFormat: DepthFormat.None);
    }
    
    public void StartColorPhase()
    {
        // all future draw calls will be drawn to the color buffer and normal buffer
        Core.GraphicsDevice.SetRenderTargets(new RenderTargetBinding[]
        {
            // gets the results from shader semantic COLOR0
            new RenderTargetBinding(ColorBuffer),
            
            // gets the results from shader semantic COLOR1
            new RenderTargetBinding(NormalBuffer)
        });
        Core.GraphicsDevice.Clear(Color.Transparent);
    }

    public void StartLightPhase()
    {
        // all future draw calls will be drawn to the light buffer
        Core.GraphicsDevice.SetRenderTarget(LightBuffer);
        Core.GraphicsDevice.Clear(Color.Black);
    }

    public void Finish()
    {
        // all future draw calls will be drawn to the screen
        //  note: 'null' means "the screen" in MonoGame
        Core.GraphicsDevice.SetRenderTarget(null);
    }

    public void DrawComposite(float ambient=.4f)
    {
        Core.DeferredCompositeMaterial.SetParameter("AmbientLight", ambient);
        Core.DeferredCompositeMaterial.SetParameter("LightBuffer", LightBuffer);
        var viewportBounds = Core.GraphicsDevice.Viewport.Bounds;
        Core.SpriteBatch.Begin(
            effect: Core.DeferredCompositeMaterial.Effect
            );
        Core.SpriteBatch.Draw(ColorBuffer, viewportBounds, Color.White);
        Core.SpriteBatch.End();   
    }

    public void DebugDraw()
    {
        var viewportBounds = Core.GraphicsDevice.Viewport.Bounds;
        
        // the debug view for the color buffer lives in the top-left.
        var colorBorderRect = new Rectangle(
            x: viewportBounds.X, 
            y: viewportBounds.Y, 
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);
        
        // shrink the color rect by 8 pixels
        var colorRect = colorBorderRect;
        colorRect.Inflate(-8, -8);
        
        
        // the debug view for the light buffer lives in the top-right.
        var lightBorderRect = new Rectangle(
            x: viewportBounds.Width / 2, 
            y: viewportBounds.Y, 
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);

        // shrink the light rect by 8 pixels
        var lightRect = lightBorderRect;
        lightRect.Inflate(-8, -8);

        // the debug view for the normal buffer lives in the top-right.
        var normalBorderRect = new Rectangle(
            x: viewportBounds.X, 
            y: viewportBounds.Height / 2, 
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);

        // shrink the normal rect by 8 pixels
        var normalRect = normalBorderRect;
        normalRect.Inflate(-8, -8);

        
        Core.SpriteBatch.Begin();
        
        // draw a debug border
        Core.SpriteBatch.Draw(Core.Pixel, colorBorderRect, Color.MonoGameOrange);
        
        // draw the color buffer
        Core.SpriteBatch.Draw(ColorBuffer, colorRect, Color.White);
        
        //draw a debug border
        Core.SpriteBatch.Draw(Core.Pixel, lightBorderRect, Color.CornflowerBlue);
        
        // draw the light buffer
        Core.SpriteBatch.Draw(LightBuffer, lightRect, Color.White);
        
        
        // draw a debug border
        Core.SpriteBatch.Draw(Core.Pixel, normalBorderRect, Color.MintCream);
        
        // draw the normal buffer
        Core.SpriteBatch.Draw(NormalBuffer, normalRect, Color.White);

        Core.SpriteBatch.End();
    }
}