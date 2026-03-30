using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class RedColorMap
{
    public Texture2D ColorMap { get; set; }

    public RedColorMap()
    {
        ColorMap = new Texture2D(Core.GraphicsDevice, 256, 1, false, SurfaceFormat.Color);
    }

    /// <summary>
    /// Given a dictionary of red-color values (0 to 255) to swapColors,
    /// Set the values of the <see cref="ColorMap"/> so that it can be used
    /// As the ColorMap parameter in the colorSwapEffect.
    /// </summary>
    public void SetColorsByRedValue(Dictionary<int, Color> map, bool overWrite = true)
    {
        var pixelData = new Color[ColorMap.Width];
        ColorMap.GetData(pixelData);

        for (var i = 0; i < pixelData.Length; i++)
        {
            // if the given color dictionary contains a color value for this red index, use it.
            if (map.TryGetValue(i, out var swapColor))
            {
                pixelData[i] = swapColor;
            }
            else if (overWrite)
            {
                // otherwise, default the pixel to transparent
                pixelData[i] = Color.Transparent;
            }
        }
        
        ColorMap.SetData(pixelData);
    }

    public void SetColorsByExistingColorMap(Texture2D existingColorMap)
    {
        var existingPixels = new Color[256];
        existingColorMap.GetData(existingPixels);

        var map = new Dictionary<int, Color>();
        for (var i = 0; i < existingPixels.Length; i++)
        {
            map[i] = existingPixels[i];
        }
        
        SetColorsByRedValue(map);
    }
}