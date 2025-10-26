#ifndef EFFECT_3DEFFECT
#define EFFECT_3DEFFECT
#include "common.fxh"

float4x4 MatrixTransform;
float2 ScreenSize;
float SpinAmount;

VertexShaderOutput MainVS(VertexShaderInput input) 
{
    VertexShaderOutput output;

    float4 pos = input.Position;
    
    // create the center of rotation
    float2 centerXZ = float2(ScreenSize.x * .5, 0);
    
    // convert the debug variable into an angle from 0 to 2 pi. 
    //  shaders use radians for angles, so 2 pi = 360 degrees
    float angle = SpinAmount * 6.28;
    
    // pre-compute the cos and sin of the angle
    float cosA = cos(angle);
    float sinA = sin(angle);
    
    // shift the position to the center of rotation
    pos.xz -= centerXZ;
    
    // compute the rotation
    float nextX = pos.x * cosA - pos.z * sinA;
    float nextZ = pos.x * sinA + pos.z * cosA;
    
    // apply the rotation
    pos.x = nextX;
    pos.z = nextZ;
    
    // shift the position away from the center of rotation
    pos.xz += centerXZ;
    
    output.Position = mul(pos, MatrixTransform);
    output.Color = input.Color;
    output.TextureCoordinates = input.TexCoord;
    return output;
}
#endif