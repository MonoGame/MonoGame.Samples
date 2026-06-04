///-------------------------------------------------------------------------------------------------
/// <remarks>   
///     Charles Humphrey, 12/07/2025. 
///     Fixes for DX implementation.
/// </remarks>
///-------------------------------------------------------------------------------------------------
#include "PPVertexShader.fxh"
#include "Macros.hlsl"

DECLARE_TEXTURE(TextureSampler, 0)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float BloomThreshold;

///-------------------------------------------------------------------------------------------------
/// <summary>   Function now uses correct vertex input structure for both OGL and DX </summary>
///
/// <remarks>   Charles Humphrey, 12/07/2025. </remarks>
///-------------------------------------------------------------------------------------------------
float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
    float4 c = SAMPLE_TEXTURE(TextureSampler, input.TexCoord);
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}

technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
