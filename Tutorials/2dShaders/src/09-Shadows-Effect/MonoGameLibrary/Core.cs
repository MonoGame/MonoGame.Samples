using System;
using System.Collections.Generic;
using ImGuiNET.SampleProgram.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    // The scene that is currently active.
    private static Scene s_activeScene;

    // The next scene to switch to, if there is one.
    private static Scene s_nextScene;
    
    /// <summary>
    /// The material that is used when changing scenes
    /// </summary>
    public static Material SceneTransitionMaterial { get; private set; }
    
    /// <summary>
    /// The material that draws point lights
    /// </summary>
    public static Material PointLightMaterial { get; private set; }
    
    /// <summary>
    /// The  material that draws shadow hulls
    /// </summary>
    public static Material ShadowHullMaterial { get; private set; }
    
    /// <summary>
    /// The material that combines the various off screen textures
    /// </summary>
    public static Material DeferredCompositeMaterial { get; private set; }
    
    /// <summary>
    /// A set of grayscale gradient textures to use as transition guides
    /// </summary>
    public static List<Texture2D> SceneTransitionTextures { get; private set; }
    
    /// <summary>
    /// The current transition between scenes
    /// </summary>
    public static SceneTransition SceneTransition { get; protected set; } = SceneTransition.Open(1000);

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }
    
    /// <summary>
    /// Gets a runtime generated 1x1 pixel texture.
    /// </summary>
    public static Texture2D Pixel { get; private set; }
    
    /// <summary>
    /// Gets the ImGui renderer used for debug UIs.
    /// </summary>
    public static ImGuiRenderer ImGuiRenderer { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }
    
    /// <summary>
    /// Gets the content manager that can load global assets from the SharedContent folder.
    /// </summary>
    public static ContentManager SharedContent { get; private set; }

    /// <summary>
    /// Gets a reference to to the input management system.
    /// </summary>
    public static InputManager Input { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    /// <summary>
    /// Gets a reference to the audio control system.
    /// </summary>
    public static AudioController Audio { get; private set; }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;

        // Apply the graphic presentation changes
        Graphics.ApplyChanges();

        // Set the window title
        Window.Title = title;

        // Set the core's content manager to a reference of hte base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content
        Content.RootDirectory = "Content";

        // Set the core's shared content manager, pointing to the SharedContent folder.
        SharedContent = new ContentManager(Services, "SharedContent");

        // Mouse is visible by default
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Create the ImGui renderer.
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();

        // Create a new input manager
        Input = new InputManager();

        // Create a new audio controller.
        Audio = new AudioController();
        
        // Create a 1x1 white pixel texture for drawing quads.
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new Color[]{ Color.White });
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        DeferredCompositeMaterial = SharedContent.WatchMaterial("effects/deferredCompositeEffect");

        PointLightMaterial = SharedContent.WatchMaterial("effects/pointLightEffect");
        PointLightMaterial.SetParameter("LightBrightness", .25f);
        PointLightMaterial.SetParameter("LightSharpness", .1f);

        ShadowHullMaterial = SharedContent.WatchMaterial("effects/shadowHullEffect");

        SceneTransitionMaterial = SharedContent.WatchMaterial("effects/sceneTransitionEffect");
        SceneTransitionMaterial.SetParameter("EdgeWidth", .05f);

        SceneTransitionTextures = new List<Texture2D>();
        SceneTransitionTextures.Add(SharedContent.Load<Texture2D>("images/angled"));
        SceneTransitionTextures.Add(SharedContent.Load<Texture2D>("images/concave"));
        SceneTransitionTextures.Add(SharedContent.Load<Texture2D>("images/radial"));
        SceneTransitionTextures.Add(SharedContent.Load<Texture2D>("images/ripple"));
    }

    protected override void UnloadContent()
    {
        // Dispose of the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);

        // Update the audio controller.
        Audio.Update();

        if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Exit();
        }
        
        // if there is a next scene waiting to be switch to, then transition
        // to that scene
        if (s_nextScene != null && SceneTransition.IsComplete)
        {
            TransitionScene();
        }

        // If there is an active scene, update it.
        if (s_activeScene != null)
        {
            s_activeScene.Update(gameTime);
        }
        
        // Check if the scene transition material needs to be reloaded.
        SceneTransitionMaterial.SetParameter("Progress", SceneTransition.DirectionalRatio);
        SceneTransitionMaterial.Update();

        PointLightMaterial.Update();
        ShadowHullMaterial.Update();
        DeferredCompositeMaterial.Update();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        if (s_activeScene != null)
        {
            s_activeScene.Draw(gameTime);
        }
        
        // Draw the scene transition quad
        SpriteBatch.Begin(effect: SceneTransitionMaterial.Effect);
        SpriteBatch.Draw(SceneTransitionTextures[SceneTransition.TextureIndex % SceneTransitionTextures.Count], GraphicsDevice.Viewport.Bounds, Color.White);
        SpriteBatch.End();
        
        Material.DrawVisibleDebugUi(gameTime);

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (s_activeScene != next)
        {
            s_nextScene = next;
            SceneTransition = SceneTransition.Close(250);
        }
    }

    private static void TransitionScene()
    {
        SceneTransition = SceneTransition.Open(500);

        // If there is an active scene, dispose of it
        if (s_activeScene != null)
        {
            s_activeScene.Dispose();
        }

        // Force the garbage collector to collect to ensure memory is cleared
        GC.Collect();

        // Change the currently active scene to the new scene
        s_activeScene = s_nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        s_nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        if (s_activeScene != null)
        {
            s_activeScene.Initialize();
        }
    }
}
