Shader "Kaiser RP/GBuffer"{
        
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "LightMode" = "KRPgbuffer" }
        Pass
        {
            HLSLPROGRAM
            
            #pragma target 3.5
            
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #pragma vertex GBufferPassVertex
            #pragma fragment GBufferPassFragment
            
            #include "GBuffer.hlsl"
            
            ENDHLSL
        }
    }
}