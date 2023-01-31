#ifndef KAISER_SHADOWS_INCLUDED
#define KAISER_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Surface.hlsl"

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_KaiserShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

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