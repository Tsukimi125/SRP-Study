Shader "Kaiser RP/LightPass"
{
    
    Properties
    {

    }

    SubShader
    {
        Tags { "LightMode" = "KRPlightpass" }
        Pass
        {
            HLSLPROGRAM
            
            #pragma target 3.5
            
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #pragma vertex LightPassVertex
            #pragma fragment LightPassFragment
            
            #include "LightPass.hlsl"
            
            ENDHLSL
        }
    }
}