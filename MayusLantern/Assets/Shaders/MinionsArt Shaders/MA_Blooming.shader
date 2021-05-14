Shader "MinionsArt/Toon/Lit Interactive Grow" {
	Properties{
		_Color("Color Away From Target", Color) = (0.5,0.5,0.5,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Color2("Color Emis", Color) = (0,0.5,1,1)
		_Mask("Emis Mask (R)", 2D) = "black" {}
		_EmisStrength("Emissive Strength", Range(1, 10)) = 5 
		_Speed("Move Speed", Range(1,50)) = 10 
		_Rigidness("Rigidness", Range(0,2)) = 10 
		_SwayMax("Sway Max", Range(0,10)) = 10 
		_YOffset("Y Offset", Range(-2,2)) = 0 
		_ShrinkXZ("Shrink Scale XZ Axis", Range(0,1)) = 0.2 
		_Shrink("Shrink Scale All", Range(0, 1)) = 0.2 

	}

		SubShader{
			Tags { "RenderType" = "Opaque"  }
			LOD 200


		CGPROGRAM
		#pragma surface surf ToonRamp vertex:vert addshadow // addshadow applies shadow after vertex animation

			// custom lighting function based
			// on angle between light direction and normal
			#pragma lighting ToonRamp exclude_path:prepass
			#pragma multi_compile_instancing
			inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
			{
				#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize(lightDir);
				#endif

				float d = dot(s.Normal, lightDir);
				float3 ramp = smoothstep(0, d , d) + 0.5f;
				half4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
				c.a = 0;
				return c;
			}

			sampler2D _MainTex,_Mask;
			float4 _Color, _Color2;
			float _Rigidness, _Speed, _SwayMax, _YOffset, _Shrink, _ShrinkXZ, _EmisStrength;

			struct Input {
				float2 uv_MainTex : TEXCOORD0;
			};

			//MaterialPropertyBlock stuff
			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float, _Moved)
			UNITY_INSTANCING_BUFFER_END(Props)


			void vert(inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input, o);			
				float4 wpos = mul(unity_ObjectToWorld, v.vertex);// world position
				
				// shrink based on proximity to target
				v.vertex.xz *= saturate(UNITY_ACCESS_INSTANCED_PROP(Props, _Moved) + _ShrinkXZ);
				v.vertex.xyz *= saturate(UNITY_ACCESS_INSTANCED_PROP(Props, _Moved) + _Shrink);
				
				// swaying
				float x = sin(wpos.x / _Rigidness + (_Time.x * _Speed)) *(v.vertex.y - _YOffset);// x axis movements
				float z = sin(wpos.z / _Rigidness + (_Time.x * _Speed)) *(v.vertex.y - _YOffset);// z axis movements
				v.vertex.x += (step(0, v.vertex.y - _YOffset) * x * _SwayMax);// apply the movement if the vertex's y above the YOffset
				v.vertex.z += (step(0, v.vertex.y - _YOffset) * z * _SwayMax);
				}

			void surf(Input IN, inout SurfaceOutput o) {
				half4 c = tex2D(_MainTex, IN.uv_MainTex);
				half4 m = tex2D(_Mask, IN.uv_MainTex);
				o.Albedo = c * _Color;
				o.Emission = (UNITY_ACCESS_INSTANCED_PROP(Props, _Moved) * _Color2 * c.rgb * _EmisStrength) * m.r ;// emission based on mask and proximity
				o.Alpha = c.a;
			}
			ENDCG
		}
			Fallback "Diffuse"
}