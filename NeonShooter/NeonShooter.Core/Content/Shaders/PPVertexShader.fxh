///-------------------------------------------------------------------------------------------------
/// <summary>   
///     This header file is for used to ensure vertex patching in the DX build.
/// </summary>
///
/// <remarks>   Charles Humphrey, 12/07/2025. </remarks>
///-------------------------------------------------------------------------------------------------

struct VertexShaderOutput {
    float4 Position : SV_POSITION;
    float4 Color : TEXCOORD1;
    float2 TexCoord : TEXCOORD0;
};