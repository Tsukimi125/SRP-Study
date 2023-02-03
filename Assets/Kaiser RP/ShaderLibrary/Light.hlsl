#ifndef KAISER_LIGHT_INCLUDED
#define KAISER_LIGHT_INCLUDED

#include "Shadows.hlsl"

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_KaiserLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];

    int _MainLightIndex;
    float4 _MainLightPosition;
    float4 _MainLightColor;

    int _DebugCascade;
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
    float attenuation;
};

Light getMainLight()
{
    Light light;
    light.color = float3(1.0, 1.0, 1.0);
    light.direction = float3(0.0, 1.0, 0.0);

    return light;
}

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex, ShadowData shadowData)
{
    DirectionalShadowData dirShadowData;
    dirShadowData.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
    dirShadowData.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    dirShadowData.normalBias = _DirectionalLightShadowData[lightIndex].z;
    return dirShadowData;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);

    // DEBUG CASCADE
    light.attenuation = lerp(light.attenuation, shadowData.cascadeIndex * 0.25, _DebugCascade);
    return light;
}

#endif


// #define MAX_OTHER_LIGHT_COUNT 64

// CBUFFER_START(_LightBuffer)
//     int _DirectionLightCount;
//     float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
//     float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
//     float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
//     //volume
//     float4 _DirectionalLightSampleData[MAX_DIRECTIONAL_LIGHT_COUNT];
//     float4 _DirectionalLightScatterData[MAX_DIRECTIONAL_LIGHT_COUNT];
//     float4 _DirectionalLightNoiseData[MAX_DIRECTIONAL_LIGHT_COUNT];
//     float4 _DirectionalLightNoiseVelocity[MAX_DIRECTIONAL_LIGHT_COUNT];

//     int _OtherLightCount;
//     float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightShadowData[MAX_OTHER_LIGHT_COUNT];
//     //volume
//     float4 _OtherLightSampleData[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightScatterData[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightNoiseData[MAX_OTHER_LIGHT_COUNT];
//     float4 _OtherLightNoiseVelocity[MAX_OTHER_LIGHT_COUNT];

//     int _MainLightIndex;
//     float4 _MainLightPosition;
//     float4 _MainLightColor;
// CBUFFER_END