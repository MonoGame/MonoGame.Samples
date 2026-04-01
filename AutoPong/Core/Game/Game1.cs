using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using myStartkit.Core.Inputs;

namespace AutoPong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly InputState inputState = new InputState();
    private SpriteFont hudFont;
    private int playerIndex = (int)PlayerIndex.One;
    private Color textDrawColour = Color.White;
    private int textStartPosition = 20;
    private int positionSpacing = 40;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = 2560;
        _graphics.PreferredBackBufferHeight = 1440;
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
        hudFont = Content.Load<SpriteFont>("Fonts/Hud");
    }

    protected override void Update(GameTime gameTime)
    {
        inputState.Update(gameTime, GraphicsDevice.Viewport);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        string state = inputState.CurrentGamePadStates[playerIndex].IsConnected ? "Connected" : "Disconnected";

        string connectedValue = $"GamePad: {state}";
        string AbuttonValue = $"A Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.A)}";
        string BbuttonValue = $"B Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.B)}";
        string XbuttonValue = $"X Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.X)}";
        string YbuttonValue = $"Y Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.Y)}";

        string StartbuttonValue = $"Start Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.Start)}";
        string BackbuttonValue = $"Back Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.Back)}";
        string BigbuttonValue = $"Big Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.BigButton)}";

        string DPadLbuttonValue = $"DPadL Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.DPadLeft)}";
        string DPadRbuttonValue = $"DPadR Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.DPadRight)}";
        string DPadUbuttonValue = $"DPadU Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.DPadUp)}";
        string DPadDbuttonValue = $"DPadD Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.DPadDown)}";

        string RBumperbuttonValue = $"RBumper Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.RightShoulder)}";
        string RThumbstickbuttonValue = $"RThumbstick Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.RightThumbstickDown)}";
        string RThumbstickStateValue = $"RThumbstick State: {inputState.CurrentGamePadStates[playerIndex].ThumbSticks.Right}";
        string RTriggerValue = $"RTrigger Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.RightTrigger)}";
        string RTriggerStateValue = $"RTrigger State: {inputState.CurrentGamePadStates[playerIndex].Triggers.Right}";

        string LBumperbuttonValue = $"LBumper Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.LeftShoulder)}";
        string LThumbstickbuttonValue = $"LThumbstick Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.LeftThumbstickDown)}";
        string LThumbstickStateValue = $"LThumbstick State: {inputState.CurrentGamePadStates[playerIndex].ThumbSticks.Left}";
        string LTriggerValue = $"LTrigger Button: {inputState.CurrentGamePadStates[playerIndex].IsButtonDown(Buttons.LeftTrigger)}";
        string LTriggerStateValue = $"LTrigger State: {inputState.CurrentGamePadStates[playerIndex].Triggers.Left}";
        _spriteBatch.Begin();
        // TODO: Add your drawing code here
        _spriteBatch.DrawString(hudFont, connectedValue, new Vector2(20, textStartPosition), textDrawColour);
        DrawInputValue(AbuttonValue, 1);
        DrawInputValue(BbuttonValue, 2);
        DrawInputValue(XbuttonValue, 3);
        DrawInputValue(YbuttonValue, 4);

        DrawInputValue(StartbuttonValue, 5);
        DrawInputValue(BackbuttonValue, 6);
        DrawInputValue(BigbuttonValue, 7);

        DrawInputValue(DPadLbuttonValue, 8);
        DrawInputValue(DPadRbuttonValue, 9);
        DrawInputValue(DPadUbuttonValue, 10);
        DrawInputValue(DPadDbuttonValue, 11);

        DrawInputValue(LBumperbuttonValue, 12);
        DrawInputValue(LThumbstickbuttonValue, 13);
        DrawInputValue(LThumbstickStateValue, 14);
        DrawInputValue(LTriggerValue, 15);
        DrawInputValue(LTriggerStateValue, 16);

        DrawInputValue(RBumperbuttonValue, 17);
        DrawInputValue(RThumbstickbuttonValue, 18);
        DrawInputValue(RThumbstickStateValue, 19);
        DrawInputValue(RTriggerValue, 20);
        DrawInputValue(RTriggerStateValue, 21);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawInputValue(string AbuttonValue, int index)
    {
        _spriteBatch.DrawString(hudFont, AbuttonValue, new Vector2(20, GetTextPositionForIndex(index)), textDrawColour);
    }

    private int GetTextPositionForIndex(int index)
    {
        return textStartPosition + positionSpacing * index;
    }
}
