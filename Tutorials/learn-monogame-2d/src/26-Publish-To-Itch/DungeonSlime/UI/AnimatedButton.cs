using System;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Graphics.Animation;
using Gum.Managers;
using Microsoft.Xna.Framework.Input;
using MonoGameGum.Forms.Controls;
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
        // Create the top-level container that will hold all visual elements
        // Width is relative to children with extra padding, height is fixed
        ContainerRuntime topLevelContainer = new ContainerRuntime();
        topLevelContainer.Height = 14f;
        topLevelContainer.HeightUnits = DimensionUnitType.Absolute;
        topLevelContainer.Width = 21f;
        topLevelContainer.WidthUnits = DimensionUnitType.RelativeToChildren;

        // Create the nine-slice background that will display the button graphics
        // A nine-slice allows the button to stretch while preserving corner appearance
        NineSliceRuntime nineSliceInstance = new NineSliceRuntime();
        nineSliceInstance.Height = 0f;
        nineSliceInstance.Texture = atlas.Texture;
        nineSliceInstance.TextureAddress = TextureAddress.Custom;
        nineSliceInstance.Dock(Gum.Wireframe.Dock.Fill);
        topLevelContainer.Children.Add(nineSliceInstance);

        // Create the text element that will display the button's label
        TextRuntime textInstance = new TextRuntime();
        // Name is required so it hooks in to the base Button.Text property
        textInstance.Name = "TextInstance";
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
        topLevelContainer.Children.Add(textInstance);

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
        nineSliceInstance.AnimationChains = new AnimationChainList
        {
            unfocusedAnimation,
            focusedAnimation
        };

        // Create a state category for button states
        StateSaveCategory category = new StateSaveCategory();
        category.Name = Button.ButtonCategoryName;
        topLevelContainer.AddCategory(category);

        // Create the enabled (default/unfocused) state
        StateSave enabledState = new StateSave();
        enabledState.Name = FrameworkElement.EnabledStateName;
        enabledState.Apply = () =>
        {
            // When enabled but not focused, use the unfocused animation
            nineSliceInstance.CurrentChainName = unfocusedAnimation.Name;
        };
        category.States.Add(enabledState);

        // Create the focused state
        StateSave focusedState = new StateSave();
        focusedState.Name = FrameworkElement.FocusedStateName;
        focusedState.Apply = () =>
        {
            // When focused, use the focused animation and enable animation playback
            nineSliceInstance.CurrentChainName = focusedAnimation.Name;
            nineSliceInstance.Animate = true;
        };
        category.States.Add(focusedState);

        // Create the highlighted+focused state (for mouse hover while focused)
        // by cloning the focused state since they appear the same
        StateSave highlightedFocused = focusedState.Clone();
        highlightedFocused.Name = FrameworkElement.HighlightedFocusedStateName;
        category.States.Add(highlightedFocused);

        // Create the highlighted state (for mouse hover)
        // by cloning the enabled state since they appear the same
        StateSave highlighted = enabledState.Clone();
        highlighted.Name = FrameworkElement.HighlightedStateName;
        category.States.Add(highlighted);

        // Add event handlers for keyboard input.
        KeyDown += HandleKeyDown;

        // Add event handler for mouse hover focus.
        topLevelContainer.RollOn += HandleRollOn;

        // Assign the configured container as this button's visual
        Visual = topLevelContainer;
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
