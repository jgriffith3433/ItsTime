Shader "TOZ/Celestial/AtmosphereFromPlanet" {
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent-401" "IgnoreProjector" = "True" }
		LOD 100

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			Cull Front
			ZWrite Off
			Blend One One
			//Blend SrcColor OneMinusSrcColor
			//Blend One SrcColor

			CGPROGRAM
			#include "../../Includes/TOZ_Atmosphere.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

		inline float remap( float a, float b, float min, float max, float t ) {
			return lerp(a, b, smoothstep(min, max, t));
		}

			v2f vert(a2v v) {
				//Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 wCenterPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float3 wVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz - wCenterPos;

				float3 ray = wVertexPos - _CamPos;
				float farPoint = length(ray);
				ray /= farPoint;

				//Calculate the ray's starting position, then calculate its scattering offset
				float3 start = _CamPos;
				float startAngle = dot(ray, start) / length(start);
				float startDepth = exp(_ScaledAverageDensity * (_InnerRadius - _CamHeight));
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
	
				float3 cDir = normalize(_WorldSpaceCameraPos - wCenterPos);
				float3 vDir = normalize(wVertexPos);
				o.CDotV = max(0.0, dot(vDir, cDir));

				//UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float NdotL = min(0, dot(_SunDirection, i.dir)) / length(i.dir);
				float NdotL2 = NdotL * NdotL;
				float rayleighPhase = RayleighPhase(NdotL2);
				NdotL = dot(_FlareDirection, i.dir) / length(i.dir);
				NdotL2 = NdotL * NdotL;
				//float miePhase = MiePhase(NdotL, NdotL2);
				//fixed3 diff = rayleighPhase * i.col0.rgb + miePhase * i.col1.rgb;
				fixed3 diff = rayleighPhase * i.col0.rgb;
				half cdotv = 1.0-remap(0.0, 1.0, 0.95, 1.0, i.CDotV);
				//diff = lerp(diff, unity_FogColor.rgb, cdotv);
				//diff = lerp(diff, fixed3(1,0,0), remap(1.0, 0.0, 0.0, 0.1, NdotL));
				float alpha = pow(i.col0.a, 0.15);
				//UNITY_APPLY_FOG(i.fogCoord, diff);
				return saturate(fixed4(diff, alpha));
			}
			ENDCG 
		}
	}

	Fallback Off
}