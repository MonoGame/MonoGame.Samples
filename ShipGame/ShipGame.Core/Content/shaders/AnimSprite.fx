//////////////////////////////////////////////////////////////////////
//                                                                  //
// Shader altered to compile on both OpenGL projects and DirectX.   //
// C.Humphrey  2024-02-19                                           //
//                                                                  //
//////////////////////////////////////////////////////////////////////

#include "Macros.hlsl"

BEGIN_CONSTANTS
float4x4 ViewProj;
float4 FrameOffset;
float2 FrameSize;
float2 FrameBlend;
END_CONSTANTS

DECLARE_TEXTURE(Texture, 0)
{
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_INPUT
{
    float4 InPosition : SV_POSITION;
    float2 InTexCoord : TEXCOORD0;    
};

struct VS_OUTPUT
{
    float4 OutPosition : SV_POSITION;
    float2 OutTexCoord : TEXCOORD0;
};

struct PS_INPUT
{
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT AnimSpriteVS(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.OutPosition = mul(input.InPosition, ViewProj);
    output.OutTexCoord = input.InTexCoord;
        
    return output;
 }

float4 AnimSpritePS(VS_OUTPUT input) : SV_TARGET0
{
    float2 tx1 = FrameSize * (FrameOffset.xy + input.OutTexCoord);
    float2 tx2 = FrameSize * (FrameOffset.zw + input.OutTexCoord);
    
    float4 color1 = SAMPLE_TEXTURE(Texture, tx1);
    float4 color2 = SAMPLE_TEXTURE(Texture, tx2);
    
    float4 blend_color = lerp(color1, color2, FrameBlend.x);
    blend_color.w *= FrameBlend.y;
    
    return blend_color;

}

Technique AnimSprite
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL AnimSpriteVS();
        PixelShader = compile PS_SHADERMODEL AnimSpritePS();
    }
}

