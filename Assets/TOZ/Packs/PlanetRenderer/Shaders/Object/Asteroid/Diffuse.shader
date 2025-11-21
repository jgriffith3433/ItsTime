Shader "TOZ/Object/Asteroid/Diffuse" {
	Properties {
		_Color("Tint Color (RGB)", Color) = (1, 1, 1, 1)
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "DisableBatching"="True" }
		LOD 200

		CGPROGRAM
		//#define CUSTOMSUNDIRECTION
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		//noambient novertexlights
		#pragma surface surf TOZLambert noshadow nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask interpolateview halfasview dithercrossfade

		fixed4 _Color;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 diffColor = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = diffColor.rgb * _Color.rgb;
			o.Alpha = diffColor.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/VertexLit"
}