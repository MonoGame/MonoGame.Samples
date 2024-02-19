using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DXShaderTest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        VertexBuffer buffer;
        VertexPositionTexture[] verts = new VertexPositionTexture[]
        {
            new VertexPositionTexture(){ Position = new Vector3(.5f,.5f,0), TextureCoordinate = new Vector2(0,0) },
            new VertexPositionTexture(){ Position = new Vector3(-.5f,.5f,0), TextureCoordinate = new Vector2(1,0) },
            new VertexPositionTexture(){ Position = new Vector3(-.5f,-.5f,0), TextureCoordinate = new Vector2(1,1) },
            new VertexPositionTexture(){ Position = new Vector3(.5f,-.5f,0), TextureCoordinate = new Vector2(0,1) },
            new VertexPositionTexture(){ Position = new Vector3(-5f,.5f,0), TextureCoordinate = new Vector2(0,0) },
            new VertexPositionTexture(){ Position = new Vector3(-.5f,.5f,0), TextureCoordinate = new Vector2(1,0) },
        };
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            if (buffer == null)
            {
                verts = new 
                buffer = new VertexBuffer(GraphicsDevice);

                buffer.SetData<VertexPositionTexture>(vertices, 0,4);
            }


            GraphicsDevice.SetVertexBuffer(buffer);
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            base.Draw(gameTime);
        }
    }
}
