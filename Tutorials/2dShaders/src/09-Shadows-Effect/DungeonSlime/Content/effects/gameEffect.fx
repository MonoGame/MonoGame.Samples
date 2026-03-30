#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

Texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
};

#include "../../../MonoGameLibrary/SharedContent/effects/3dEffect.fxh"
#include "../../../MonoGameLibrary/SharedContent/effects/colors.fxh"

struct PixelShaderOutput {
    float4 color: COLOR0;
    float4 normal: COLOR1;
};

PixelShaderOutput MainPS(VertexShaderOutput input)
{
    PixelShaderOutput output;
    output.color = ColorSwapPS(input);
        
    // do not even render the pixel if the alpha is blank.
    clip(output.color.a - 1);
    
    // read the normal data from the NormalMap
    float4 normal = tex2D(NormalMapSampler,input.TextureCoordinates);
    output.normal = normal;

    return output;
}


technique SpriteDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};