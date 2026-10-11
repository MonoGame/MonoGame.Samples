// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "Macros.hlsl"
#include "ShadowMap.hlsl"

#define EDITOR 1

BEGIN_CONSTANTS

float4x4 WorldViewProj;
float4x4 World;
float4x4 View;
float3 LightAmbient;
float3 CameraPosition; // World Space!
float4 ClusterInfo;

// Development features.
#if EDITOR
bool ShowDiffuse = false;
bool ShowNormals = false;
bool ShowRoughness = false;
bool ShowMetalness = false;
bool ShowOcclusion = false;
bool ShowLighting = false;
bool ShowDetailLighting = false;
#endif

END_CONSTANTS


DECLARE_TEXTURE(Texture, 0)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
    AddressW = wrap;
};

DECLARE_TEXTURE(TextureSamplerClamp, 1)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
};

DECLARE_TEXTURE(Bump0, 2)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
    AddressW = wrap;
};

DECLARE_TEXTURE(SpecularOrRMA0, 3)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
    AddressW = wrap;
};

DECLARE_TEXTURE(Emissive0, 4)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
    AddressW = wrap;
};

DECLARE_CUBEMAP(Reflect, 5)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = wrap;
    AddressV = wrap;
    AddressW = wrap;
};

DECLARE_TEXTURE_FORMAT(ClusterTexture, half2, 6)
{
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

DECLARE_TEXTURE_FORMAT(LightListTexture, half2, 7)
{
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

DECLARE_TEXTURE(LightInfoTexture, 8)
{
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

struct VS_INPUT_NM
{
    float4 InPosition : POSITION;
    float2 InTexCoord : TEXCOORD0;
    float3 InNormal : NORMAL;
    float3 InBinormal : BINORMAL;
    float3 InTangent : TANGENT;
};

struct VS_OUTPUT_NM
{
    float4 Position : SV_POSITION0;

    float2 TexCoord : TEXCOORD0;
    float3 ReflectDir : TEXCOORD1;
    float3 WPosition : TEXCOORD2;
    float ViewZ : TEXCOORD3;
    
    // TODO: MojoShader does not like TANGENT, NORMAL, BINORMAL
    // in the input to the pixel shader for some reason and will 
    // produce non-compiling glsl code.
    //
    // We need to fix this for 3.8.5.
    //    
   
    float3 Tangent : TEXCOORD4;
    float3 Binormal : TEXCOORD5;
    float3 Normal : TEXCOORD6;
};


VS_OUTPUT_NM Default_VS(VS_INPUT_NM input)
{
    VS_OUTPUT_NM Output;

    Output.Position = mul(input.InPosition, WorldViewProj);

    // Pass the world space position of the mesh.
    Output.WPosition = mul(float4(input.InPosition.xyz, 1), World).xyz;
    
    Output.ViewZ = abs(mul(float4(Output.WPosition.xyz, 1), View).z);
    
    Output.TexCoord = input.InTexCoord;

    Output.Tangent = normalize(mul(input.InTangent, (float3x3) World));
    Output.Normal = normalize(mul(input.InNormal, (float3x3) World));
    Output.Binormal = normalize(mul(input.InBinormal, (float3x3) World));

    Output.ReflectDir = reflect(Output.WPosition.xyz - CameraPosition, Output.Normal);

    return Output;
}


#define MAX_LIGHTS 512.0
#define LIGHT_Y_OFFSET (1.0 / MAX_LIGHTS)

float3 ProcessLight(int index, float4 normal, float3 viewDir, float3 position, float3x3 tangentSpace, float4 diffuse, float4 specular, float4 reflect)
{   
    float lightIndex = ((float)index + 0.5) * LIGHT_Y_OFFSET;    
    float4 lightPositionAndRadius   = SAMPLE_TEXTURE(LightInfoTexture, float2(0.0, lightIndex));
    float4 lightColorAndIntensity   = SAMPLE_TEXTURE(LightInfoTexture, float2(0.5, lightIndex));
    float4 lightShadowLookup        = SAMPLE_TEXTURE(LightInfoTexture, float2(1.0, lightIndex));
    
    float3 color = 0;
    
    float3 lightPosition = lightPositionAndRadius.xyz;   
    float oneOverLightRadius = lightPositionAndRadius.w;
    float3 lightColor = lightColorAndIntensity.xyz * lightColorAndIntensity.w;
    
    float3 shadowIndex = float3(
        lightShadowLookup.x * SHADOW_FACE_MIN_SIZE,
        lightShadowLookup.y * SHADOW_FACE_MIN_SIZE,
        lightShadowLookup.z * SHADOW_FACE_MIN_SIZE);
    
    float3 lightDir = lightPosition - position;
    
    float3 l = normalize(lightDir);
    float3 h = normalize(l + viewDir);
    float ndotl = saturate(dot(normal.xyz, l));
    float ndoth = saturate(dot(normal.xyz, h));
    if (ndotl <= 0)    
        return float3(0, 0, 0);
    
    float atten = 1.0 - saturate(length(lightDir) * oneOverLightRadius);
    
    specular.xyz *= lightColor * pow(ndoth, specular.w * 255) * atten;
    diffuse.xyz *= (lightColor * ndotl) * atten;
    reflect *= 1 - normal.w;
    
    color.xyz = diffuse.xyz + specular.xyz + reflect.xyz;
    
    float3 debugColor;
    
    // Sample the shadow.
    float shadow = Shadow_Sample(shadowIndex, position, lightPosition, oneOverLightRadius, debugColor);
   
    color.xyz = lerp(color.xyz, float3(0, 0, 0), shadow);
    
    return color;
}


#define PI 3.14159265358979323846

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(float3 N, float3 H, float alpha)
{
    float NoH = saturate(dot(N, H));
    float a2 = alpha * alpha;
    float d = (NoH * NoH) * (a2 - 1.0) + 1.0;
    return a2 / (PI * d * d);
}

float GeometrySchlickGGX(float NoV, float alpha)
{
    float k = (alpha + 1.0) * (alpha + 1.0) / 8.0;
    return NoV / (NoV * (1.0 - k) + k);
}

float GeometrySmith(float3 N, float3 V, float3 L, float alpha)
{
    float NoV = saturate(dot(N, V));
    float NoL = saturate(dot(N, L));
    float ggx1 = GeometrySchlickGGX(NoV, alpha);
    float ggx2 = GeometrySchlickGGX(NoL, alpha);
    return ggx1 * ggx2;
}

float3 ProcessLight_RMA(int index, float4 normal, float3 viewDir, float3 position, float3x3 tangentSpace, float4 diffuse, float4 rma, float4 reflect)
{
    float lightIndex = ((float) index + 0.5) * LIGHT_Y_OFFSET;
    float4 lightPositionAndRadius = SAMPLE_TEXTURE(LightInfoTexture, float2(0.0, lightIndex));
    float4 lightColorAndIntensity = SAMPLE_TEXTURE(LightInfoTexture, float2(0.5, lightIndex));
    float4 lightShadowLookup = SAMPLE_TEXTURE(LightInfoTexture, float2(1.0, lightIndex));
    
    float3 color = 0;
    
    float3 lightPosition = lightPositionAndRadius.xyz;
    float oneOverLightRadius = lightPositionAndRadius.w;
    float3 lightColor = lightColorAndIntensity.xyz;
    
    float3 shadowIndex = float3(
        lightShadowLookup.x * SHADOW_FACE_MIN_SIZE,
        lightShadowLookup.y * SHADOW_FACE_MIN_SIZE,
        lightShadowLookup.z * SHADOW_FACE_MIN_SIZE);
    
    float3 lightDir = lightPosition - position;
    
    float ndotl = max(dot(normal.xyz, normalize(lightDir)), 0.0);
    if (ndotl < 0.0001)    
        return float3(0, 0, 0);
    
    float3 l = normalize(lightDir);
    float3 h = normalize(l + viewDir);
    float vdoth = saturate(dot(viewDir, h));
    
    float3 ndotv = max(dot(normal.xyz, viewDir), 0.0);
    
    float dist = length(lightDir);
    float atten = 1.0 - saturate(dist * oneOverLightRadius);
    //atten *= atten; // smoother falloff
    
    float lightIntensity = lightColorAndIntensity.w / max(0.001, 4.0 * PI * dist * dist);
    
    float roughness = rma.y;
    float metallic = rma.z;
    
    float3 term = float3(0.04, 0.04, 0.04); // ???
    
    float3 f0 = lerp(term, diffuse.xyz, metallic);

    float3 F = FresnelSchlick(vdoth, f0);
    float D = DistributionGGX(normal.xyz, h, roughness);
    float G = GeometrySmith(normal.xyz, viewDir, l, roughness);
    
    float3 numerator = D * G * F;
    float denominator = 4.0 * ndotv * ndotl + 0.001;
    float3 specular = numerator / denominator;
    
    float3 kD = (1.0 - F) * (1.0 - metallic); // metals have no diffuse
    
    diffuse.xyz = kD * diffuse.xyz / PI;
    
    float3 radiance = lightColor * lightIntensity * atten;
    color.xyz = (diffuse.xyz + specular.xyz) * radiance * ndotl;
    
    float3 debugColor;
    
    // Sample the shadow.
    float shadow = Shadow_Sample(shadowIndex, position, lightPosition, oneOverLightRadius, debugColor);
   
    color.xyz = lerp(color.xyz, float3(0, 0, 0), shadow);
    
    return color;
}


float GetLuminance(float3 color)
{
    return dot(color, float3(0.2126, 0.7152, 0.0722));
}

#define CLUSTERS_X  16
#define CLUSTERS_Y  8
#define CLUSTERS_Z  24
#define MAX_CLUSTERS (CLUSTERS_X * CLUSTERS_Y * CLUSTERS_Z)
#define MAX_LIGHTS_PER_CLUSTER  16

#define TO_INT2(x) int2(x * 65535.0f + 0.5f)

int3 Light_GetClusterIndex3(float4 svPosition, float viewZ)
{
    int clusterX = min((int)(svPosition.x * ClusterInfo.x), CLUSTERS_X - 1);
    int clusterY = min((int)(svPosition.y * ClusterInfo.y), CLUSTERS_Y - 1);

    float zNorm = saturate((viewZ - ClusterInfo.z) * ClusterInfo.w);
    int clusterZ = min((int)(zNorm * CLUSTERS_Z), CLUSTERS_Z - 1);
    
    return int3(clusterX, clusterY, clusterZ);
}

int Light_GetClusterIndex(float4 svPosition, float viewZ)
{
    int3 index = Light_GetClusterIndex3(svPosition, viewZ);
   
    return index.x + (index.y * CLUSTERS_X) + (index.z * CLUSTERS_X * CLUSTERS_Y);
}

float4 NormalMapping_PS(VS_OUTPUT_NM input) : SV_TARGET0
{
    float4 diffuse = SAMPLE_TEXTURE(Texture, input.TexCoord);   
    float4 specular = SAMPLE_TEXTURE(SpecularOrRMA0, input.TexCoord);
    float4 normal = SAMPLE_TEXTURE(Bump0, input.TexCoord);
    float4 glow = SAMPLE_TEXTURE(Emissive0, input.TexCoord);
    float4 reflect = SAMPLE_CUBEMAP(Reflect, input.ReflectDir);
    
    float3x3 tangent_space = float3x3(  normalize(input.Tangent),
                                        normalize(input.Binormal),
                                        normalize(input.Normal));

    // Development stuff.
#if EDITOR    
    if (ShowLighting)
        normal.rgb = float3(0.5, 0.5, 1);
    if (ShowLighting || ShowDetailLighting)
        diffuse = float4(1, 1, 1, diffuse.a);
#endif
    
    float3 tn = normalize(normal.xyz * 2.0 - 1.0);
    
    float4 n = float4(mul(tn, tangent_space), normal.w);
    float3 v = normalize(CameraPosition - input.WPosition.xyz);
    
    // Development stuff.
#if EDITOR
    if (ShowDiffuse)
        return float4(diffuse.rgb, 0);
    if (ShowNormals)
        return float4(normal.xyz, 0);
#endif
    
    // Prepare the final color.
    float4 color;
    color.rgb = diffuse.xyz * LightAmbient;
    
    // Look up the cluster for this pixel.
    int clusterIndex = Light_GetClusterIndex(input.Position, input.ViewZ);      
    int2 cluster = TO_INT2(LOAD_TEXTURE(ClusterTexture, int3(clusterIndex, 0, 0)).xy);
    
    // Accumulate the lighting.
    for (int i = 0; i < MAX_LIGHTS_PER_CLUSTER; i++)
    {
        if (i >= cluster.x)
            break;
        
        int2 lightList = TO_INT2(LOAD_TEXTURE(LightListTexture, int3(i, cluster.y, 0)).xy);
        color.rgb += ProcessLight(lightList.x, n, v, input.WPosition, tangent_space, diffuse, specular, reflect);
    }
    
    // Apply the emissive elements of the material.
    color.xyz += glow.xyz * 5.5;
    
    color.w = GetLuminance(color.rgb) * 0.5;
    
    return color;
}

Technique NormalMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL Default_VS();
        PixelShader = compile PS_SHADERMODEL NormalMapping_PS();
    }
}


float4 RMA_PS(VS_OUTPUT_NM input) : SV_TARGET0
{
    // RMA texture is:
    // R - Ambient occulusion
    // G - Roughness
    // B - Metallic
    // 
    
    float4 diffuse = SAMPLE_TEXTURE(Texture, input.TexCoord);
    float4 rma = SAMPLE_TEXTURE(SpecularOrRMA0, input.TexCoord);
    float4 normal = SAMPLE_TEXTURE(Bump0, input.TexCoord);
    float4 glow = SAMPLE_TEXTURE(Emissive0, input.TexCoord);
    float4 reflect = float4(1, 1, 1, 1); //SAMPLE_CUBEMAP(Reflect, input.ReflectDir);
    
    float3x3 tangent_space = float3x3(  normalize(input.Tangent),
                                        normalize(input.Binormal),
                                        normalize(input.Normal));

    // Development stuff.
#if EDITOR    
    if (ShowLighting)
        normal.rgb = float3(0.5, 0.5, 1);
    if (ShowLighting || ShowDetailLighting)
        diffuse = float4(1, 1, 1, diffuse.a);
#endif
    
    float3 tn = normalize(normal.xyz * 2.0 - 1.0);
    
    float4 n = float4(normalize(mul(tn, tangent_space)), normal.w);
    float3 v = normalize(CameraPosition - input.WPosition.xyz);
    
    // Development stuff.
#if EDITOR     
    if (ShowDiffuse)
        return float4(diffuse.rgb, 0);
    if (ShowNormals)
        return float4(normal.xyz, 0);
    if (ShowRoughness)
        return float4(rma.ggg, 0);
    if (ShowMetalness)
        return float4(rma.bbb, 0);
    if (ShowOcclusion)
        return float4(rma.rrr, 0);
#endif
    
    float4 color;
    color.rgb = diffuse.xyz * LightAmbient * rma.r;
    
    // Look up the cluster for this pixel.
    int clusterIndex = Light_GetClusterIndex(input.Position, input.ViewZ);
    int2 cluster = TO_INT2(LOAD_TEXTURE(ClusterTexture, int3(clusterIndex, 0, 0)).xy);
    
    // Accumulate the lighting.
    for (int i = 0; i < MAX_LIGHTS_PER_CLUSTER; i++)
    {
        if (i >= cluster.x)
            break;
        
        int2 lightList = TO_INT2(LOAD_TEXTURE(LightListTexture, int3(i, cluster.y, 0)).xy);
        color.rgb += ProcessLight_RMA(lightList.x, n, v, input.WPosition, tangent_space, diffuse, rma, reflect);
    }
    
    // Apply the emissive elements of the material.
    color.xyz += glow.xyz * 5.5;
    
    color.w = GetLuminance(color.rgb) * 0.5;
    
    return color;
}

Technique RMA
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL Default_VS();
        PixelShader = compile PS_SHADERMODEL RMA_PS();
    }
}