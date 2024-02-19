#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 ViewProj;
float4 FrameOffset;
float2 FrameSize;
float2 FrameBlend;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

void AnimSpriteVS( 
     in float4 InPosition     : SV_POSITION,
     in float2 InTexCoord     : TEXCOORD0,
    out float4 OutPosition    : SV_POSITION,
    out float2 OutTexCoord    : TEXCOORD0)
{
    OutPosition = mul(InPosition, ViewProj);
    OutTexCoord = InTexCoord;
}

float4 AnimSpritePS( in float2 TexCoord : TEXCOORD0 ) : COLOR0
{
    float2 tx1 = FrameSize * (FrameOffset.xy + TexCoord);
    float2 tx2 = FrameSize * (FrameOffset.zw + TexCoord);
    
    float4 color1 = tex2D(TextureSampler, tx1);
    float4 color2 = tex2D(TextureSampler, tx2);
    
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

