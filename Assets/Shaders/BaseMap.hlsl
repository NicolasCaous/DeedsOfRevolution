#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
float4 _MainTex_ST;
float4 _Albedo;
float _Brightness;
float _Smoothness;

struct vertex {
	float3 positionOS : POSITION;
	float2 uv : TEXCOORD0;
	half3 normal : NORMAL;
};

struct v2f {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	half3 normalWS : NORMAL;
	float3 viewDir : TEXCOORD1;
};

v2f Vertex(vertex input) {
	v2f output;

	output.positionCS = GetVertexPositionInputs(input.positionOS).positionCS;
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	output.normalWS = TransformObjectToWorldNormal(input.normal);
	output.viewDir = GetWorldSpaceViewDir(TransformObjectToWorld(output.positionCS));

	return output;
}

float4 Fragment(v2f input) : SV_TARGET {
	float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
	Light mainLight = GetMainLight();

	float diffuse = saturate(dot(input.normalWS, mainLight.direction));

	color.xyz = saturate(color.xyz * _Brightness) * _Albedo * diffuse;

	return color;
}