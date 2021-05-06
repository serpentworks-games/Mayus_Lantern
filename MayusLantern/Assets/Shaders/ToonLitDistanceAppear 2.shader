Shader "Toon/Lit Distance Rotated" {
	Properties{
		_Color("Color Primary", Color) = (0.5,0.5,0.5,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Offset("Offset", Range(0,10)) = 1 
		_Bobbing("Bobbing Up&Down", Range(0,10)) = 1 
		[Toggle(X_ON)] _X_ON("X axis Offset (Y if False)", Int) = 0
		_AngleY("Angle Y", Range(0,360)) = 10 
		_AngleZ("Angle Z", Range(0,360)) = 10 
		_Noise("Noise Texture", 2D) = "white" {}
		_Color2("Secondary Color", Color) = (1,1,1,1)
		_SecondTex("Secondary (RGB)", 2D) = "white" {}
		_ScrollSpeed("MoveSpeed", Range(0,20)) = 10 
		_NScale("Noise Scale", Range(0, 10)) = 1
		_DisAmount("Noise Texture Opacity", Range(0.01, 1)) = 0.01
		_Radius("Radius", Range(0, 10)) = 0
		_DisLineWidth("Line Width", Range(0, 2)) = 0
		_DisLineColor("Line Tint", Color) = (1,1,1,1)

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
		 #pragma shader_feature X_ON
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

			// rotation from the unity skybox shader
			  float3 RotateAroundYInDegrees(float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}

			  // set up for Z axis
			   float3 RotateAroundZInDegrees(float3 vertex, float degrees)
			 {
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xy), vertex.z).zxy;
			 }

			sampler2D _MainTex,_Noise,_SecondTex;
			float _Offset;
			float4 _Color, _Color2;
			float _AngleY, _AngleZ;

			float _DisAmount, _NScale;
			float _DisLineWidth;
			float4 _DisLineColor;
			float _Radius, _ScrollSpeed, _Bobbing;


			float3 _PositionMoving; // from script

			struct Input {
				float2 uv_MainTex : TEXCOORD0;
				float3 worldPos;// built in value to use the world space position
				float3 worldNormal; // built in value for world normal
			};

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float, _Moved)
			UNITY_DEFINE_INSTANCED_PROP(float, _RandomOffset)
			UNITY_INSTANCING_BUFFER_END(Props)

			void vert(inout appdata_full v)//
			{
				float awayFromPlayer = 1 - UNITY_ACCESS_INSTANCED_PROP(Props, _Moved);
				float offsetRandomise = UNITY_ACCESS_INSTANCED_PROP(Props, _RandomOffset) * _Offset;

				// up and down movement
				v.vertex.y += sin(_Time.y* UNITY_ACCESS_INSTANCED_PROP(Props, _RandomOffset)) * _Bobbing * awayFromPlayer; 

				// offset
#if X_ON
				 v.vertex.x += offsetRandomise - UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * offsetRandomise);
#else
				 v.vertex.y += offsetRandomise - UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * offsetRandomise);
#endif


				 // rotation with randomised angle
				 float angleY = _AngleY * UNITY_ACCESS_INSTANCED_PROP(Props, _RandomOffset);
				 float angleWhileAwayY = angleY * awayFromPlayer;

				 float angleZ = _AngleZ * UNITY_ACCESS_INSTANCED_PROP(Props, _RandomOffset);
				 float angleWhileAwayZ = angleZ * awayFromPlayer;

				 v.vertex.xyz = RotateAroundYInDegrees(RotateAroundZInDegrees(v.vertex.xyz, -90 + angleWhileAwayZ),90 + angleWhileAwayY);

			}

			 void surf(Input IN, inout SurfaceOutput o) {
				 half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				 half4 c2 = tex2D(_SecondTex, IN.uv_MainTex) * _Color2;

				 float speed = _Time.x * _ScrollSpeed;

					 // triplanar noise
				 float3 blendNormal = saturate(pow(IN.worldNormal * 1.4, 4));
				 half4 nSide1 = tex2D(_Noise, (IN.worldPos.xy + speed) * _NScale);
				 half4 nSide2 = tex2D(_Noise, (IN.worldPos.xz + speed) * _NScale);
				 half4 nTop = tex2D(_Noise, (IN.worldPos.yz + speed) * _NScale);

				 float3 noisetexture = nSide1;
				 noisetexture = lerp(noisetexture, nTop, blendNormal.x);
				 noisetexture = lerp(noisetexture, nSide2, blendNormal.y);

					 // distance influencer position to world position
				 float3 dis = distance(_PositionMoving, IN.worldPos);
				 float3 sphere = 1 - saturate(dis / _Radius);

				 float3 sphereNoise = noisetexture.r * sphere;

				 float3 DissolveLine = step(sphereNoise - _DisLineWidth, _DisAmount) * step(_DisAmount, sphereNoise); // line between two textures
				 DissolveLine *= _DisLineColor; // color the line

				 float3 primaryTex = (step(sphereNoise - _DisLineWidth, _DisAmount) * c.rgb);
				 float3 secondaryTex = (step(_DisAmount, sphereNoise) * c2.rgb);
				 float3 resultTex = primaryTex + secondaryTex + DissolveLine;
				 o.Albedo = resultTex;

				 o.Emission = DissolveLine * 2;
				 o.Alpha = c.a;
				}
			 ENDCG
	}
		Fallback "Diffuse"
}