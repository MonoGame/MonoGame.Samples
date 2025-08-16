using System;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;
using MonoGameLibrary.Graphics;

namespace DungeonSlime.UI;

/// <summary>
/// A custom slider control that inherits from Gum's Slider class.
/// </summary>
public class OptionsSlider : Slider
{
    // Reference to the text label that displays the slider's title
    private TextRuntime _textInstance;

    // Reference to the rectangle that visually represents the current value
    private ColoredRectangleRuntime _fillRectangle;

    /// <summary>
    /// Gets or sets the text label for this slider.
    /// </summary>
    public string Text
    {
        get => _textInstance.Text;
        set => _textInstance.Text = value;
    }

    /// <summary>
    /// Creates a new OptionsSlider instance using graphics from the specified texture atlas.
    /// </summary>
    /// <param name="atlas">The texture atlas containing slider graphics.</param>
    public OptionsSlider(TextureAtlas atlas)
    {
        // Create the top-level container for all visual elements
        ContainerRuntime topLevelContainer = new ContainerRuntime();
        topLevelContainer.Height = 55f;
        topLevelContainer.Width = 264f;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        // Create the background panel that contains everything
        NineSliceRuntime background = new NineSliceRuntime();
        background.Texture = atlas.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureWidth = backgroundRegion.Width;
        background.Dock(Gum.Wireframe.Dock.Fill);
        topLevelContainer.AddChild(background);

        // Create the title text element
        _textInstance = new TextRuntime();
        _textInstance.CustomFontFile = @"fonts/04b_30.fnt";
        _textInstance.UseCustomFont = true;
        _textInstance.FontScale = 0.5f;
        _textInstance.Text = "Replace Me";
        _textInstance.X = 10f;
        _textInstance.Y = 10f;
        _textInstance.WidthUnits = DimensionUnitType.RelativeToChildren;
        topLevelContainer.AddChild(_textInstance);

        // Create the container for the slider track and decorative elements
        ContainerRuntime innerContainer = new ContainerRuntime();
        innerContainer.Height = 13f;
        innerContainer.Width = 241f;
        innerContainer.X = 10f;
        innerContainer.Y = 33f;
        topLevelContainer.AddChild(innerContainer);

        TextureRegion offBackgroundRegion = atlas.GetRegion("slider-off-background");

        // Create the "OFF" side of the slider (left end)
        NineSliceRuntime offBackground = new NineSliceRuntime();
        offBackground.Dock(Gum.Wireframe.Dock.Left);
        offBackground.Texture = atlas.Texture;
        offBackground.TextureAddress = TextureAddress.Custom;
        offBackground.TextureHeight = offBackgroundRegion.Height;
        offBackground.TextureLeft = offBackgroundRegion.SourceRectangle.Left;
        offBackground.TextureTop = offBackgroundRegion.SourceRectangle.Top;
        offBackground.TextureWidth = offBackgroundRegion.Width;
        offBackground.Width = 28f;
        offBackground.WidthUnits = DimensionUnitType.Absolute;
        offBackground.Dock(Gum.Wireframe.Dock.Left);
        innerContainer.AddChild(offBackground);

        TextureRegion middleBackgroundRegion = atlas.GetRegion("slider-middle-background");

        // Create the middle track portion of the slider
        NineSliceRuntime middleBackground = new NineSliceRuntime();
        middleBackground.Dock(Gum.Wireframe.Dock.FillVertically);
        middleBackground.Texture = middleBackgroundRegion.Texture;
        middleBackground.TextureAddress = TextureAddress.Custom;
        middleBackground.TextureHeight = middleBackgroundRegion.Height;
        middleBackground.TextureLeft = middleBackgroundRegion.SourceRectangle.Left;
        middleBackground.TextureTop = middleBackgroundRegion.SourceRectangle.Top;
        middleBackground.TextureWidth = middleBackgroundRegion.Width;
        middleBackground.Width = 179f;
        middleBackground.WidthUnits = DimensionUnitType.Absolute;
        middleBackground.Dock(Gum.Wireframe.Dock.Left);
        middleBackground.X = 27f;
        innerContainer.AddChild(middleBackground);

        TextureRegion maxBackgroundRegion = atlas.GetRegion("slider-max-background");

        // Create the "MAX" side of the slider (right end)
        NineSliceRuntime maxBackground = new NineSliceRuntime();
        maxBackground.Texture = maxBackgroundRegion.Texture;
        maxBackground.TextureAddress = TextureAddress.Custom;
        maxBackground.TextureHeight = maxBackgroundRegion.Height;
        maxBackground.TextureLeft = maxBackgroundRegion.SourceRectangle.Left;
        maxBackground.TextureTop = maxBackgroundRegion.SourceRectangle.Top;
        maxBackground.TextureWidth = maxBackgroundRegion.Width;
        maxBackground.Width = 36f;
        maxBackground.WidthUnits = DimensionUnitType.Absolute;
        maxBackground.Dock(Gum.Wireframe.Dock.Right);
        innerContainer.AddChild(maxBackground);

        // Create the interactive track that responds to clicks
        // The special name "TrackInstance" is required for Slider functionality
        ContainerRuntime trackInstance = new ContainerRuntime();
        trackInstance.Name = "TrackInstance";
        trackInstance.Dock(Gum.Wireframe.Dock.Fill);
        trackInstance.Height = -2f;
        trackInstance.Width = -2f;
        middleBackground.AddChild(trackInstance);

        // Create the fill rectangle that visually displays the current value
        _fillRectangle = new ColoredRectangleRuntime();
        _fillRectangle.Dock(Gum.Wireframe.Dock.Left);
        _fillRectangle.Width = 90f; // Default to 90% - will be updated by value changes
        _fillRectangle.WidthUnits = DimensionUnitType.PercentageOfParent;
        trackInstance.AddChild(_fillRectangle);

        // Add "OFF" text to the left end
        TextRuntime offText = new TextRuntime();
        offText.Red = 70;
        offText.Green = 86;
        offText.Blue = 130;
        offText.CustomFontFile = @"fonts/04b_30.fnt";
        offText.FontScale = 0.25f;
        offText.UseCustomFont = true;
        offText.Text = "OFF";
        offText.Anchor(Gum.Wireframe.Anchor.Center);
        offBackground.AddChild(offText);

        // Add "MAX" text to the right end
        TextRuntime maxText = new TextRuntime();
        maxText.Red = 70;
        maxText.Green = 86;
        maxText.Blue = 130;
        maxText.CustomFontFile = @"fonts/04b_30.fnt";
        maxText.FontScale = 0.25f;
        maxText.UseCustomFont = true;
        maxText.Text = "MAX";
        maxText.Anchor(Gum.Wireframe.Anchor.Center);
        maxBackground.AddChild(maxText);

        // Define colors for focused and unfocused states
        Color focusedColor = Color.White;
        Color unfocusedColor = Color.Gray;

        // Create slider state category - Slider.SliderCategoryName is the required name
        StateSaveCategory sliderCategory = new StateSaveCategory();
        sliderCategory.Name = Slider.SliderCategoryName;
        topLevelContainer.AddCategory(sliderCategory);

        // Create the enabled (default/unfocused) state
        StateSave enabled = new StateSave();
        enabled.Name = FrameworkElement.EnabledStateName;
        enabled.Apply = () =>
        {
            // When enabled but not focused, use gray coloring for all elements
            background.Color = unfocusedColor;
            _textInstance.Color = unfocusedColor;
            offBackground.Color = unfocusedColor;
            middleBackground.Color = unfocusedColor;
            maxBackground.Color = unfocusedColor;
            _fillRectangle.Color = unfocusedColor;
        };
        sliderCategory.States.Add(enabled);

        // Create the focused state
        StateSave focused = new StateSave();
        focused.Name = FrameworkElement.FocusedStateName;
        focused.Apply = () =>
        {
            // When focused, use white coloring for all elements
            background.Color = focusedColor;
            _textInstance.Color = focusedColor;
            offBackground.Color = focusedColor;
            middleBackground.Color = focusedColor;
            maxBackground.Color = focusedColor;
            _fillRectangle.Color = focusedColor;
        };
        sliderCategory.States.Add(focused);

        // Create the highlighted+focused state by cloning the focused state
        StateSave highlightedFocused = focused.Clone();
        highlightedFocused.Name = FrameworkElement.HighlightedFocusedStateName;
        sliderCategory.States.Add(highlightedFocused);

        // Create the highlighted state by cloning the enabled state
        StateSave highlighted = enabled.Clone();
        highlighted.Name = FrameworkElement.HighlightedStateName;
        sliderCategory.States.Add(highlighted);

        // Assign the configured container as this slider's visual
        Visual = topLevelContainer;

        // Enable click-to-point functionality for the slider
        // This allows users to click anywhere on the track to jump to that value
        IsMoveToPointEnabled = true;

        // Add event handlers
        Visual.RollOn += HandleRollOn;
        ValueChanged += HandleValueChanged;
        ValueChangedByUi += HandleValueChangedByUi;
    }

    /// <summary>
    /// Automatically focuses the slider when the user interacts with it
    /// </summary>
    private void HandleValueChangedByUi(object sender, EventArgs e)
    {
        IsFocused = true;
    }

    /// <summary>
    /// Automatically focuses the slider when the mouse hovers over it
    /// </summary>
    private void HandleRollOn(object sender, EventArgs e)
    {
        IsFocused = true;
    }

    /// <summary>
    /// Updates the fill rectangle width to visually represent the current value
    /// </summary>
    private void HandleValueChanged(object sender, EventArgs e)
    {
        // Calculate the ratio of the current value within its range
        double ratio = (Value - Minimum) / (Maximum - Minimum);

        // Update the fill rectangle width as a percentage
        // _fillRectangle uses percentage width units, so we multiply by 100
        _fillRectangle.Width = 100 * (float)ratio;
    }
}
