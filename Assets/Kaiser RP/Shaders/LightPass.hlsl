#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"


// UNITY_INSTANCING_BUFFER_START(PerInstance)
// 	UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
// UNITY_INSTANCING_BUFFER_END(PerInstance)

TEXTURE2D(_GT0);
SAMPLER(sampler_GT0);

TEXTURE2D(_GT1);
SAMPLER(sampler_GT1);

TEXTURE2D(_GT2);
SAMPLER(sampler_GT2);

TEXTURE2D(_GT3);
SAMPLER(sampler_GT3);

TEXTURE2D(_gdepth);
SAMPLER(sampler_gdepth);

CBUFFER_START(UnityPerMaterial)
    float4 _GT0_ST;
    float4 _GT1_ST;
    float4 _GT2_ST;
    float4 _GT3_ST;
    float4 _gdepth_ST;
CBUFFER_END

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LightPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);


    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(positionWS);
    output.uv = input.uv;

    return output;
}

float4 LightPassFragment(Varyings input) : SV_Target
{
    float2 uv = input.uv;
    float3 albedo = SAMPLE_TEXTURE2D(_GT0, sampler_GT0, uv).rgb;
    float3 normal = SAMPLE_TEXTURE2D(_GT1, sampler_GT1, uv).rgb;
    normal = normal * 2.0 - 1.0;
    // float3 lightDir = GetMainLight();
    float3 lightDir = normalize(_MainLightPosition.xyz);
    // float3 V = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);

    float depth = SAMPLE_TEXTURE2D(_gdepth, sampler_gdepth, uv).r;

    // float4 ndcPos = float4(uv * 2 - 1, depth, 1);
    // float4 worldPos = mul(UNITY_MATRIX_I_VP, ndcPos);


    UNITY_SETUP_INSTANCE_ID(input);
    return float4(albedo * saturate(dot(normal, lightDir)), 1.0);
    // return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);

}
