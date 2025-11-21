Shader "TOZ/Celestial/WaterFromPlanet" {
	Properties {
		_Color("Tint Color (RGB)", Color) = (0, 0.15, 0.115, 1)
		_ShallowColor("Shallow Color", Color) = (0.502, 1.0, 1.0, 1.0)

		[Space]
		[Header(Bumpmap Properties)]
		_WaveTex("Wave Texture", 2D) = "bump" {}
		_WavePower("Wave Power", Range(0.0, 1.0)) = 0.5
		_WaveSpeed("Wave Speed", Range(0.0, 0.1)) = 0.01

		[Space]
		[Header(Fresnel Properties)]
		_FresnelPower("Fresnel Power", Range(0.0, 10.0)) = 2.5
		_FresnelCutOff("Fresnel Cutoff", Range(0.0, 10.0)) = 5.0

		[Space]
		_Shore("Shore", Range(0.0, 1.0)) = 0.01
		_Shallow("Shallow", Range(0.0001, 1.0)) = 0.5

		[Space]
		[NoScaleOffset]_CubeTex("Reflection Texture (RGB)", Cube) = "" {}

		[Space]
		_RefractionTex("Refraction Texture (RGB)", 2D) = "white" {}

		[Space]
		_Shininess("_Shininess", Range (0.1, 120.0)) = 50.0
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
			#pragma target 3.0
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			uniform half3 _SunDirection;
			uniform half3 _StarColor;

			fixed4 _Color;
			fixed4 _ShallowColor;

			float _WavePower, _WaveSpeed;

			float _FresnelPower, _FresnelCutOff;

			half _Shore, _Shallow;

			UNITY_DECLARE_TEXCUBE(_CubeTex);

			sampler2D _RefractionTex;
			float4 _RefractionTex_ST;

			sampler2D _WaveTex;
			float4 _WaveTex_ST;

			float _Shininess;

			struct a2v {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float4 coord0 : TEXCOORD0; 	// Bump Textures
				float4 TBNW0 : TEXCOORD1;
				float4 TBNW1 : TEXCOORD2;
				float4 TBNW2 : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
				float2 coord1 : TEXCOORD5; 	// Refraction(xy)
				UNITY_FOG_COORDS(6)
			};

			v2f vert (a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

				float2 uv = TRANSFORM_TEX(v.texcoord, _WaveTex);
				float offset = _Time.y * _WaveSpeed;
				o.coord0.xy = uv + offset;
				o.coord0.zw = uv - offset;

				float3 pW = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 nW = UnityObjectToWorldNormal(v.normal);
				fixed3 tW = UnityObjectToWorldDir(v.tangent.xyz);
				fixed3 bW = cross(nW, tW) * (v.tangent.w * unity_WorldTransformParams.w);
				o.TBNW0 = float4(tW.x, bW.x, nW.x, pW.x);
				o.TBNW1 = float4(tW.y, bW.y, nW.y, pW.y);
				o.TBNW2 = float4(tW.z, bW.z, nW.z, pW.z);

				o.viewDir = WorldSpaceViewDir(v.vertex);

				o.coord1 = TRANSFORM_TEX(v.texcoord, _RefractionTex);

				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				//Normals
				half4 bump = tex2D(_WaveTex, i.coord0.xy) + tex2D(_WaveTex, i.coord0.zw);
				fixed3 normal = UnpackNormal(bump * 0.5);
				fixed3 normalW;
				normalW.x = dot(i.TBNW0.xyz, normal);
				normalW.y = dot(i.TBNW1.xyz, normal);
				normalW.z = dot(i.TBNW2.xyz, normal);

				//Projections
				float3 viewDir = normalize(i.viewDir);
				float EdotN = max(0.0, dot(viewDir, normalW));
				float3 reflRay = reflect(-viewDir, normalW);

				//Depth
				half depth = 1.0-i.color.r;
				half shores = smoothstep(0, _Shore, depth);
				half shallows = 1.0 - smoothstep(_Shallow + _Shore, _Shore, depth);
				
				//Fresnel
				half fresnel = (1.0 - max(0.0, min(1.0, saturate(pow(1.0 - EdotN, _FresnelPower) * _FresnelCutOff))));
				shallows *= 1.0-fresnel;

				//Water color
				fixed3 refl = UNITY_SAMPLE_TEXCUBE(_CubeTex, reflRay).rgb;
				fixed3 refr = tex2D(_RefractionTex, i.coord1 + normal.xy * _WavePower * shores).rgb;

				fixed3 shoreCol = fixed3(1,1,1) * refr;
				fixed3 deepCol = _Color * refl;
				fixed3 shallowCol = _ShallowColor * ((refr * 4.0 + refl * 1.0) / 5.0);
				fixed3 result = lerp(shoreCol, lerp(shallowCol, deepCol, shallows), shores);

				//Uncomment below; if polar snow is needed
				//result = lerp(result, fixed3(1, 1, 1), pow(saturate(i.color.g), 10.0));

				//Diffuse Term
				half NdotL = dot(normalW, _SunDirection.xyz);
				half diff = max(NdotL, UNITY_LIGHTMODEL_AMBIENT);
				result *= diff;

				//Specular term Phong (wrong angles underwater, good speculars)
				float LdotR = dot(_SunDirection.xyz, reflRay);
				float spec = pow(max(LdotR, 0), _Shininess);
				result += _StarColor.rgb * 0.5 * spec;

				//Result
				UNITY_APPLY_FOG(i.fogCoord, result);
				return fixed4(result, 1.0);
			}
			ENDCG
		}
	}

	FallBack Off
}