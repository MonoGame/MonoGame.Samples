#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProj;
float4 LightPosition;
float3 LightColor;
float3 LightAmbient;
float3 CameraPosition;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};
sampler2D TextureSamplerClamp = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
};

texture2D Bump0;
sampler2D NormalSampler = sampler_state
{
    Texture = <Bump0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Specular0;
sampler2D SpecularSampler = sampler_state
{
    Texture = <Specular0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Emissive0;
sampler2D GlowMapSampler = sampler_state
{
    Texture = <Emissive0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

textureCUBE Reflect;
samplerCUBE ReflectSampler = sampler_state
{
    Texture = <Reflect>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

void PlainMappingVS( 
     in float4 InPosition    : SV_POSITION,
     in float2 InTexCoord    : TEXCOORD0,
    out float4 OutPosition    : SV_POSITION,
    out float2 OutTexCoord    : TEXCOORD0 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    OutTexCoord = InTexCoord;
}

float4 PlainMappingPS( in float2 TexCoord : TEXCOORD0 ) : COLOR0
{
    return tex2D(TextureSampler, TexCoord);
}

void NormalMappingVS( 
     in float4 InPosition    : SV_POSITION,
     in float2 InTexCoord    : TEXCOORD0,
     in float3 InNormal      : NORMAL0,  
     in float3 InBinormal    : BINORMAL0,
     in float3 InTangent     : TANGENT0,
    out float4 OutPosition   : SV_POSITION,
    out float2 OutTexCoord   : TEXCOORD0,
    out float3 OutLightDir   : TEXCOORD1,
    out float3 OutViewDir    : TEXCOORD2,
    out float3 OutReflectDir : TEXCOORD3 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    
    OutTexCoord = InTexCoord;

    float3x3 tangent_space = float3x3(InTangent, InBinormal, InNormal);
    
    OutLightDir = mul(tangent_space, LightPosition.xyz - InPosition.xyz);
   
    OutViewDir = mul(tangent_space, CameraPosition - InPosition.xyz);
    
    OutReflectDir = reflect(InPosition.xyz - CameraPosition, InNormal);
}

float4 NormalMappingPS(
    in float2 TexCoord        : TEXCOORD0,
    in float3 LightDir        : TEXCOORD1,
    in float3 ViewDir         : TEXCOORD2,
    in float3 ReflectDir      : TEXCOORD3 ) : COLOR0
{
    float4 diffuse = tex2D(TextureSampler, TexCoord);
    float4 specular = tex2D(SpecularSampler, TexCoord);
    float4 normal = tex2D(NormalSampler, TexCoord);
    float4 reflect = texCUBE(ReflectSampler, ReflectDir);
    float4 glow = tex2D(GlowMapSampler, TexCoord);
    
    float3 n = normalize(normal.xyz - 0.5);
    float3 l = normalize(LightDir);
    float3 v = normalize(ViewDir);
    float3 h = normalize(l+v);
    
    float ndotl = saturate(dot(n,l));
    float ndoth = saturate(dot(n,h));
    if (ndotl == 0)    ndoth = 0;
    
    float3 ambient = LightAmbient * diffuse.xyz;
    
    specular.xyz *= LightColor * pow(ndoth, specular.w * 255);
    diffuse.xyz *= LightColor * ndotl;
    reflect *= 1 - normal.w;

    float glow_intensity = saturate(dot(glow.xyz, 1.0) + dot(specular.xyz, 1.0));

    float4 color;
    color.xyz = ambient + glow.xyz + diffuse.xyz + specular.xyz + reflect.xyz;
    color.w = glow_intensity;
    
    return color;
}

void ViewMappingVS( 
     in float4 InPosition   : SV_POSITION,
     in float3 InNormal     : NORMAL0,  
    out float4 OutPosition  : SV_POSITION,
    out float  OutFacing    : TEXCOORD0 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    
    float3 view = normalize(CameraPosition - InPosition.xyz);
    
    OutFacing = saturate(dot(view, InNormal));
}

float4 ViewMappingPS(in float Facing : TEXCOORD0) : COLOR0
{
    Facing *= Facing;
    Facing *= Facing;
    
    float4 tex = tex2D(TextureSamplerClamp, float2(Facing, 0));
    tex.w = 1.0f;

    return tex;
}

Technique PlainMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL PlainMappingVS();
        PixelShader = compile PS_SHADERMODEL PlainMappingPS();
    }
}

Technique NormalMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL NormalMappingVS();
        PixelShader = compile PS_SHADERMODEL NormalMappingPS();
    }
}

Technique ViewMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL ViewMappingVS();
        PixelShader = compile PS_SHADERMODEL ViewMappingPS();
    }
}

