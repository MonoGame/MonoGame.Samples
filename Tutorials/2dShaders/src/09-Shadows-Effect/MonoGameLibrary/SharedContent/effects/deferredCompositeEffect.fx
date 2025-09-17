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

float4 Blur(float2 texCoord)
{
	float4 color = float4(0, 0, 0, 0);

	float2 texelSize = 1 / float2(1280,720);
	int k=1;
    // Offsets for a 3x3 kernel
    for (int x = -k; x <= k; x++)
    {
        for (int y = -k; y <= k; y++)
        {
            float2 offset = float2(x, y) * texelSize * 8;
            color += tex2D(LightBufferSampler, texCoord + offset);
        }
    }

    // Average the 9 samples
    color /= pow(k*2+1, 2);
	color.a = 1;
    return color;
}

float AmbientLight;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;

	float4 light2 = Blur(input.TextureCoordinates) * input.Color;
	//float4 light = tex2D(LightBufferSampler,input.TextureCoordinates) * input.Color;
	float4 light = light2;//

    float3 toneMapped = light.xyz / (.25 + dot(light.xyz, float3(0.299, 0.587, 0.114)));
    light.xyz = toneMapped;
    
    light = saturate(light + AmbientLight);
    
    // return color*.001 + light;
	return color * light;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};