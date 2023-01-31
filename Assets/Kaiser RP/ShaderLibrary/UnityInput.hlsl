#ifndef KAISER_UNITY_INPUT_INCLUDED
#define KAISER_UNITY_INPUT_INCLUDED

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;

    float4x4 unity_MatrixV;
    float4x4 unity_MatrixInvV;
    float4x4 glstate_matrix_projection;
    real4 unity_WorldTransformParams;

    float4x4 unity_MatrixPreviousM;
    float4x4 unity_MatrixPreviousMI;
CBUFFER_END

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerCamera)
    float3 _WorldSpaceCameraPos;
CBUFFER_END

#endif