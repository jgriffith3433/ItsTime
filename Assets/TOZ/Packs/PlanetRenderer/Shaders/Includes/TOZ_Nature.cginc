#ifndef TOZ_NATURE_INCLUDED
#define TOZ_NATURE_INCLUDED

#include "UnityCG.cginc"

//Variables
float _Occlusion, _AO, _BaseLight;
//float4 _Woosh;
static const float4 _Woosh = float4(1.0, 0.0, 1.0, 0.10);

float4 SmoothCurve(float4 x) {
	return x * x *(3.0 - 2.0 * x);
}
float4 TriangleWave(float4 x) {
	return abs(frac(x + 0.5) * 2.0 - 1.0);
}
float4 SmoothTriangleWave(float4 x) {
	return SmoothCurve(TriangleWave(x));
}

// Detail Bending
inline float4 AnimateVertex(float4 pos, float3 normal, float4 animParams) {
	// animParams.x = branch phase
	// animParams.y = edge flutter factor
	// animParams.z = primary factor
	// animParams.w = secondary factor
	//For TreeIT
	//params.x = color.z
	//params.y = color.x
	//params.z = color.w //Actually lightmap coord.x but color.w is smiliar
	//params.w = color.z //Actually lightmap coord.y but isnt necessary

	// Phases (object, branch, vertex)
	float objPhase = dot(unity_ObjectToWorld._14_24_34, 1.0);
	float branchPhase = objPhase + animParams.x;
	float vertexPhase = dot(pos.xyz, branchPhase + animParams.y);

	// x is used for edges; y is used for branches
	float2 wavesIn = _Time.yy + float2(vertexPhase, branchPhase);

	// 1.975, 0.793, 0.375, 0.193 are good frequencies
	float4 waves = (frac(wavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0);

	waves = SmoothTriangleWave(waves);
	float2 wavesSum = waves.xz + waves.yw;

	float detailAmp = 0.1;
	float branchAmp = 0.3;

	// Edge (xz) and branch bending (y)
	float3 bend = animParams.y * detailAmp * normal.xyz;
	//bend.y = animParams.w * branchAmp; // jumping leaves and trunk is stupid
	pos.xyz += ((wavesSum.xyx * bend) + (_Woosh.xyz * wavesSum.y * animParams.w)) * _Woosh.w;

	// Primary bending
	pos.xyz += animParams.z * _Woosh.xyz;

	return pos;
}

float3 ShadeOccludedLeaves(float4 vertex, float4 tangent, fixed4 color, int lightCount) {
	float3 viewpos = UnityObjectToViewPos (vertex.xyz);

	float4 lightDir = 0;
	//lightDir.w = 1;
	lightDir.w = _AO * color.w;
	float3 lightColor = 0.0;
	for(int i = 0; i < lightCount; i++) {
		float3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
		toLight.z *= -1.0;
		// don't produce NaNs if some vertex position overlaps with the light
		float lengthSq = max(dot(toLight, toLight), 0.000001);
		float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);

		lightDir.xyz = mul((float3x3)unity_CameraToWorld, normalize(toLight));
		lightDir.xyz *= _Occlusion * color.w;
		//lightDir.w = (atten / 2) * _AO;

		//float diff = max(0, dot(tangent, lightDir));
		//float diff = max(0, dot(tangent, lightDir)) + _BaseLight;
		float diff = (max(0, dot(tangent, lightDir)) + _BaseLight) / (1.0 + _BaseLight);
		lightColor += unity_LightColor[i].rgb * (diff * atten);
	}

	return (UNITY_LIGHTMODEL_AMBIENT.xyz + lightColor);
}

float3 ShadeOccludedBark(float4 vertex, float3 normal, float4 tangent, int lightCount) {
	float3 viewpos = UnityObjectToViewPos (vertex.xyz);

	float4 lightDir = 0;
	lightDir.w = _AO;
	float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
	for(int i = 0; i < lightCount; i++) {
		float3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
		toLight.z *= -1.0;
		// don't produce NaNs if some vertex position overlaps with the light
		float lengthSq = max(dot(toLight, toLight), 0.000001);
		float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);

		lightDir.xyz = mul((float3x3)unity_CameraToWorld, normalize(toLight));

		float diff = max(0, dot(normal, lightDir)) * (_AO * tangent.w + _BaseLight);
		lightColor += unity_LightColor[i].rgb * (diff * atten);
	}

	return (lightColor);
}

#endif
