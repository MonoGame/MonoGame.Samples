float4x4 g_WorldViewProj;

texture g_ColorMap:TEXUNIT0;

float4 g_Color;
float2 g_PixelSize;

#define BLUR_RANGE 5

sampler ColorSampler = 
sampler_state
{
    Texture = <g_ColorMap>;
    MipFilter = NONE;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT MainVS( 
    float4 Pos      : POSITION, 
    float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Output;

    Output.Position = mul( Pos, g_WorldViewProj );
    Output.TexCoord = TexCoord;
    
    return Output;
}

float4 ColorPS() : COLOR
{
    return g_Color;
}

float4 ColorTexturePS(float2 TexCoord : TEXCOORD0) : COLOR
{
    return g_Color * tex2D(ColorSampler,TexCoord + g_PixelSize);
}

float4 BlurHorizontalPS(float2 TexCoord : TEXCOORD0) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = TexCoord + float2(i*g_PixelSize.x, 0);
        
        float4 c = tex2D(ColorSampler, tc);
        
        c.xyz *= c.w;
        
        color += c;
    }
    return color/(2*BLUR_RANGE+1);
}

float4 BlurHorizontalSplitPS(float2 TexCoord : TEXCOORD0) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = TexCoord + float2(i*g_PixelSize.x, 0);
        float4 c = tex2D(ColorSampler, tc);
        
        c.xyz *= c.w;
        c.w = 1;
        
        const float split = 0.499;
        if (TexCoord.x >= split)
        {
            if (tc.x >= split)
                color += c;
        }
        else
            if (tc.x < split)
                color += c;
    }

    return color/color.w;
}

float4 BlurVerticalPS(float2 TexCoord : TEXCOORD0) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = TexCoord + float2(0, i*g_PixelSize.y);
        float4 c = tex2D(ColorSampler, tc);
        color += c;
    }
    return color/(2*BLUR_RANGE+1);
}

technique Color
{
    pass P0
    {          
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS( );
        PixelShader  = compile ps_4_0_level_9_1 ColorPS( );
#else
        VertexShader = compile vs_3_0 MainVS( );
        PixelShader  = compile ps_3_0 ColorPS( );
#endif
    }
}

technique ColorTexture
{
    pass P0
    {          
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS( );
        PixelShader  = compile ps_4_0_level_9_1 ColorTexturePS( );
#else
        VertexShader = compile vs_3_0 MainVS( );
        PixelShader  = compile ps_3_0 ColorTexturePS( );
#endif
    }
}

technique BlurHorizontal
{
    pass P0
    {          
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS( );
        PixelShader  = compile ps_4_0_level_9_1 BlurHorizontalPS( );
#else
        VertexShader = compile vs_3_0 MainVS( );
        PixelShader  = compile ps_3_0 BlurHorizontalPS( );
#endif
    }
}

technique BlurVertical
{
    pass P0
    {          
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS( );
        PixelShader  = compile ps_4_0_level_9_1 BlurVerticalPS( );
#else
        VertexShader = compile vs_3_0 MainVS( );
        PixelShader  = compile ps_3_0 BlurVerticalPS( );
#endif
    }
}

technique BlurHorizontalSplit
{
    pass P0
    {          
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS( );
        PixelShader  = compile ps_4_0_level_9_1 BlurHorizontalSplitPS( );
#else
        VertexShader = compile vs_3_0 MainVS( );
        PixelShader  = compile ps_3_0 BlurHorizontalSplitPS( );
#endif
    }
}
