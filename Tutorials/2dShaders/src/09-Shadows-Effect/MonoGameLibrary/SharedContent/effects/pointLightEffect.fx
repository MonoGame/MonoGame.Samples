#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D NormalBuffer;
sampler2D NormalBufferSampler = sampler_state
{
	Texture = <NormalBuffer>;
};

#include "3dEffect.fxh"

float LightBrightness;
float LightSharpness;

struct LightVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
	float2 ScreenCoordinates : TEXCOORD1;
};


LightVertexShaderOutput LightVS(VertexShaderInput input)
{
    LightVertexShaderOutput output;

    VertexShaderOutput mainVsOutput = MainVS(input);

    // forward along the existing values from the MainVS's output
    output.Position = mainVsOutput.Position;
    output.Color = mainVsOutput.Color;
    output.TextureCoordinates = mainVsOutput.TextureCoordinates;
    
    // normalize the clip-space position
    float4 normalized = output.Position / output.Position.w;
    
    // normalize from -1,1 to 0,1
    output.ScreenCoordinates = .5 * (float2(normalized.xy) + 1);
    
    // invert the y coordinate, because MonoGame flips it. 
    output.ScreenCoordinates.y = 1 - output.ScreenCoordinates.y;
    
    return output;
}
float4 MainPS(LightVertexShaderOutput input) : COLOR
{
    float dist = length(input.TextureCoordinates - .5);   
  
    float range = 5; // arbitrary maximum. 
  
    float falloff = saturate(.5 - dist) * (LightBrightness * range + 1);
    falloff = pow(abs(falloff), LightSharpness * range + 1);
   
    float4 normal = tex2D(NormalBufferSampler,input.ScreenCoordinates);
    // flip the y of the normals, because the art assets have them backwards.
    normal.y = 1 - normal.y;

    // convert from [0,1] to [-1,1]
    float3 normalDir = (normal.xyz-.5)*2;
    
    // find the direction the light is travelling at the current pixel
    float3 lightDir = normalize(float3(.5 - input.TextureCoordinates, 1));
    
    // how much is the normal direction pointing towards the light direction?
    float lightAmount = (dot(normalDir, lightDir));
   
    float4 color = input.Color;
    color.a *= falloff * lightAmount;
    return color;
}

technique SpriteDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL LightVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};