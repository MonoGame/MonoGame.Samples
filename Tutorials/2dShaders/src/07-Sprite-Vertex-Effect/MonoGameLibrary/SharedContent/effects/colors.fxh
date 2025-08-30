#ifndef COLORS
#define COLORS

#include "common.fxh"

// the custom color map passed to the Material.SetParameter()
Texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// a control variable to lerp between original color and swapped color
float OriginalAmount;
float Saturation;

float4 Grayscale(float4 color)
{
    // Calculate the grayscale value based on human perception of colors
    float grayscale = dot(color.rgb, float3(0.3, 0.59, 0.11));

    // create a grayscale color vector (same value for R, G, and B)
    float3 grayscaleColor = float3(grayscale, grayscale, grayscale);

    // Linear interpolation between he grayscale color and the original color's
    // rgb values based on the saturation parameter.
    float3 finalColor = lerp(grayscale, color.rgb, Saturation);

    // Return the final color with the original alpha value
    return float4(finalColor, color.a);
}

float4 SwapColors(float4 color)
{
	// produce the key location
	//  note the x-offset by half a texel solves rounding errors.
	float2 keyUv = float2(color.r , 0);
	
	// read the swap color value
	float4 swappedColor = tex2D(ColorMapSampler, keyUv) * color.a;
	
	// ignore the swap if the map does not have a value
	bool hasSwapColor = swappedColor.a > 0;
	if (!hasSwapColor)
	{
	    return color;
	}
	
	// return the result color
	return lerp(swappedColor, color, OriginalAmount);
}

float4 ColorSwapPS(VertexShaderOutput input) : COLOR
{
    // read the original color value
	float4 originalColor = tex2D(SpriteTextureSampler,input.TextureCoordinates);

    float4 swapped = SwapColors(originalColor);
    float4 saturated = Grayscale(swapped);
    
    return saturated;
}

#endif