﻿//////////////////////////////////////////////////////////////////////
//                                                                  //
// Shader altered to compile on both OpenGL projects and DirectX.   //
// C.Humphrey  2024-02-19                                           //
//                                                                  //
// Updated pixel shaders to take the output structure of the        //
// vertex shaders for paramters. In DX the individual parameters    //
// were not getting set.                                            //
// C.Humphrey 2023-02-19                                            //
//                                                                  //
//////////////////////////////////////////////////////////////////////
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

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
    float4 Position : SV_POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT MainVS( 
    float4 Pos      : SV_POSITION0, 
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

float4 ColorTexturePS(VS_OUTPUT input): COLOR
{
    return g_Color * tex2D(ColorSampler,input.TexCoord + g_PixelSize);
}

float4 BlurHorizontalPS(VS_OUTPUT input) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = input.TexCoord + float2(i*g_PixelSize.x, 0);
        
        float4 c = tex2D(ColorSampler, tc);
        
        c.xyz *= c.w;
        
        color += c;
    }
    return color/(2*BLUR_RANGE+1);
}

float4 BlurHorizontalSplitPS(VS_OUTPUT input) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = input.TexCoord + float2(i*g_PixelSize.x, 0);
        float4 c = tex2D(ColorSampler, tc);
        
        c.xyz *= c.w;
        c.w = 1;
        
        const float split = 0.499;
        if (input.TexCoord.x >= split)
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

float4 BlurVerticalPS(VS_OUTPUT input) : COLOR
{
    float4 color = float4(0,0,0,0);
    for( float i=-BLUR_RANGE;i<=BLUR_RANGE;i++ )
    {
        float2 tc = input.TexCoord + float2(0, i*g_PixelSize.y);
        float4 c = tex2D(ColorSampler, tc);
        color += c;
    }
    return color/(2*BLUR_RANGE+1);
}

technique Color
{
    pass P0
    {          
        VertexShader = compile VS_SHADERMODEL MainVS( );
        PixelShader  = compile PS_SHADERMODEL ColorPS( );
    }
}

technique ColorTexture
{
    pass P0
    {          
        VertexShader = compile VS_SHADERMODEL MainVS( );
        PixelShader  = compile PS_SHADERMODEL ColorTexturePS( );
    }
}

technique BlurHorizontal
{
    pass P0
    {          
        VertexShader = compile VS_SHADERMODEL MainVS( );
        PixelShader  = compile PS_SHADERMODEL BlurHorizontalPS( );
    }
}

technique BlurVertical
{
    pass P0
    {          
        VertexShader = compile VS_SHADERMODEL MainVS( );
        PixelShader  = compile PS_SHADERMODEL BlurVerticalPS( );
    }
}

technique BlurHorizontalSplit
{
    pass P0
    {          
        VertexShader = compile VS_SHADERMODEL MainVS( );
        PixelShader  = compile PS_SHADERMODEL BlurVerticalPS( );
    }
}