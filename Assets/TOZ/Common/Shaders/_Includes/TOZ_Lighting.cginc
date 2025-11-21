#ifndef TOZ_LIGHTING_INCLUDED
#define TOZ_LIGHTING_INCLUDED

#include "Lighting.cginc"

#ifdef CUSTOMSUNDIRECTION
	uniform float4 _SunDirection;
#endif

/*******************************************************
//LIGHT MODELS
*******************************************************/
inline fixed Lambert(fixed3 normal, half3 lightDir) {
	return max(dot(normal, lightDir), 0.0);
}

fixed _LightWrapAmount;
inline fixed LambertWrap(fixed3 normal, half3 lightDir) {
	fixed NdotL = max(dot(normal, lightDir), 0);
	return (NdotL + _LightWrapAmount) / (1.0 + _LightWrapAmount);
}

//Minnaert is good for planets and some fabrics
inline fixed Minnaert(fixed3 normal, half3 viewDir, half3 lightDir) {
	fixed NdotL = max(dot(normal, lightDir), 0);
	half NdotV = max(dot(normal, viewDir), 0);
	return saturate(pow(NdotL, 1.0 + _LightWrapAmount) * pow(1.0 - NdotV, 1.0 - _LightWrapAmount));
}

inline fixed Lommel(fixed3 normal, half3 viewDir, half3 lightDir) {
	//lightDir.y = -0.5;
	//lightDir.y = viewDir.y; //Moon thing, not looking good
	fixed NdotL = max(dot(normal, lightDir), 0);
	half NdotV = max(dot(normal, viewDir), 0);
	return NdotL / (NdotL + NdotV);
}

inline fixed Toon(fixed3 normal, half3 lightDir) {
	fixed diff = max(dot(normal, lightDir), 0.0);
	if(diff < 0.4)
		diff *= 0.3;
	else if(diff < 0.7)
		diff *= 0.6;
	else
		diff *= 1.0;
	return diff;
}

fixed _Roughness;
inline fixed OrenNayar(fixed3 normal, half3 viewDir, half3 lightDir, fixed gloss, half spec) {
	fixed NdotL = dot(normal, lightDir);
	half NdotV = dot(normal, viewDir);

	half3 lProj = normalize(lightDir - normal * NdotL);
	half3 vProj = normalize(viewDir - normal * NdotV);
	half cx = max(dot(lProj, vProj), 0);
	half cosa = min(NdotL, NdotV);
	half cosb = max(NdotL, NdotV);
	half dx = sqrt((1.0 - cosa * cosa) * (1.0 - cosb * cosb)) / cosb;
	return max(NdotL, 0.0) * (_Roughness + (gloss * spec) * cx * dx);
}

inline float BlinnSpec(fixed3 normal, half3 viewDir, half3 lightDir, fixed gloss, half spec) {
	half3 halfDir = normalize(lightDir + viewDir);
	half NdotH = max(dot(normal, halfDir), 0);
	return gloss * pow(NdotH, spec * 128.0);
}

inline float PhongSpec(fixed3 normal, half3 viewDir, half3 lightDir, fixed gloss, half spec) {
	half3 reflDir = normalize(reflect(-viewDir, normal));
	half LdotR = max(dot(lightDir, reflDir), 0);
	return gloss * pow(LdotR, spec * 128.0);
}

//Ward-isotropic
inline float IsotropicSpec(fixed3 normal, half3 viewDir, half3 lightDir, fixed gloss, half spec) {
	half3 halfDir = normalize(lightDir + viewDir);
	half NdotH = max(dot(normal, halfDir), 0);
	half NdotH2 = NdotH * NdotH;
	return gloss * exp(-(spec * 128.0) * (1.0 - NdotH2) / NdotH2);
}

fixed _Torrance;
inline float CookTorranceSpec(fixed3 normal, half3 viewDir, half3 lightDir, fixed gloss, half spec) {
	fixed NdotL = dot(normal, lightDir);
	half NdotV = dot(normal, viewDir);
	half3 h = normalize(lightDir + viewDir);
	half LdotH = saturate(dot(lightDir, h));
	fixed NdotH = saturate(dot(normal, h));
	half NdotHSqr = NdotH * NdotH;
	half roughnessSqr = _Roughness * _Roughness;

	half f = lerp(pow(1.0-NdotV, 5.0), 1.0, _Roughness);
	half x = 2.0 * NdotH / dot(viewDir, h);
	half g = min(1.0, min(x * NdotL, x * NdotV));
	return gloss * pow(_Torrance * f * g / NdotV, spec * 128.0);
}

fixed _RimBias, _RimPower;
inline float Rim(fixed3 normal, half3 viewDir) {
	half NdotV = max(dot(normal, viewDir), 0);
	return pow(1.0 + _RimBias - NdotV, _RimPower);
}

fixed _SSSPower, _SSSThickness;
inline float SSS(half3 viewDir, half3 lightDir) {
	half LdotV = max(dot(viewDir, -lightDir), 0.0);
	return pow(LdotV, _SSSPower) * (1.0 - _SSSThickness);
}

inline fixed Translucency(half3 viewDir, half3 lightDir) {
	return max(dot(viewDir, -lightDir), 0.0);
}

/*******************************************************
//HELPERS
*******************************************************/
inline void CalculateGI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		data.light.dir = _SunDirection.xyz;
		//gi.light.dir = _SunDirection.xyz; NO NEED
	#endif

	gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline half4 CalculateLambertDeferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif

	UnityStandardData data;
	data.diffuseColor = s.Albedo;
	data.occlusion = 1;
	data.specularColor = 0;
	data.smoothness = 0;
	data.normalWorld = s.Normal;

	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);
	half4 emission = half4(s.Emission, 1);

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		emission.rgb += s.Albedo * gi.indirect.diffuse;
	#endif

	return emission;
}

inline half4 CalculatePhongDeferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif

	UnityStandardData data;
	data.diffuseColor = s.Albedo;
	data.occlusion = 1;
	data.specularColor = _SpecColor.rgb * s.Gloss * (1/UNITY_PI);
	data.smoothness = s.Specular;
	data.normalWorld = s.Normal;

	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);
	half4 emission = half4(s.Emission, 1);

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		emission.rgb += s.Albedo * gi.indirect.diffuse;
	#endif

	return emission;
}

/*******************************************************
//NOLIGHTS
*******************************************************/
inline fixed4 TOZNoLightsLight(SurfaceOutput s, UnityLight light) {
	//Result
    fixed4 c;
    c.rgb = s.Albedo;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZNoLights(SurfaceOutput s, UnityGI gi) {
	fixed4 c = TOZNoLightsLight(s, gi.light);
    return c;
}

inline half4 LightingTOZNoLights_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZNoLights_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//LAMBERT
*******************************************************/
inline fixed4 TOZLambertLight(SurfaceOutput s, UnityLight light) {
	//Diffuse
	fixed diff = Lambert(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZLambert(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZLambertLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZLambert_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZLambert_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//WRAPPED LAMBERT
*******************************************************/
inline fixed4 TOZLambertWrapLight(SurfaceOutput s, UnityLight light) {
	//Diffuse
	fixed diff = LambertWrap(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZLambertWrap(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZLambertWrapLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZLambertWrap_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZLambertWrap_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//MINNAERT
*******************************************************/
inline fixed4 TOZMinnaertLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = Minnaert(s.Normal, viewDir, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);
/*
	#if defined(USING_DIRECTIONAL_LIGHT)
		float sunAmount = max(dot(-viewDir, light.dir), 0.0);
		unity_FogColor.rgb = lerp(unity_FogColor.rgb, light.color, pow(sunAmount, 64));
	#endif
*/
	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZMinnaert(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZMinnaertLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZMinnaert_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZMinnaert_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//LOMMEL
*******************************************************/
inline fixed4 TOZLommelLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = Lommel(s.Normal, viewDir, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZLommel(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZLommelLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZLommel_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZLommel_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//TOON
*******************************************************/
inline fixed4 TOZToonLight(SurfaceOutput s, UnityLight light) {
	//Diffuse
	fixed diff = Toon(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Result
    fixed4 c;
	c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZToon(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZToonLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZToon_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZToon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//BLINN
*******************************************************/
inline fixed4 TOZBlinnLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = Lambert(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Specular
	float spec = BlinnSpec(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 specColor = (light.color * spec);

	//Result
	fixed4 c;
	c.rgb = diffColor + specColor;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingTOZBlinn(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZBlinnLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZBlinn_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZBlinn_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//PHONG
*******************************************************/
inline fixed4 TOZPhongLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = Lambert(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Specular
	float spec = PhongSpec(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 specColor = (light.color * spec);

	//Result
	fixed4 c;
	c.rgb = diffColor + specColor;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingTOZPhong(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZPhongLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZPhong_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZPhong_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//PHONG-RIM
*******************************************************/
inline fixed4 TOZRimPhongLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Rim
	float rim = Rim(s.Normal, viewDir);
	fixed3 rimColor = (light.color * rim);

	//Result
	fixed4 c = TOZPhongLight(s, viewDir, light);
	c.rgb += rimColor;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingTOZRimPhong(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZRimPhongLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZRimPhong_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZRimPhong_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//ISOTROPIC
*******************************************************/
inline fixed4 TOZIsotropicLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = Lambert(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Specular
	float spec = IsotropicSpec(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 specColor = (light.color * spec);

	//Result
	fixed4 c;
	c.rgb = diffColor + specColor;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingTOZIsotropic(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZIsotropicLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZIsotropic_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZIsotropic_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//BIDIRECTIONAL
*******************************************************/
inline fixed4 TOZBiDirectionalLight(SurfaceOutput s, UnityLight light) {
	//Data
	fixed NdotL = dot(s.Normal, light.dir);

	//Diffuse
	fixed d1 = max(NdotL, 0);
	fixed d2 = max(-NdotL, 0);
	fixed lum = Luminance(light.color);
	fixed3 diffColor = s.Albedo * ((light.color * d1) + (lum * fixed3(0,1,0) * d2));

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZBiDirectional(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZBiDirectionalLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZBiDirectional_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZBiDirectional_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//HEMISPHERIC
*******************************************************/
inline fixed4 TOZHemisphericLight(SurfaceOutput s, UnityLight light) {
	//Data
	fixed NdotL = dot(s.Normal, light.dir);

	//Diffuse
	fixed diff = (NdotL + 1.0) * 0.5;
	fixed lum = Luminance(light.color);
	fixed3 diffColor = s.Albedo * lerp(lum * fixed3(0,1,0), light.color, diff);

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZHemispheric(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZHemisphericLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZHemispheric_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZHemispheric_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//TRIDIRECTIONAL
*******************************************************/
inline fixed4 TOZTriDirectionalLight(SurfaceOutput s, UnityLight light) {
	//Data
	fixed NdotL = dot(s.Normal, light.dir);

	//Diffuse
	fixed d1 = max(NdotL, 0);
	fixed d2 = max(-NdotL, 0);
	fixed d3 = 1.0 - abs(NdotL);
	fixed lum = Luminance(light.color);
	fixed3 diffColor = s.Albedo * ((light.color * d1) + (lum * fixed3(0,1,0) * d2) + (lum * fixed3(0,0,1) * d3));

	//Result
    fixed4 c;
    c.rgb = diffColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZTriDirectional(SurfaceOutput s, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZTriDirectionalLight(s, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZTriDirectional_Deferred(SurfaceOutput s, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculateLambertDeferred(s, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZTriDirectional_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//SSS
*******************************************************/
fixed4 _SSSColor;
inline fixed4 TOZSSSLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	//fixed diff = Lambert(s.Normal, light.dir);
	fixed diff = LambertWrap(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Specular
	float spec = PhongSpec(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 specColor = (light.color * spec);

	//SSS
	float sss = SSS(viewDir, light.dir);
	fixed3 sssColor = _SSSColor.rgb * (light.color * sss);

	//Rim
	float rim = Rim(s.Normal, viewDir);
	fixed3 rimColor = _SSSColor.rgb * (light.color * rim) * sss;
	
	//Result
    fixed4 c;
    c.rgb = diffColor + specColor + sssColor + rimColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZSSS(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZSSSLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZSSS_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZSSS_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//OREN-NAYAR
*******************************************************/
inline fixed4 TOZOrenNayarLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = OrenNayar(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 diffColor = s.Albedo * (light.color * diff);

	//Specular
	float spec = CookTorranceSpec(s.Normal, viewDir, light.dir, s.Gloss, s.Specular);
	fixed3 specColor = (light.color * spec);

	//Result
    fixed4 c;
	c.rgb = diffColor + specColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZOrenNayar(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZOrenNayarLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZOrenNayar_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZOrenNayar_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

/*******************************************************
//LEAF (BLINN)
*******************************************************/
inline fixed4 TOZLeafLight(SurfaceOutput s, half3 viewDir, UnityLight light) {
	//Diffuse
	fixed diff = LambertWrap(s.Normal, light.dir);
	fixed3 diffColor = s.Albedo * (light.color * diff);

    // view dependent back contribution for translucency
	float translucency = Translucency(viewDir, light.dir);
	fixed3 translucencyColor = _SSSThickness * (light.color * translucency) * s.Albedo;

	//Result
    fixed4 c;
	c.rgb = diffColor + translucencyColor;
    c.a = s.Alpha;
    return c;
}

inline fixed4 LightingTOZLeaf(SurfaceOutput s, half3 viewDir, UnityGI gi) {
	#if defined(CUSTOMSUNDIRECTION) && defined(USING_DIRECTIONAL_LIGHT)
		gi.light.dir = _SunDirection.xyz;
	#endif
	fixed4 c = TOZLeafLight(s, viewDir, gi.light);
    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif
    return c;
}

inline half4 LightingTOZLeaf_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
	return CalculatePhongDeferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
}

inline void LightingTOZLeaf_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
	CalculateGI(s, data, gi);
}

#endif
