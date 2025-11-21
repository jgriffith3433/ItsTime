Shader "TOZ/Celestial/AtmosphereFromSpace" {
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent-401" "IgnoreProjector" = "True" }
		LOD 100

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			Cull Front
			ZWrite Off
			Blend One One

			CGPROGRAM
			#include "../../Includes/TOZ_Atmosphere.cginc"
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(a2v v) {
				//Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 wCenterPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float3 wVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz - wCenterPos;

				float3 ray = wVertexPos - _CamPos;
				float farPoint = length(ray);
				ray /= farPoint;

				//Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float nearPoint = GetNearIntersection(_CamPos, ray, _CamHeight2, _OuterRadius2);
				farPoint -= nearPoint;

				//Calculate the ray's starting position, then calculate its scattering offset
				float3 start = _CamPos + (ray * nearPoint);
				float startAngle = dot(ray, start) / _OuterRadius;
				float startDepth = exp(-(1.0 / _AverageDensity));
				float startOffset = startDepth * ScaleDepth(startAngle);

				//Initialize the scattering loop variables
				float sampleLength = farPoint / (float)sampleCount;
				float scaledSampleLength = sampleLength * _Scale;
				float3 sampleRay = ray * sampleLength;
				float3 samplePoint = start + sampleRay * 0.5;

				//Now loop through the sample rays
				fixed3 color = float3(0.0, 0.0, 0.0);
				fixed3 atten = fixed3(0.0, 0.0, 0.0);
				for(int i = 0; i < sampleCount; i++) {
					float height = length(samplePoint);
					float depth = exp(_ScaledAverageDensity * (_InnerRadius - height));
					float lightAngle = dot(_SunDirection, samplePoint) / height;
					float camAngle = dot(ray, samplePoint) / height;
					float scatter = (startOffset + depth * (ScaleDepth(lightAngle) - ScaleDepth(camAngle)));
					//atten = exp(-scatter * (_InvWaveLength * _Kr4PI + _Km4PI));
					atten = exp(-clamp(scatter, 0.0, MAX_SCATTER) * (_InvWaveLength * _Kr4PI + _Km4PI));
					color += atten * (depth * scaledSampleLength);
					samplePoint += sampleRay;
				}

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.dir = -ray;
				o.col0.rgb = color * (_InvWaveLength * _SkyDensity);
				//o.col1.rgb = color * _SunSize;
				o.col1.rgb = (color * _SunSize) * saturate(ray.y + SKY_GROUND_THRESHOLD);
				o.col0.a = atten.r;
				o.col1.a = o.col1.r;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float NdotL = min(0, dot(_SunDirection, i.dir)) / length(i.dir);
				float NdotL2 = NdotL * NdotL;
				float rayleighPhase = RayleighPhase(NdotL2);
				NdotL = dot(_FlareDirection, i.dir) / length(i.dir);
				NdotL2 = NdotL * NdotL;
				float miePhase = MiePhase(NdotL, NdotL2);
				fixed3 diff = rayleighPhase * i.col0.rgb + miePhase * i.col1.rgb;
				float alpha = pow(i.col0.a, 0.15);
				return saturate(fixed4(diff, alpha));
			}
			ENDCG 
		}
	}

	Fallback Off
}