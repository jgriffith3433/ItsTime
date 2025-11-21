Shader "TOZ/Celestial/SplatFromSpace" {
	Properties {
		_ControlTex("Control Texture (RGBA)", 2D) = "red" {}
		_Splat0("Layer 0 (R)", 2D) = "red" {}
		_Splat1("Layer 1 (G)", 2D) = "green" {}
		_Splat2("Layer 2 (B)", 2D) = "blue" {}
		_Splat3("Layer 3 (A)", 2D) = "black" {}
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "DisableBatching" = "False" }
		LOD 200

		CGPROGRAM
		#define CUSTOMSUNDIRECTION
		#include "../../../../../Common/Shaders/_Includes/TOZ_Lighting.cginc"
		//noshadow
		//#pragma surface surf TOZLambert vertex:vert addshadow noforwardadd novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv exclude_path:deferred exclude_path:prepass
		#pragma surface surf TOZLambert vertex:vert noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa noforwardadd nolppv noshadowmask interpolateview halfasview

		sampler2D _ControlTex;
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

		struct Input {
			fixed4 color : COLOR0;
			float4 coord0 : TEXCOORD0;
			float4 coord1 : TEXCOORD1;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
			o.coord0.xy = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
			o.coord0.zw = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
			o.coord1.xy = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
			o.coord1.zw = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			//Diffuse
			fixed4 control = tex2D(_ControlTex, float2(IN.color.r, 0));
			control.rg = saturate(control.rg - IN.color.b);
			control.b = saturate(control.b + IN.color.b);

			fixed3 sand = tex2D(_Splat0, IN.coord0.xy).rgb;
			fixed3 grass = tex2D(_Splat1, IN.coord0.zw).rgb;
			fixed3 rock = tex2D(_Splat2, IN.coord1.xy).rgb;
			fixed3 snow = tex2D(_Splat3, IN.coord1.zw).rgb;
			
			fixed4 diffColor = fixed4(0, 0, 0, 1);
			diffColor.rgb = control.r * sand;
			diffColor.rgb += control.g * grass;
			diffColor.rgb += control.b * rock;
			diffColor.rgb += control.a * snow;

			o.Albedo = diffColor.rgb;
			//Uncomment below; if polar snow is needed
			//o.Albedo = lerp(diffColor.rgb, fixed3(1, 1, 1), grade(0.0, 1.0, 0.5, 0.9, IN.color.g));
			o.Alpha = diffColor.a;
		}
		ENDCG
	}

	Fallback "Diffuse"
}