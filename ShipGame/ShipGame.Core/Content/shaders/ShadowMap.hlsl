// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if !defined(SHADOWMAP_H)
#define SHADOWMAP_H

#include "Macros.hlsl"

#define SHADOW_ATLAS_SCALE (1.0 / 4096.0)
#define SHADOW_FACE_MIN_SIZE 32

DECLARE_TEXTURE(ShadowAtlasTexture, 9)
{
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

// Returns the cubemap face index for the normalized view direction.
int Shadow_GetFaceIndex(float3 dir)
{
    float3 absDir = abs(dir);
    
    if (absDir.x > absDir.y && absDir.x > absDir.z)
        return dir.x > 0 ? 0 : 1;
    
    if (absDir.y > absDir.z)
        return dir.y > 0 ? 2 : 3;
    
    return dir.z > 0 ? 4 : 5;
}

float3 Shadow_GetDebugFaceColors(int face)
{
    switch (face)
    {
        case 0:
            return float3(1, 0, 0);
        case 1:
            return float3(0.1, 0, 0);
        case 2:
            return float3(0, 1, 0);
        case 3:
            return float3(0, 0.1, 0);
        case 4:
            return float3(0, 0, 1);
        case 5:
            return float3(0, 0, 0.1);
    }
    
    return float3(1, 0, 1);
}

// Returns the normalized face UVs.
float2 Shadow_ProjectDirectionToFaceUV(int face, float3 normDir)
{
    float u = 0, v = 0;
    float3 absDir = abs(normDir);

    switch (face)
    {
        case 0: // +X
            u = normDir.z / absDir.x;
            v = -normDir.y / absDir.x;
            break;
        case 1: // -X
            u = -normDir.z / absDir.x;
            v = -normDir.y / absDir.x;
            break;
        case 2: // +Y
            u = normDir.x / absDir.y;
            v = -normDir.z / absDir.y;
            break;
        case 3: // -Y
            u = normDir.x / absDir.y;
            v = normDir.z / absDir.y;
            break;
        case 4: // +Z
            u = -normDir.x / absDir.z;
            v = -normDir.y / absDir.z;
            break;
        case 5: // -Z
            u = normDir.x / absDir.z;
            v = -normDir.y / absDir.z;
            break;
    }

    u = 0.5f * (u + 1);
    v = 0.5f * (v + 1);

    return float2(u, v);
}

float2 Shadow_GetAtlasUV(int face, float2 faceUV, float2 tileSize)
{
    float2 faceOffsets[] = 
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 0 },
        { 0, 1 },
        { 1, 1 },
        { 2, 1 },
    };
    
    float2 pixelUV = (faceOffsets[face].xy + faceUV) * tileSize;   
    return pixelUV * SHADOW_ATLAS_SCALE;
}

float Shadow_GetPCFSample(float2 uv, float compareDepth)
{
    /*
    float result = 0.0;

    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y)
        {
            float2 offset = float2(x, y) * texelSize;
            float sample = SAMPLE_TEXTURE(ShadowAtlasTexture, uv + offset).r;
            result += (sample >= compareDepth) ? 1.0 : 0.0;
        }
    }

    return result / 9.0;
    */
    
    float minScale = 0.2;
    float maxScale = 2.0;
    
    float2 texelSize = SHADOW_ATLAS_SCALE.xx;
    float kernelScale = clamp(compareDepth * 1.5, minScale, maxScale);
    float2 offsetSize = texelSize * kernelScale;

    float result = 0.0;
    
    UNROLL
    for (int x = -1; x <= 1; ++x)
    {
        UNROLL
        for (int y = -1; y <= 1; ++y)
        {
            float2 offset = float2(x, y) * offsetSize;
            float sample = SAMPLE_TEXTURE(ShadowAtlasTexture, uv + offset).r;
            result += (sample >= compareDepth) ? 1.0 : 0.0;
        }
    }
    
    return result / 9.0;
}

float Shadow_Sample(float3 index, float3 viewPosition, float3 lightPosition, float oneOverLightRadius, out float3 debugColor)
{
    // If there is no face size... then we're fully unshadowed.
    if (index.z < 1)
        return 0;
       
    float3 lightVector = viewPosition - lightPosition;
    float3 lightDir = normalize(lightVector);
    
    int face = Shadow_GetFaceIndex(lightDir);
    debugColor = Shadow_GetDebugFaceColors(face);
    
    float2 faceUV = Shadow_ProjectDirectionToFaceUV(face, lightDir);
    
    float2 atlasUV = (index.xy * SHADOW_ATLAS_SCALE) + Shadow_GetAtlasUV(face, faceUV, float2(index.z, index.z));
    
    float actualDepth = length(lightVector) * oneOverLightRadius;
    
    float shadow = 1.0f - Shadow_GetPCFSample(atlasUV, actualDepth);

    //float shadowDepth = SAMPLE_TEXTURE(ShadowAtlasTexture, atlasUV).r;  
    //float fuzziness = 0.015;    
    //float shadow = smoothstep(shadowDepth, shadowDepth + fuzziness, actualDepth);
    
    return shadow;
}

#endif // SHADOWMAP_H