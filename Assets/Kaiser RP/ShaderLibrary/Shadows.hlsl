#ifndef KAISER_SHADOWS_INCLUDED
#define KAISER_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Common.hlsl"
#include "Surface.hlsl"


#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_KaiserShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowDistanceFade;
CBUFFER_END

struct ShadowData
{
    int cascadeIndex;
    float strength;
};

float FadeShadowStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData shadowData;
    shadowData.strength = FadeShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSquared = DistanceSquared(
            surfaceWS.position, sphere.xyz
        );
        if (distanceSquared < sphere.w)
        {
            if (i == _CascadeCount - 1)
            {
                shadowData.strength *= FadeShadowStrength(
                    distanceSquared, 1.0 / sphere.w, _ShadowDistanceFade.z
                );
            }
            break;
        }
    }
    if (i == _CascadeCount)
    {
        shadowData.strength = 0.0;
    }
    shadowData.cascadeIndex = i;
    return shadowData;
}

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
};

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
    );
}

float GetDirectionalShadowAttenuation(
    DirectionalShadowData shadowData, Surface surfaceWS) 
{
    if (shadowData.strength == 0.0)
    {
        return 1.0;
    }
    
    float3 positionSTS = mul(
        _DirectionalShadowMatrices[shadowData.tileIndex], 
        float4(surfaceWS.position, 1.0)
    ).xyz;

    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, shadowData.strength);
}

#endif