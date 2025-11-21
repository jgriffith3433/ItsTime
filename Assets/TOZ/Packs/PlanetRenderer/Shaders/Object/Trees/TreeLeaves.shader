Shader "TOZ/Object/Trees/TreeLeaves" {
	Properties {
		[NoScaleOffset] _MainTex("Main Texture (RGB)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0, 1)) = 0.5
		_LightWrapAmount("Light Wrap", Range(0, 1)) = 0.5
		_SSSThickness("Translucency", Range(0, 1)) = 1.0
		_C1("Dead Color (RGB)", Color) = (1, 0, 0, 1)
		_C2("Average Color (RGB)", Color) = (1, 1, 0, 1)
		_C3("Young Color (RGB)", Color) = (0, 1, 0, 1)
	}

	SubShader {
		Tags {
			"RenderType" = "TreeTransparentCutout"
			"Queue" = "AlphaTest"
			"DisableBatching" = "True"
			"IgnoreProjector" = "True"
		}
		LOD 200
		Cull Off
		ColorMask RGB

		CGPROGRAM
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		#include "../../../../../Common/Shaders/_Includes/TOZ_Math.cginc"
		#include "../../Includes/TOZ_Nature.cginc"
		//noambient novertexlights
		//#pragma surface surf TOZLambertWrap alphatest:_Cutoff vertex:vert addshadow nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask interpolateview halfasview dithercrossfade
		#pragma surface surf TOZLeaf alphatest:_Cutoff vertex:vert addshadow nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask dithercrossfade //noshadow

		fixed4 _Color, _C1, _C2, _C3;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR; // color.a = AO
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.vertex = AnimateVertex(v.vertex, v.normal, v.color.xyzw);
			v.color.rgb = interp3(_C1, _C2, _C3, v.color.y); //Leaf variations
		}

		void surf (Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = c.rgb * IN.color.rgb; // you can multiply with IN.color.a to get AO
			o.Alpha = c.a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}