using System;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals;
using Gum.Graphics.Animation;
using Gum.Managers;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;

namespace DungeonSlime.UI;

/// <summary>
/// A custom button implementation that inherits from Gum's Button class to provide
/// animated visual feedback when focused.
/// </summary>
internal class AnimatedButton : Button
{
    /// <summary>
    /// Creates a new AnimatedButton instance using graphics from the specified texture atlas.
    /// </summary>
    /// <param name="atlas">The texture atlas containing button graphics and animations</param>
    public AnimatedButton(TextureAtlas atlas)
    {
        // Each Forms conrol has a general Visual property that
        // has properties shared by all control types. This Visual
        // type matches the Forms type. It can be casted to access
        // controls-specific properties.
        ButtonVisual buttonVisual = (ButtonVisual)Visual;
        // Width is relative to children with extra padding, height is fixed
        buttonVisual.Height = 14f;
        buttonVisual.HeightUnits = DimensionUnitType.Absolute;
        buttonVisual.Width = 21f;
        buttonVisual.WidthUnits = DimensionUnitType.RelativeToChildren;

        // Get a reference to the nine-slice background to display the button graphics
        // A nine-slice allows the button to stretch while preserving corner appearance
        NineSliceRuntime background = buttonVisual.Background;
        background.Texture = atlas.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.Color = Microsoft.Xna.Framework.Color.White;
        // texture coordinates for the background are set down below

        TextRuntime textInstance = buttonVisual.TextInstance;
        textInstance.Text = "START";
        textInstance.Blue = 130;
        textInstance.Green = 86;
        textInstance.Red = 70;
        textInstance.UseCustomFont = true;
        textInstance.CustomFontFile = "fonts/04b_30.fnt";
        textInstance.FontScale = 0.25f;
        textInstance.Anchor(Gum.Wireframe.Anchor.Center);
        textInstance.Width = 0;
        textInstance.WidthUnits = DimensionUnitType.RelativeToChildren;

        // Get the texture region for the unfocused button state from the atlas
        TextureRegion unfocusedTextureRegion = atlas.GetRegion("unfocused-button");

        // Create an animation chain for the unfocused state with a single frame
        AnimationChain unfocusedAnimation = new AnimationChain();
        unfocusedAnimation.Name = nameof(unfocusedAnimation);
        AnimationFrame unfocusedFrame = new AnimationFrame
        {
            TopCoordinate = unfocusedTextureRegion.TopTextureCoordinate,
            BottomCoordinate = unfocusedTextureRegion.BottomTextureCoordinate,
            LeftCoordinate = unfocusedTextureRegion.LeftTextureCoordinate,
            RightCoordinate = unfocusedTextureRegion.RightTextureCoordinate,
            FrameLength = 0.3f,
            Texture = unfocusedTextureRegion.Texture
        };
        unfocusedAnimation.Add(unfocusedFrame);

        // Get the multi-frame animation for the focused button state from the atlas
        Animation focusedAtlasAnimation = atlas.GetAnimation("focused-button-animation");

        // Create an animation chain for the focused state using all frames from the atlas animation
        AnimationChain focusedAnimation = new AnimationChain();
        focusedAnimation.Name = nameof(focusedAnimation);
        foreach (TextureRegion region in focusedAtlasAnimation.Frames)
        {
            AnimationFrame frame = new AnimationFrame
            {
                TopCoordinate = region.TopTextureCoordinate,
                BottomCoordinate = region.BottomTextureCoordinate,
                LeftCoordinate = region.LeftTextureCoordinate,
                RightCoordinate = region.RightTextureCoordinate,
                FrameLength = (float)focusedAtlasAnimation.Delay.TotalSeconds,
                Texture = region.Texture
            };

            focusedAnimation.Add(frame);
        }

        // Assign both animation chains to the nine-slice background
        background.AnimationChains = new AnimationChainList
        {
            unfocusedAnimation,
            focusedAnimation
        };


        // Reset all state to default so we don't have unexpected variable assignments:
        buttonVisual.ButtonCategory.ResetAllStates();

        // Get the enabled (default/unfocused) state
        StateSave enabledState = buttonVisual.States.Enabled;
        enabledState.Apply = () =>
        {
            // When enabled but not focused, use the unfocused animation
            background.CurrentChainName = unfocusedAnimation.Name;
        };

        // Create the focused state
        StateSave focusedState = buttonVisual.States.Focused;
        focusedState.Apply = () =>
        {
            // When focused, use the focused animation and enable animation playback
            background.CurrentChainName = focusedAnimation.Name;
            background.Animate = true;
        };

        // Create the highlighted+focused state (for mouse hover while focused)
        StateSave highlightedFocused = buttonVisual.States.HighlightedFocused;
        highlightedFocused.Apply = focusedState.Apply;

        // Create the highlighted state (for mouse hover)
        // by cloning the enabled state since they appear the same
        StateSave highlighted = buttonVisual.States.Highlighted;
        highlighted.Apply = enabledState.Apply;

        // Add event handlers for keyboard input.
        KeyDown += HandleKeyDown;

        // Add event handler for mouse hover focus.
        buttonVisual.RollOn += HandleRollOn;
    }

    /// <summary>
    /// Handles keyboard input for navigation between buttons using left/right keys.
    /// </summary>
    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Left)
        {
            // Left arrow navigates to previous control
            HandleTab(TabDirection.Up, loop: true);
        }
        if (e.Key == Keys.Right)
        {
            // Right arrow navigates to next control
            HandleTab(TabDirection.Down, loop: true);
        }
    }

    /// <summary>
    /// Automatically focuses the button when the mouse hovers over it.
    /// </summary>
    private void HandleRollOn(object sender, EventArgs e)
    {
        IsFocused = true;
    }
}
