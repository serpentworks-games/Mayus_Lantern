Shader "Toon/Lit Ice VertexColor Tessellated" {
	Properties{
		[Header(Main)]		
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)		
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}	
		[Header(Tesselation)]
		_Tess("Tessellation", Range(1,32)) = 4

		[Header(Toon Ramp)]
		_RampColor("ToonRamp Color", Color) = (0,0,0.5,1)
		_RampOffset("ToonRamp Offset", Range(0,1)) = 0.6
		_ToonRampSoftNess("ToonRamp Softness", Range(0,1)) = 0.1		

		[Header(Snow)]
		_ColorSnow("Snow Color", Color) = (0.7,1,1,1)
		_NoiseScale("Noise Scale Snow", Range(0,1)) = 1
		_SnowHeight("Snow Height", Range(0,0.1)) = 0.05
		_SnowCutoff("Snow Cutoff", Range(0.1,1)) = 0.5
		_RimColorSnow("Snow Rim Color", Color) = (0,0.6,0.8,1)
		_RimPowerSnow("Snow Rim Power", Range(0,20)) = 5

	
		[Header(Ice Colors)]
		_ColorIce("Ice Base Color", Color) = (0.2,0.5,0.5,1)
		_RimColor("Ice Rim Color", Color) = (0,0.6,0.8,1)
		_RimPowerIce("Ice Rim Power", Range(0,20)) = 5

		[Header(Ice Displacement)]	
		_Scale("Noise Scale Ice", Range(0,1)) = 1
		_IceDisplacement("Ice Displacement Cutoff", Range(-2,0)) = -1
		_Sharpness("Ice Sharpness", Range(0,1)) = 1
		_Height("Ice Length", Range(0,2)) = 0.7	
		_IceCutoff("Ice Cutoff", Range(0.1,1)) = 0.1
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200	
		Cull Off
		CGPROGRAM

		#pragma surface surf ToonRamp vertex:vert addshadow nolightmap tessellate:tessDistance		
		#pragma target 4.0
		#pragma require tessellation tessHW
		#include "Tessellation.cginc"

		float _ToonRampSoftNess,_RampOffset;
		float4 _RampColor;
		// custom lighting function that uses a texture ramp based
		// on angle between light direction and normal
		#pragma lighting ToonRamp exclude_path:prepass
		inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
		{
		#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
		#endif
			float d = dot(s.Normal, lightDir);
			float dOffset = fwidth(d) + _RampOffset;
			float ramp = smoothstep(dOffset, dOffset + _ToonRampSoftNess, d);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (ramp + _RampColor)* (atten * 2);
			c.a = s.Alpha;
			return c;
		}

		float _Tess;
		// big thanks to Cone Wars devs for explaining tesselation http://diary.conewars.com/tessellation-melt-shader-part-3/
		float MeltCalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess, float4 color)
		{
			float3 worldPosition = mul(unity_ObjectToWorld, vertex).xyz;
			float dist = distance(worldPosition, _WorldSpaceCameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0);
			
			if (color.r < 0.4 ) {
				f = 0.001;
			}				
			return f * tess; 
		}

		float4 MeltDistanceBasedTess(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tess, float4 v0c, float4 v1c, float4 v2c)
		{
			float3 f;
			f.x = MeltCalcDistanceTessFactor(v0, minDist, maxDist, tess, v0c);
			f.y = MeltCalcDistanceTessFactor(v1, minDist, maxDist, tess, v1c);
			f.z = MeltCalcDistanceTessFactor(v2, minDist, maxDist, tess, v2c);

			return UnityCalcTriEdgeTessFactors(f);
		}

		float4 tessDistance(appdata_full v0, appdata_full v1, appdata_full v2)
		{
			float minDist = 10.0;
			float maxDist = 25.0;

			return MeltDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess, v0.color, v1.color, v2.color);
		}

		sampler2D _MainTex, _Noise;
		float4 _Color, _ColorIce, _ColorSnow, _RimColor, _RimColorSnow;
		float _IceDisplacement, _Sharpness, _Height, _RimPowerIce, _RimPowerSnow;
		float _Scale, _NoiseScale, _SnowHeight;
		float _SnowCutoff, _IceCutoff, IceVertexCutoff;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float3 worldPos;
			float3 viewDir;
			float3 worldNormal; INTERNAL_DATA
			float4 color : COLOR;		
		};

		void vert(inout appdata_full v)
		{
			// world position
			float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
			// icicle texture based on world position
			float4 icicleNoise = tex2Dlod(_Noise, float4(worldPosition.xz * _Scale, 0, 0));

			float3 WorldNormal = UnityObjectToWorldNormal(v.normal);		
			// facing bottom
			float3 Bottom = mul(float3(0,-1,0), unity_ObjectToWorld);
			// smooth falloff of parts facing bottom
			float edge = smoothstep(WorldNormal.y, WorldNormal.y + _Sharpness, _IceDisplacement *icicleNoise.r);
			// pull down icicles
			v.vertex.xyz += (Bottom  * edge * v.color.r) * _Height;
			// move vertices outward where snow is
			v.vertex.xyz += normalize(v.normal) * (saturate(v.color.r * (1-edge)* _SnowHeight * icicleNoise.r));
		}
	
		void surf(Input IN, inout SurfaceOutput o) {			
			// triplanar noise
			float3 blendNormal = saturate(pow(IN.worldNormal * 1.4, 4));

			// normal noise triplanar for x, y, z sides
			float3 xn = tex2D(_Noise, IN.worldPos.zy * _NoiseScale);
			float3 yn = tex2D(_Noise, IN.worldPos.zx * _NoiseScale);
			float3 zn = tex2D(_Noise, IN.worldPos.xy * _NoiseScale);

			// lerped together all sides for noise texture
			float3 noisetexture = zn;
			noisetexture = lerp(noisetexture, xn, blendNormal.x);
			noisetexture = lerp(noisetexture, yn, blendNormal.y);
			
			// main texture
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color; 

			// part of model facing bottom
			float facingBottom =1- smoothstep(_IceCutoff * noisetexture.r, (_IceCutoff + 0.01)* noisetexture.r, IN.worldNormal.y);
			// part of model with red vertex color
			float snowEdge = step(_SnowCutoff * noisetexture.r, IN.color.r);

			// base texture, not on red vertex color
			float3 baseAlbedo = c.rgb * (1- snowEdge);

			// snow , only on red vertex color, not on bottom facing part
			float3 snowAlbedo = _ColorSnow * (1 - facingBottom) * snowEdge;
			
			// ice, only on red vertex color, and bottom facing part
			float3 iceAlbedo = _ColorIce * facingBottom * snowEdge; 
			// combined
			o.Albedo = baseAlbedo + snowAlbedo + iceAlbedo;

			// rim lighting
			half rimTextured = 1.0 - (saturate(dot(normalize(IN.viewDir), o.Normal)) * noisetexture.r); //+ IN.test.r; // rimlight
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)); //+ IN.test.r; // rimlight

			// ice rim
			float3 iceRim = _RimColor.rgb *pow(rimTextured, _RimPowerIce)* facingBottom* (step(0.25, IN.color.r));
			// snow rim
			float3 snowRim = _RimColorSnow.rgb *pow(rim, _RimPowerSnow) * (1 - facingBottom) * (step(0.25, IN.color.r));
			//combined
			o.Emission = snowRim + iceRim ;// add glow rimlight to snow
			
			o.Alpha = 1;
		}
		ENDCG

		}

			Fallback "Diffuse"
}
