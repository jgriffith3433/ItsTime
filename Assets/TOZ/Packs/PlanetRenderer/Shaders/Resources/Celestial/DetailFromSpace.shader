Shader "TOZ/Celestial/DetailFromSpace" {
	Properties {
		_ControlTex("Control Texture (RGB)", 2D) = "white" {}
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
		//noshadow
		//#pragma surface surf TOZLambert noforwardadd novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv exclude_path:deferred exclude_path:prepass vertex:vert
		#pragma surface surf TOZLambert vertex:vert noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa noforwardadd nolppv noshadowmask interpolateview halfasview

		sampler2D _ControlTex;

		struct Input {
			float2 coord3;
			float4 color : COLOR;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.coord3.xy = v.texcoord3.xy;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 diffColor = tex2D(_ControlTex, float2(IN.color.r, 0));

			o.Albedo = diffColor.rgb;
			//Uncomment below; if polar snow is needed
			//o.Albedo = lerp(diffColor.rgb, fixed3(1, 1, 1), smoothstep(0.5, 0.9, IN.color.g));
			o.Alpha = diffColor.a;
		}
		ENDCG
	}

	Fallback "Diffuse"
}