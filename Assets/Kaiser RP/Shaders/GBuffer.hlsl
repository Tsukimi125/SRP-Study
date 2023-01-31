

#include "../ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

// UNITY_INSTANCING_BUFFER_START(PerInstance)
// 	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
// UNITY_INSTANCING_BUFFER_END(PerInstance)

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float _Color;
    float4 _MainTex_ST;
    float _Cutoff;
CBUFFER_END

struct Attributes {
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 tangentWS : TEXCOORD3;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings GBufferPassVertex (Attributes input) {
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);


    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    //access material property via UNITY_ACCESS_INSTANCED_PROP( , );
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.uv = input.uv * baseST.xy + baseST.zw;
    // float4 detailST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DetailMap_ST);
    // output.detailUV = input.baseUV * detailST.xy + detailST.zw;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
    return output;
}

void GBufferPassFragment(
    Varyings input,
    out float4 GT0 : SV_Target0,
    out float4 GT1 : SV_Target1,
    out float4 GT2 : SV_Target2,
    out float4 GT3 : SV_Target3)
{	
    float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
    float3 normal = input.normalWS;

    GT0 = float4(color, 1);
    GT1 = float4(normal * 0.5 + 0.5, 0);
    GT2 = float4(1, 1, 0, 1);
    GT3 = float4(0, 0, 1, 1);

    UNITY_SETUP_INSTANCE_ID(input);
	// return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
}
