#ifndef TOZ_MATH_INCLUDED
#define TOZ_MATH_INCLUDED

static const float EPSILON = 1.0e-4;
static const float FLT_MIN = 1.175494351e-38; // Minimum representable positive floating-point number
static const float FLT_MAX = 3.402823466e+38; // Maximum representable floating-point number

//remaps v to [0;1] in interval [min;max]
inline float remap(float v, float min, float max) {
	return clamp((v-min) / (max-min), 0.0, 1.0);
}

//grades t to [a;b] in interval [min;max]
inline float grade(float a, float b, float min, float max, float t) {
	return lerp(a, b, smoothstep(min, max, t));
}

//quantizes a in v levels
inline float truncate(float a, float v) {
	return floor(a * v) / v;
}

//interpolate between a, b, c with t
inline float interp3(float a, float b, float c, float t) {
    float v0 = lerp(a, b, t);
    float v1 = lerp(b, c, t);
    return lerp(v0, v1, t);
}

//interpolate between a, b, c with t
inline fixed4 interp3(fixed4 a, fixed4 b, fixed4 c, float t) {
    fixed4 v0 = lerp(a, b, t);
    fixed4 v1 = lerp(b, c, t);
    return lerp(v0, v1, t);
}

//interpolate between a, b, c, d with t
inline float interp4(float a, float b, float c, float d, float t) {
    float v0 = lerp(a, b, t);
    float v1 = lerp(b, c, t);
    float v2 = lerp(c, d, t);
    float v3 = lerp(v0, v1, t);
    float v4 = lerp(v1, v2, t);
    return lerp(v3, v4, t);
}

//interpolate between a, b, c, d with t
inline fixed4 interp4(fixed4 a, fixed4 b, fixed4 c, fixed4 d, float t) {
    fixed4 v0 = lerp(a, b, t);
    fixed4 v1 = lerp(b, c, t);
    fixed4 v2 = lerp(c, d, t);
    fixed4 v3 = lerp(v0, v1, t);
    fixed4 v4 = lerp(v1, v2, t);
    return lerp(v3, v4, t);
}

//Better pow
inline float safepow(float a, float v) {
	return exp(log(a) * v);
}

inline float SmoothCurve( float x ) {
    return x * x *( 3.0 - 2.0 * x );
}

//Creates a band in the given range
inline float band(float bottom, float center, float top, float t) {
	return smoothstep(center - bottom, center, t) * smoothstep(center + top, center, t);
}
#endif