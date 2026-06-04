// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if !defined(MACROS_H)
#define MACROS_H


#if defined(SM6)

// Macros for targetting shader model 6.0 (Vulkan, DX12)
#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0_level_9_1 vsname (); PixelShader = compile ps_4_0_level_9_1 psname(); } }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define _vs(r)
#define _ps(r)
#define _cb(r)

#define DECLARE_TEXTURE_FORMAT(Name, Format, Index) \
    Texture2D<Format> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_TEXTURE(Name, Index) \
    Texture2D<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_TEXTURE3D(Name, Index) \
    Texture3D<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_CUBEMAP(Name, Index) \
    TextureCube<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define LOAD_TEXTURE(Name, texCoord)  Name.Load(texCoord)
#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_TEXTURE3D(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

#define UNROLL [unroll]


#elif defined(SM4)

// DirectX 11 path

// Macros for targetting shader model 4.0 (DX11)
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0_level_9_1 vsname (); PixelShader = compile ps_4_0_level_9_1 psname(); } }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define _vs(r)
#define _ps(r)
#define _cb(r)

#define DECLARE_TEXTURE_FORMAT(Name, Format, Index) \
    Texture2D<Format> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_TEXTURE(Name, Index) \
    Texture2D<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_TEXTURE3D(Name, Index) \
    Texture3D<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define DECLARE_CUBEMAP(Name, Index) \
    TextureCube<float4> Name : register(t##Index); \
    sampler Name##Sampler : register(s##Index)

#define LOAD_TEXTURE(Name, texCoord)  Name.Load(texCoord)
#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_TEXTURE3D(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

#define UNROLL [unroll]

#else
 
// OpenGL path

#define SV_POSITION POSITION
#define SV_TARGET0 COLOR
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

#define BEGIN_CONSTANTS
#define MATRIX_CONSTANTS
#define END_CONSTANTS

#define _vs(r)  : register(vs, r)
#define _ps(r)  : register(ps, r)
#define _cb(r)

#define DECLARE_TEXTURE(Name, Index) \
    sampler2D Name : register(s##Index)

#define DECLARE_TEXTURE3D(Name, Index) \
    sampler3D Name : register(s##Index)

#define DECLARE_CUBEMAP(Name, Index) \
    samplerCUBE Name : register(s##Index)

#define DECLARE_TEXTURE_FORMAT(Name, Format, Index) \
    sampler2D Name : register(s##Index)

#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  texCUBE(Name, texCoord)
#define SAMPLE_TEXTURE3D(Name, texCoord)  tex3D(Name, texCoord)
#define LOAD_TEXTURE(Name, texCoord)  tex2D(Name, texCoord.xy)

#define UNROLL [unroll]

#endif


#endif // MACROS_H
