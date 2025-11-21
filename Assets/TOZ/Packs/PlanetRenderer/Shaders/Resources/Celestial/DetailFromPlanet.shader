Shader "TOZ/Celestial/DetailFromPlanet" {
	Properties {
		_ControlTex("Control Texture (RGB)", 2D) = "white" {}
		_Detail0Tex("Detail0 Texture (RGB)", 2D) = "gray" {}
		_Detail1Tex("Detail1 Texture (RGB)", 2D) = "gray" {}
	}

	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"DisableBatching" = "False"
		}
		LOD 200

		CGPROGRAM
		#define CUSTOMSUNDIRECTION
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		//#include "../../../../../Common/Shaders/_Includes/TOZ_Math.cginc"
		//noforwardadd novertexlights
		//#pragma surface surf TOZLambert fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv exclude_path:deferred exclude_path:prepass vertex:vert
		#pragma surface surf TOZLambert vertex:vert fullforwardshadows nolightmap nodynlightmap nodirlightmap nometa nolppv noshadowmask interpolateview halfasview

		sampler2D _ControlTex, _Detail0Tex, _Detail1Tex;
		float4 _Detail0Tex_ST, _Detail1Tex_ST;

		struct Input {
			float4 coord0;
			float2 coord3;
			float4 color : COLOR;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.coord0.xy = v.texcoord.xy * _Detail0Tex_ST.xy + _Detail0Tex_ST.zw;
			o.coord0.zw = v.texcoord.xy * _Detail1Tex_ST.xy + _Detail1Tex_ST.zw;
			o.coord3.xy = v.texcoord3.xy;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 diffColor = tex2D(_ControlTex, float2(IN.color.r, 0));

			fixed3 det0 = tex2D(_Detail0Tex, IN.coord0.xy).rgb;
			fixed3 det1 = tex2D(_Detail1Tex, IN.coord0.zw).rgb;
			fixed3 detail = lerp(det0, det1, smoothstep(0.0, 0.3, IN.color.b));
			diffColor.rgb *= detail * unity_ColorSpaceDouble.r;

			o.Albedo = diffColor.rgb;
			//Uncomment below; if polar snow is needed
			//o.Albedo = lerp(diffColor.rgb, fixed3(1, 1, 1), smoothstep(0.5, 0.9, IN.color.g));
			o.Alpha = diffColor.a;
		}
		ENDCG
	}

	Fallback "Diffuse"
}