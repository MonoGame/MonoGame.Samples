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
struct LightVertexShaderOutput  
{  
   float4 Position : POSITION0;  
   float4 Color : COLOR0;  
   float2 TextureCoordinates : TEXCOORD0;  
   float3 ScreenData : TEXCOORD1;
};

float LightBrightness;  
float LightSharpness;  


LightVertexShaderOutput LightVS(VertexShaderInput input)
{
    LightVertexShaderOutput output;

    VertexShaderOutput mainVsOutput = MainVS(input);

    // forward along the existing values from the MainVS's output
    output.Position = mainVsOutput.Position;// / mainVsOutput.Position.w;
    output.Color = mainVsOutput.Color;
    output.TextureCoordinates = mainVsOutput.TextureCoordinates;

	// pack the required position variables, x, y, and w, into the ScreenData
	output.ScreenData.xy = output.Position.xy;
	output.ScreenData.z = output.Position.w;
	
    return output;
}


float4 MainPS(LightVertexShaderOutput input) : COLOR {
    float dist = length(input.TextureCoordinates - .5);   
    float range = 5; // arbitrary maximum. 

    float falloff = saturate(.5 - dist) * (LightBrightness * range + 1);
    falloff = pow(abs(falloff), LightSharpness * range + 1);

	// correct the perspective divide. 
	input.ScreenData /= input.ScreenData.z;

	// put the clip-space coordinates into screen space.
	float2 screenCoords = .5*(input.ScreenData.xy + 1);
	screenCoords.y = 1 - screenCoords.y;

    float4 normal = tex2D(NormalBufferSampler,screenCoords);
	// flip the y of the normals, because the art assets have them backwards.
    normal.y = 1 - normal.y;

    // convert from [0,1] to [-1,1]
    float3 normalDir = (normal.xyz-.5)*2;

    // find the direction the light is travelling at the current pixel
    float3 lightDir = float3(normalize(.5 - input.TextureCoordinates), 1);

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