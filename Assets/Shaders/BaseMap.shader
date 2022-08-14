Shader "Custom/BaseMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Albedo ("Albedo", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Float) = 1.0
        _Smoothness ("Smoothness", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "BaseMap.hlsl"
            ENDHLSL
        }
    }
}
