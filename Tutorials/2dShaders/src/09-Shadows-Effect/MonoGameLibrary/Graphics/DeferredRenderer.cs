using System;
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
    
    /// <summary>
    /// The state used when writing shadow hulls
    /// </summary>
    private DepthStencilState _stencilWrite;
    
    /// <summary>
    /// The state used when drawing point lights
    /// </summary>
    private DepthStencilState _stencilTest;
    
    /// <summary>
    /// The state used to prepare the light buffer
    /// </summary>
    private DepthStencilState _stencilSetup;
    
    /// <summary>
    /// A custom blend state that wont write any color data
    /// </summary>
    private BlendState _shadowBlendState;

    private AlphaTestEffect _alphaTest;


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
            preferredDepthFormat: DepthFormat.Depth24Stencil8);
        
        _stencilWrite = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // instruct every fragment to interact with the stencil buffer
            StencilFunction = CompareFunction.GreaterEqual,
            
            // every operation will increase the shadow value (up to the max of 255), but only when the original 
            //  stencil value was greater or equal to '1'. ('1' is the default clear value)
            StencilPass = StencilOperation.IncrementSaturation,
            
            // this is the value that will be written into the stencil buffer
            ReferenceStencil = 1,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        _stencilTest = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // instruct only fragments that have a current value greater or equal to the
            //  ReferenceStencil value to interact
            StencilFunction = CompareFunction.GreaterEqual,
            
            // '1' is the minimum value for shadow.
            ReferenceStencil = 1,
            
            // don't change the value of the stencil buffer. KEEP the current value.
            StencilPass = StencilOperation.Keep,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        
        _stencilSetup = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // in the setup, always set the pixel to '0'
            StencilFunction = CompareFunction.Always,
            
            // Write a '0' anywhere we don't want a shadow to appear
            ReferenceStencil = 0,
            
            // don't change the value of the stencil buffer. KEEP the current value.
            StencilPass = StencilOperation.Replace,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        
        _shadowBlendState = new BlendState
        {
            // no color channels will be written into the render target
            ColorWriteChannels = ColorWriteChannels.None
        };

        _alphaTest = new AlphaTestEffect(Core.GraphicsDevice);
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

    public void StartLightStencilPhase()
    {
        Core.GraphicsDevice.SetRenderTarget(LightBuffer);
        Core.GraphicsDevice.Clear(Color.Black);
        
        Core.SpriteBatch.Begin(blendState:_shadowBlendState, depthStencilState: _stencilSetup);
    }

    public void EndLightStencilPhase()
    {
        Core.SpriteBatch.End();
    }
    
    public void DrawLights(List<PointLight> lights, List<ShadowCaster> shadowCasters, Action<BlendState, DepthStencilState> prepareStencil)
    {
        
        
        _stencilWrite = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // instruct every fragment to interact with the stencil buffer
            StencilFunction = CompareFunction.LessEqual,
            
            // every operation will increase the shadow value (up to the max of 255), but only when the original 
            //  stencil value was greater or equal to '1'. ('1' is the default clear value)
            StencilPass = StencilOperation.IncrementSaturation,

            // this is the value that will be written into the stencil buffer
            ReferenceStencil = 1,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        _stencilTest = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // instruct only fragments that have a current value greater or equal to the
            //  ReferenceStencil value to interact
            StencilFunction = CompareFunction.GreaterEqual,
            
            // '1' is the minimum value for shadow.
            ReferenceStencil = 1,
            
            // don't change the value of the stencil buffer. KEEP the current value.
            StencilPass = StencilOperation.Keep,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        
        _stencilSetup = new DepthStencilState
        {
            // instruct MonoGame to use the stencil buffer
            StencilEnable = true,
            
            // in the setup, always set the pixel to '0'
            StencilFunction = CompareFunction.Always,
            
            // Write a '0' anywhere we don't want a shadow to appear
            ReferenceStencil = 0,
            
            // don't change the value of the stencil buffer. KEEP the current value.
            StencilPass = StencilOperation.Replace,
            
            // ignore depth from the stencil buffer write/reads
            DepthBufferEnable = false
        };
        
        
        
        
        Core.GraphicsDevice.SetRenderTarget(LightBuffer);
        Core.GraphicsDevice.Clear(Color.Black);
        // foreach (var light in lights)
        for (var l = 0 ; l < lights.Count - 0 ; l ++)
        {
            var light = lights[l];
            // initialize the stencil to '1'.
            Core.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 1);
            
            // Anything that draws in this setup will set the stencil back to '0'. This '0' acts as a "don't draw a shadow here". 
            prepareStencil?.Invoke(_shadowBlendState, _stencilSetup);
            
            
            Core.ShadowHullMaterial.SetParameter("LightPosition", light.Position);
            
            Core.SpriteBatch.Begin(
                depthStencilState: _stencilWrite,
                effect: Core.ShadowHullMaterial.Effect,
                blendState: _shadowBlendState,
                rasterizerState: RasterizerState.CullNone
            );
            foreach (var caster in shadowCasters)
            {
                for (var i = 0; i < caster.Points.Count; i++)
                {
                    var a = caster.Position + caster.Points[i];
                    var b = caster.Position + caster.Points[(i + 1) % caster.Points.Count];
            
                    var screenSize = new Vector2(LightBuffer.Width, LightBuffer.Height);
                    var aToB = (b - a) / screenSize;
                    var packed = PointLight.PackVector2_SNorm(aToB);
                    Core.SpriteBatch.Draw(Core.Pixel, a, packed);
                }
            }
            
            Core.SpriteBatch.End();
        
            
            Core.SpriteBatch.Begin(
                depthStencilState: _stencilTest,
                effect: Core.PointLightMaterial.Effect,
                blendState: BlendState.Additive
            );

            var diameter = light.Radius * 2;
            var rect = new Rectangle(
                (int)(light.Position.X - light.Radius), 
                (int)(light.Position.Y - light.Radius),
                diameter, diameter);
            Core.SpriteBatch.Draw(NormalBuffer, rect, light.Color);
            Core.SpriteBatch.End();

        }
        
        Core.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);
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
        
        // draw the light buffer
        Core.SpriteBatch.Draw(NormalBuffer, normalRect, Color.White);

        Core.SpriteBatch.End();
    }
}