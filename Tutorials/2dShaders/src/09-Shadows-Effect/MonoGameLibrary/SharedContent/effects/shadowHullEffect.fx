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

#include "3dEffect.fxh"

float2 UnpackVector2FromColor_SNorm(float4 color)
{
    // Convert [0,1] to byte range [0,255]
    float4 bytes = color * 255.0;

    // Reconstruct 16-bit unsigned ints (x and y)
    float xInt = bytes.r * 256.0 + bytes.g;
    float yInt = bytes.b * 256.0 + bytes.a;

    // Convert from unsigned to signed short range [-32768, 32767]
    if (xInt >= 32768.0) xInt -= 65536.0;
    if (yInt >= 32768.0) yInt -= 65536.0;

    // Convert from signed 16-bit to float in [-1, 1]
    float x = xInt / 32767.0;
    float y = yInt / 32767.0;

    return float2(x, y);
}

float2 LightPosition;
VertexShaderOutput ShadowHullVS(VertexShaderInput input) 
{   
    VertexShaderInput modified = input;
    float distance = ScreenSize.x + ScreenSize.y;
    float2 pos = input.Position.xy;
    
    float2 P = pos - (.5 * input.TexCoord) / ScreenSize;
    float2 A = P;
    
    float2 aToB = UnpackVector2FromColor_SNorm(input.Color) * ScreenSize;
    float2 B = A + aToB;

    // expand the segment by 1 unit in each direction
    float2 direction = normalize(aToB);
    A -= direction;
    B += direction;
    
    // cull faces
    float2 normal = float2(-direction.y, direction.x);
    float alignment = dot(normal, (LightPosition - A));
    if (alignment < 0){
        modified.Color.a = -1;
    }

    float2 lightRayA = normalize(A - LightPosition);
    float2 a = A + distance * lightRayA;
    float2 lightRayB = normalize(B - LightPosition);
    float2 b = B + distance * lightRayB;    
    
    int id = input.TexCoord.x + input.TexCoord.y * 2;
    if (id == 0) {        // S --> A
    	pos = A;
    } else if (id == 1) { // D --> a
    	pos = a;
    } else if (id == 3) { // F --> b
    	pos = b;
    } else if (id == 2) { // G --> B
    	pos = B;
    }
    
    modified.Position.xy = pos;
    VertexShaderOutput output = MainVS(modified);
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    clip(input.Color.a);
    return float4(0,0,0,1); // return black
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
		VertexShader = compile VS_SHADERMODEL ShadowHullVS();
	}
};