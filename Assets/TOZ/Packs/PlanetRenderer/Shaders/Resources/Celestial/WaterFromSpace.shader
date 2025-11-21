Shader "TOZ/Celestial/WaterFromSpace" {
	Properties {
		[Header(Color Properties)]
		_Color("Tint Color (RGB)", Color) = (0, 0.15, 0.115, 1)
		_ShallowColor("Shallow Color", Color) = (0.502, 1.0, 1.0, 1.0)

		[Space]
		_Shininess("_Shininess", Range (0.1, 120.0)) = 15.0
	}

	SubShader {
		Tags { 
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"IgnoreProjector" = "True"
		}
		LOD 200

		Cull Back
		//ZWrite Off

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			uniform half3 _SunDirection;
			uniform half3 _StarColor;

			fixed4 _Color;
			fixed4 _ShallowColor;

			float _Shininess;

			struct a2v {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float3 normal : TEXCOORD0;
				float3 eye : TEXCOORD1;
			};

			v2f vert (a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.eye = WorldSpaceViewDir(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				//Projections
				float3 normal = normalize(i.normal);
				float3 eye = normalize(i.eye);
				float3 reflRay = reflect(-eye, normal);

				//Water color
				fixed3 result = _Color.rgb * _ShallowColor.rgb;

				//Uncomment below; if polar snow is needed
				//result = lerp(result, fixed3(1, 1, 1), pow(i.color.g, 10.0));

				//Diffuse Term
				half NdotL = dot(normal, _SunDirection.xyz);
				half diff = max(NdotL, UNITY_LIGHTMODEL_AMBIENT);
				result *= diff;

				//Specular term Phong (wrong angles underwater, good speculars)
				float LdotR = dot(_SunDirection.xyz, reflRay);
				float spec = pow(max(LdotR, 0), _Shininess);
				result += _StarColor.rgb * 0.5 * spec;

				//Result
				return fixed4(result, 1.0);
			}
			ENDCG
		}
	}

	FallBack Off
}