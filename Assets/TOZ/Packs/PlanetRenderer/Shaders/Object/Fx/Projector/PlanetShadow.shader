Shader "TOZ/Object/Fx/Projector/PlanetShadow" {
	Properties {
		_Color("Ambient Color", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex("Cookie", 2D) = "gray" {}
		[NoScaleOffset]_FalloffTex("FallOff", 2D) = "white" {}

		[Space]
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 2
		[Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTest Mode", Int) = 4
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Blend Src", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Blend Dst", Int) = 10
		[Enum(None,0, A,1, R,8, G,4, B,2, RGB,14, RGBA,15)]_ColorMask("Color Mask", Float) = 14
	}

	Subshader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			Cull [_CullMode]
			ZWrite [_ZWriteMode]
			ZTest [_ZTestMode]
			Blend [_BlendSrc] [_BlendDst]
			ColorMask [_ColorMask]
			Offset -1, -1
			/*
			ZWrite Off
			ColorMask RGB
			Blend DstColor SrcColor
			Offset -1, -1
*/
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _FalloffTex;
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			struct a2v {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 coord0 : TEXCOORD0;
				float4 coord1 : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.coord0 = mul(unity_Projector, v.vertex);
				o.coord1 = mul(unity_ProjectorClip, v.vertex);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.coord0));
				col.rgb = (1.0 - col.rgb) * _Color.rgb;
				col.a *= _Color.a;

				fixed4 falloff = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.coord1));
				fixed4 result = col * falloff.a;

				UNITY_APPLY_FOG(i.fogCoord, result);
				return result;
			}
			ENDCG
		}
	}

	Fallback Off
}