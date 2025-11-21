#ifndef TOZ_ATMOSPHERE_INCLUDED
#define TOZ_ATMOSPHERE_INCLUDED

#include "UnityCG.cginc"

//Variables
static const int sampleCount = 2;

uniform float3 _CamPos;					// The camera's current position
uniform float _CamHeight;					// The camera's current height (distance to planet center)
uniform float _InnerRadius;				// The inner (planetary) radius
uniform float _OuterRadius;				// The outer (atmosphere) radius
uniform float _AverageDensity; 			// The scale depth (the altitude of the atmosphere's average density)
uniform float3 _SunDirection;				// The direction vector to the sun light source
uniform float _SunPower; 					// Power of the sun
uniform float3 _FlareDirection;			// The direction vector to sun flare object
uniform float3 _InvWaveLength;			// 1 / pow(wavelength, 4) for the red, green, and blue channels
uniform float _Kr; 							// skyDensity
uniform float _Km; 							// sunSize
uniform float _G;								// Sun blur[Range(-0.985f, -0.99f)]

#define _CamHeight2 (_CamHeight * _CamHeight)
#define _InnerRadius2 (_InnerRadius * _InnerRadius)
#define _OuterRadius2 (_OuterRadius * _OuterRadius)
#define _Scale (1.0f / (_OuterRadius - _InnerRadius))
#define _ScaledAverageDensity (_Scale / _AverageDensity)
#define _SkyDensity (_Kr * _SunPower)
#define _SunSize (_Km * _SunPower)
#define _Kr4PI (_Kr * 4.0 * 3.14159265359)
#define _Km4PI (_Km * 4.0 * 3.14159265359)
#define _G2 (_G * _G)

#define SKY_GROUND_THRESHOLD 0.05
#define MAX_SCATTER 50.0 // Maximum scattering value, to prevent math overflows on Adrenos

//The scale equation calculated by Vernier's Graphical Analysis
float ScaleDepth(float val) {
	float x = 1.0 - val;
	return _AverageDensity * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
}

//Calculates the Rayleigh phase function
inline float RayleighPhase(float val) {
	return 0.75 + (1.0 * val);
}

//Calculates the Mie phase function
inline float MiePhase(float val, float val2) {
	return 1.5 * ((1.0 - _G2) / (2.0 + _G2)) * (1.0 + val2) / pow(1.0 + _G2 - 2.0 * _G * val, 1.5);
}

//Returns the near intersection point of a line and a sphere
float GetNearIntersection(float3 pos, float3 ray, float distance2, float radius2) {
	float B = 2.0 * dot(pos, ray);
	float C = distance2 - radius2;
	float det = max(0.0, B*B - 4.0 * C);
	return 0.5 * (-B - sqrt(det));
}

struct a2v {
	float4 vertex : POSITION;
};

struct v2f {
	float4 pos : SV_POSITION;
	float3 dir : TEXCOORD0;
	float4 col0 : COLOR0;
	float4 col1 : COLOR1;
	half CDotV : TEXCOORD1;
	//UNITY_FOG_COORDS(2)
};
#endif