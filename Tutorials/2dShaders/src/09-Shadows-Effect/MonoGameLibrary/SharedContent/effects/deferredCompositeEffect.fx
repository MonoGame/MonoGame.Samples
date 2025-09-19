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


Texture2D LightBuffer;
sampler2D LightBufferSampler = sampler_state
{
	Texture = <LightBuffer>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float AmbientLight;
float2 ScreenSize;
float BoxBlurStride;

float4 Blur(float2 texCoord)
{
	float4 color = float4(0, 0, 0, 0);

	float2 texelSize = 1 / ScreenSize;
	int kernalSize = 1;
	float stride = BoxBlurStride * 30; // allow the stride to range up a size of 30
    for (int x = -kernalSize; x <= kernalSize; x++)
    {
        for (int y = -kernalSize; y <= kernalSize; y++)
        {

            float2 offset = float2(x, y) * texelSize * stride;
            color += tex2D(LightBufferSampler, texCoord + offset);
        }
    }

	int totalSamples = pow(kernalSize*2+1, 2);
    color /= totalSamples;
	color.a = 1;
    return color;
}


float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;
	float4 light = Blur(input.TextureCoordinates) * input.Color;

    float3 toneMapped = light.xyz / (.5 + dot(light.xyz, float3(0.299, 0.587, 0.114)));
    light.xyz = toneMapped;
    
    light = saturate(light + AmbientLight);
	return color * light;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};