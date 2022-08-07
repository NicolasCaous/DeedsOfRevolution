#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
float4 _MainTex_ST;

struct vertex {
	float3 positionOS : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
};

v2f Vertex(vertex input) {
	v2f output;

	output.positionCS = GetVertexPositionInputs(input.positionOS).positionCS;
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);

	return output;
}

float4 Fragment(v2f input) : SV_TARGET{
	float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
	return color;
}