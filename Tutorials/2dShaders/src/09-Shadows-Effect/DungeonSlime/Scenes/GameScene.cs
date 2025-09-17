using System;
using System.Collections.Generic;
using DungeonSlime.GameObjects;
using DungeonSlime.UI;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    // Reference to the slime.
    private Slime _slime;

    // Reference to the bat.
    private Bat _bat;

    // Defines the tilemap to draw.
    private Tilemap _tilemap;

    // The normal texture atlas
    private Texture2D _normalAtlas;

    // Defines the bounds of the room that the slime and bat are contained within.
    private Rectangle _roomBounds;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect;

    // Tracks the players score.
    private int _score;

    private GameSceneUI _ui;

    private GameState _state;

    // The amount of saturation to provide the grayscale shader effect
    private float _saturation = 1.0f;
    private Texture2D _colorMap;
    private RedColorMap _slimeColorMap;
    private TimeSpan _lastGrowTime;
    
    // The uber material for the game objects
    private Material _gameMaterial;
    private SpriteCamera3d _camera;

    // The deferred rendering resources
    private DeferredRenderer _deferredRenderer;
    
    // A list of point lights to be rendered
    private List<PointLight> _lights = new List<PointLight>();
    
    // A list of shadow casters for all the lights
    private List<ShadowCaster> _shadowCasters = new List<ShadowCaster>();

    // The speed of the fade to grayscale effect.
    private const float FADE_SPEED = 0.02f;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // During the game scene, we want to disable exit on escape. Instead,
        // the escape key will be used to return back to the title screen
        Core.ExitOnEscape = false;

        // Create the room bounds by getting the bounds of the screen then
        // using the Inflate method to "Deflate" the bounds by the width and
        // height of a tile so that the bounds only covers the inside room of
        // the dungeon tilemap.
        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        // Subscribe to the slime's BodyCollision event so that a game over
        // can be triggered when this event is raised.
        _slime.BodyCollision += OnSlimeBodyCollision;

        // Create any UI elements from the root element created in previous
        // scenes
        GumService.Default.Root.Children.Clear();

        // Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize a new game to be played.
        InitializeNewGame();
        
        // Create the deferred rendering resources
        _deferredRenderer = new DeferredRenderer();
        InitializeLights();
    }

    private void InitializeLights()
    {
        // torch 1
        _lights.Add(new PointLight
        {
            Position = new Vector2(260, 100),
            Color = Color.CornflowerBlue,
            Radius = 500
        });
        // torch 2
        _lights.Add(new PointLight
        {
            Position = new Vector2(520, 100),
            Color = Color.CornflowerBlue,
            Radius = 500
        });
        // torch 3
        _lights.Add(new PointLight
        {
            Position = new Vector2(740, 100),
            Color = Color.CornflowerBlue,
            Radius = 500
        });
        // torch 4
        _lights.Add(new PointLight
        {
            Position = new Vector2(1000, 100),
            Color = Color.CornflowerBlue,
            Radius = 500
        });
        
        // random lights
        _lights.Add(new PointLight
        {
            Position = new Vector2(Random.Shared.Next(50, 400),400),
            Color = Color.MonoGameOrange,
            Radius = 500
        });
        _lights.Add(new PointLight
        {
            Position = new Vector2(Random.Shared.Next(650, 1200),300),
            Color = Color.MonoGameOrange,
            Radius = 500
        });
        
        // An inner shadow caster
        var tileUnit = new Vector2(_tilemap.TileWidth, _tilemap.TileHeight);
        var size = new Vector2(_tilemap.Columns, _tilemap.Rows);
        _shadowCasters.Add(new ShadowCaster
        {
            Points = new List<Vector2>
            {
                tileUnit * new Vector2(1, 1),
                tileUnit * new Vector2(size.X - 1, 1),
                tileUnit * new Vector2(size.X - 1, size.Y - 1),
                tileUnit * new Vector2(1, size.Y - 1),
            }
        });
    }

    private void InitializeUI()
    {
        // Clear out any previous UI element incase we came here
        // from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _ui = new GameSceneUI();

        // Subscribe to the events from the game scene ui.
        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.RetryButtonClick += OnRetryButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        // Change the game state back to playing
        _state = GameState.Playing;
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        // Player has chosen to retry, so initialize a new game
        InitializeNewGame();
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        // Player has chosen to quit, so return back to the title scene
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeNewGame()
    {
        // Calculate the position for the slime, which will be at the center
        // tile of the tile map.
        Vector2 slimePos = new Vector2();
        slimePos.X = (_tilemap.Columns / 2) * _tilemap.TileWidth;
        slimePos.Y = (_tilemap.Rows / 2) * _tilemap.TileHeight;

        // Initialize the slime
        _slime.Initialize(slimePos, _tilemap.TileWidth);

        // Initialize the bat
        _bat.RandomizeVelocity();
        PositionBatAwayFromSlime();

        // Reset the score
        _score = 0;

        // Set the game state to playing
        _state = GameState.Playing;
    }

    public override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        // Create the tilemap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Create the animated sprite for the slime from the atlas.
        AnimatedSprite slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        slimeAnimation.Scale = new Vector2(4.0f, 4.0f);

        // Create the slime
        _slime = new Slime(slimeAnimation);

        // Create the animated sprite for the bat from the atlas.
        AnimatedSprite batAnimation = atlas.CreateAnimatedSprite("bat-animation");
        batAnimation.Scale = new Vector2(4.0f, 4.0f);

        // Load the bounce sound effect for the bat
        SoundEffect bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

        // Create the bat
        _bat = new Bat(batAnimation, bounceSoundEffect);

        // Load the collect sound effect
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        // Load the colorSwap map
        _colorMap = Content.Load<Texture2D>("images/color-map-dark-purple");
        _slimeColorMap = new RedColorMap();
        _slimeColorMap.SetColorsByExistingColorMap(_colorMap);
        _slimeColorMap.SetColorsByRedValue(new Dictionary<int, Color>
        {
            // main color
            [32] = Color.LightSteelBlue,
        }, false);

        // Load the normal maps
        _normalAtlas = Content.Load<Texture2D>("images/atlas-normal");
        
        // Load the game material
        _gameMaterial = Content.WatchMaterial("effects/gameEffect");
        _gameMaterial.SetParameter("ColorMap", _colorMap);
        _camera = new SpriteCamera3d();
        _gameMaterial.SetParameter("MatrixTransform", _camera.CalculateMatrixTransform());
        _gameMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));
        _gameMaterial.SetParameter("NormalMap", _normalAtlas);
    }

    private bool p = false;
    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.P))
        {
            p = !p;
        }

        if (p) return;
        
        // Ensure the UI is always updated
        _ui.Update(gameTime);

        // Set the camera view to look at the player slime
        var viewport = Core.GraphicsDevice.Viewport;
        var center = .5f * new Vector2(viewport.Width, viewport.Height);
        var slimePosition = new Vector2(_slime?.GetBounds().X ?? center.X, _slime?.GetBounds().Y ?? center.Y);
        var offset = .01f * (slimePosition - center);
        _camera.LookOffset = offset;

        var matrixTransform = _camera.CalculateMatrixTransform();
        _gameMaterial.SetParameter("MatrixTransform", matrixTransform);
        Core.PointLightMaterial.SetParameter("MatrixTransform", matrixTransform);
        Core.PointLightMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));
        Core.ShadowHullMaterial.SetParameter("MatrixTransform", matrixTransform);
        Core.ShadowHullMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));

        // Update the colorSwap material if it was changed
        _gameMaterial.Update();

        if (_state != GameState.Playing)
        {
            // The game is in either a paused or game over state, so
            // gradually decrease the saturation to create the fading grayscale.
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);

            // If its just a game over state, return back
            if (_state == GameState.GameOver)
            {
                return;
            }
        }
        else
        {
            _saturation = 1;
        }

        // If the pause button is pressed, toggle the pause state
        if (GameController.Pause())
        {
            TogglePause();
        }

        // At this point, if the game is paused, just return back early
        if (_state == GameState.Paused)
        {
            return;
        }

        // Update the slime;
        _slime.Update(gameTime);

        // Update the bat;
        _bat.Update(gameTime);

        // Perform collision checks
        CollisionChecks(gameTime);

        // Move some lights around for artistic effect
        MoveLightsAround(gameTime);
    }

    private void MoveLightsAround(GameTime gameTime)
    {
        var t = (float)gameTime.TotalGameTime.TotalSeconds * .25f;
        var bounds = Core.GraphicsDevice.Viewport.Bounds;
        bounds.Inflate(-100, -100);

        var halfWidth = bounds.Width / 2;
        var halfHeight = bounds.Height / 2;
        var center = bounds.Center.ToVector2();
        _lights[^1].Position = center + new Vector2(halfWidth * MathF.Cos(t), .7f * halfHeight * MathF.Sin(t * 1.1f));
        _lights[^2].Position = center + new Vector2(halfWidth * MathF.Cos(t + MathHelper.Pi), halfHeight * MathF.Sin(t - MathHelper.Pi));
    }
    
    private void CollisionChecks(GameTime gameTime)
    {
        // Capture the current bounds of the slime and bat
        Circle slimeBounds = _slime.GetBounds();
        Circle batBounds = _bat.GetBounds();

        // FIrst perform a collision check to see if the slime is colliding with
        // the bat, which means the slime eats the bat.
        if (slimeBounds.Intersects(batBounds))
        {
            // Move the bat to a new position away from the slime.
            PositionBatAwayFromSlime();

            // Randomize the velocity of the bat.
            _bat.RandomizeVelocity();

            // Tell the slime to grow.
            _slime.Grow();

            // Remember when the last time the slime grew
            _lastGrowTime = gameTime.TotalGameTime;

            // Increment the score.
            _score += 100;

            // Update the score display on the UI.
            _ui.UpdateScoreText(_score);

            // Play the collect sound effect
            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }

        // Next check if the slime is colliding with the wall by validating if
        // it is within the bounds of the room.  If it is outside the room
        // bounds, then it collided with a wall which triggers a game over.
        if (slimeBounds.Top < _roomBounds.Top ||
           slimeBounds.Bottom > _roomBounds.Bottom ||
           slimeBounds.Left < _roomBounds.Left ||
           slimeBounds.Right > _roomBounds.Right)
        {
            GameOver();
            return;
        }

        // Finally, check if the bat is colliding with a wall by validating if
        // it is within the bounds of the room.  If it is outside the room
        // bounds, then it collided with a wall, and the bat should bounce
        // off of that wall.
        if (batBounds.Top < _roomBounds.Top)
        {
            _bat.Bounce(Vector2.UnitY);
        }
        else if (batBounds.Bottom > _roomBounds.Bottom)
        {
            _bat.Bounce(-Vector2.UnitY);
        }

        if (batBounds.Left < _roomBounds.Left)
        {
            _bat.Bounce(Vector2.UnitX);
        }
        else if (batBounds.Right > _roomBounds.Right)
        {
            _bat.Bounce(-Vector2.UnitX);
        }
    }

    private void PositionBatAwayFromSlime()
    {
        // Calculate the position that is in the center of the bounds
        // of the room.
        float roomCenterX = _roomBounds.X + _roomBounds.Width * 0.5f;
        float roomCenterY = _roomBounds.Y + _roomBounds.Height * 0.5f;
        Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);

        // Get the bounds of the slime and calculate the center position
        Circle slimeBounds = _slime.GetBounds();
        Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);

        // Calculate the distance vector from the center of the room to the
        // center of the slime.
        Vector2 centerToSlime = slimeCenter - roomCenter;

        // Get the bounds of the bat
        Circle batBounds = _bat.GetBounds();

        // Calculate the amount of padding we will add to the new position of
        // the bat to ensure it is not sticking to walls
        int padding = batBounds.Radius * 2;

        // Calculate the new position of the bat by finding which component of
        // the center to slime vector (X or Y) is larger and in which direction.
        Vector2 newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            // The slime is closer to either the left or right wall, so the Y
            // position will be a random position between the top and bottom
            // walls.
            newBatPosition.Y = Random.Shared.Next(
                _roomBounds.Top + padding,
                _roomBounds.Bottom - padding
            );

            if (centerToSlime.X > 0)
            {
                // The slime is closer to the right side wall, so place the
                // bat on the left side wall
                newBatPosition.X = _roomBounds.Left + padding;
            }
            else
            {
                // The slime is closer ot the left side wall, so place the
                // bat on the right side wall.
                newBatPosition.X = _roomBounds.Right - padding * 2;
            }
        }
        else
        {
            // The slime is closer to either the top or bottom wall, so the X
            // position will be a random position between the left and right
            // walls.
            newBatPosition.X = Random.Shared.Next(
                _roomBounds.Left + padding,
                _roomBounds.Right - padding
            );

            if (centerToSlime.Y > 0)
            {
                // The slime is closer to the top wall, so place the bat on the
                // bottom wall
                newBatPosition.Y = _roomBounds.Top + padding;
            }
            else
            {
                // The slime is closer to the bottom wall, so place the bat on
                // the top wall.
                newBatPosition.Y = _roomBounds.Bottom - padding * 2;
            }
        }

        // Assign the new bat position
        _bat.Position = newBatPosition;
    }

    private void OnSlimeBodyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            // We're now unpausing the game, so hide the pause panel
            _ui.HidePausePanel();

            // And set the state back to playing
            _state = GameState.Playing;
        }
        else
        {
            // We're now pausing the game, so show the pause panel
            _ui.ShowPausePanel();

            // And set the state to paused
            _state = GameState.Paused;

            // Set the grayscale effect saturation to 1.0f;
            _saturation = 1.0f;
        }
    }

    private void GameOver()
    {
        // Show the game over panel
        _ui.ShowGameOverPanel();

        // Set the game state to game over
        _state = GameState.GameOver;

        // Set the grayscale effect saturation to 1.0f;
        _saturation = 1.0f;
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(new Color(32, 16, 20));
        
        _gameMaterial.SetParameter("Saturation", _saturation);
        
        // Start rendering to the deferred renderer
        _deferredRenderer.StartColorPhase();
        Core.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            sortMode: SpriteSortMode.Immediate,
            rasterizerState: RasterizerState.CullNone,
            effect: _gameMaterial.Effect);

        // Update the colorMap
        _gameMaterial.SetParameter("ColorMap", _colorMap);
        
        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);
        
        // Draw the bat.
        _bat.Draw();
        
        // Draw the slime.
        _slime.Draw(segmentIndex =>
        {
            const int flashTimeMs = 125;
            var map = _colorMap;
            var elapsedMs = (gameTime.TotalGameTime.TotalMilliseconds - _lastGrowTime.TotalMilliseconds);
            var intervalsAgo = (int)(elapsedMs / flashTimeMs);

            if (intervalsAgo < _slime.Size && (intervalsAgo - segmentIndex) % _slime.Size == 0)
            {
                map = _slimeColorMap.ColorMap;
            }
            
            _gameMaterial.SetParameter("ColorMap", map);
        });

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        // render the shadow buffers
        var casters = new List<ShadowCaster>();
        casters.AddRange(_shadowCasters);
        casters.AddRange(_slime.ShadowCasters);
        casters.Add(_bat.ShadowCaster);
        
        // start rendering the lights
        _deferredRenderer.DrawLights(_lights, casters);
        
        // finish the deferred rendering
        _deferredRenderer.Finish();
        
        _deferredRenderer.DrawComposite();

        // Draw the UI
        _ui.Draw();
        
        // Render the debug view for the game
        //_deferredRenderer.DebugDraw();

    }
}
