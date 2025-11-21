Shader "TOZ/Object/Trees/TreeBark" {
	Properties {
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		_LightWrapAmount("Light Wrap", Range(0, 1)) = 0.5
	}

	SubShader {
		Tags {
			"RenderType" = "TreeOpaque"
			"Queue" = "Geometry"
			"IgnoreProjector"="True"
			"DisableBatching"="True"
		}
		LOD 200

		CGPROGRAM
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		#include "../../Includes/TOZ_Nature.cginc"
		//noambient novertexlights
		#pragma surface surf TOZLambertWrap vertex:vert addshadow nolightmap nodynlightmap nodirlightmap nometa noforwardadd nolppv noshadowmask interpolateview halfasview

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR; // color.a = AO
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.vertex = AnimateVertex(v.vertex, v.normal, half4(0, 0, 0, v.color.w)); //Bending only
			v.color.rgb = fixed3(1, 1, 1);
		}

		void surf(Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = c.rgb; // you can multiply with IN.color.a to get AO
			o.Alpha = c.a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}