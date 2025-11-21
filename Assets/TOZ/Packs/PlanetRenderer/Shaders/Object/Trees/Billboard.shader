Shader "TOZ/Object/Trees/Billboard" {
	Properties {
		[NoScaleOffset] _MainTex("Main Texture (RGB)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}

	SubShader {
		Tags {
			"RenderType" = "TreeTransparentCutout"
			"Queue" = "AlphaTest"
			"IgnoreProjector"="True"
			"DisableBatching"="True"
		}
		LOD 200
		Cull Off
		//ZWrite Off
		ColorMask RGB

		CGPROGRAM
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		//noambient novertexlights
		#pragma surface surf TOZLambert alphatest:_Cutoff addshadow nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask interpolateview halfasview dithercrossfade

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 diffColor = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = diffColor.rgb;
			o.Alpha = diffColor.a;
		}
		ENDCG
	}

	Fallback Off
}