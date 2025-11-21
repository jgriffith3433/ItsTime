Shader "TOZ/Celestial/CloudsFromSpace" {
	Properties {
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Transparent-400"
			"IgnoreProjector" = "True"
			"PreviewType" = "Skybox"
		}
		LOD 200

		Cull Back
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			uniform float4 _SunDirection;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 cPos : SV_POSITION;
				float2 coord : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			v2f vert(a2v v) {
				v2f o;
				o.cPos = UnityObjectToClipPos(v.vertex);
				o.coord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				//Diffuse Term
				fixed NdotL = max(dot(i.normal, _SunDirection.xyz), 0.0);
				fixed diff = max(NdotL, UNITY_LIGHTMODEL_AMBIENT);

				//Result
				fixed4 result = tex2D(_MainTex, i.coord);
				result.rgb *= diff;

				return result;
			}
			ENDCG
		}
	}

	FallBack Off
}