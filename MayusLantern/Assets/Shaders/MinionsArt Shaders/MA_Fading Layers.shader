
Shader "MinionsArt/Toon/Fading Layers" {
	Properties{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ToonRamp("Toon Ramp Offset", Color) = (0,0,0,0) 

		_DepthMap("DepthMap", 2D) = "black" {}
		_Parallax("Parallax", Range(-0.5,0.5)) = 0.1

		_Normal("Normal", 2D) = "bump" {}

		_Iterations("Iterations", float) = 5
		_ParColor("Par COlor", Color) = (1,1,1,1)
	}

		SubShader{
		Tags{ "RenderType" = "TransparentCutout" "Queue" = "Geometry+1"  }
		LOD 200
		Cull Off
	
		CGPROGRAM

#pragma surface surf ToonRamp fullforwardshadows addshadow vertex:vert // transparency
#pragma target 3.5

		float4 _ToonRamp;

	// custom lighting function that uses a texture ramp based
	// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass
	inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
	{
#ifndef USING_DIRECTIONAL_LIGHT
		lightDir = normalize(lightDir);
#endif
		float d = dot(s.Normal, lightDir) ;
		float dChange = fwidth(d);
		float3 lightIntensity = smoothstep(0 , dChange + 0.05, d  )+ (_ToonRamp);
		
		half4 c;
		c.rgb = s.Albedo * _LightColor0.rgb * (lightIntensity) * (atten * 2);
		c.a = 0; 
		return c;
	}


	sampler2D _MainTex;
	float4 _Color;
	sampler2D _DepthMap, _Normal;
	float _Parallax;
	float _Iterations;
	float4 _ParColor;

	uniform float _DayNightEmis;
	struct Input {
		float2 uv_MainTex : TEXCOORD0;
		float2 uv_DepthMap;
		float2 uv_Normal;
		float3 viewDirTangent;


	};

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		float3 viewDir = worldPos - _WorldSpaceCameraPos;
		float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) / v.tangent.w;
		float3x3 tangentMatrix = float3x3(v.tangent.xyz, bitangent, v.normal);
		o.viewDirTangent = mul(tangentMatrix, viewDir);
		
	}

	void surf(Input IN, inout SurfaceOutput o) {
		float parallax = 0;
		for (int j = 0; j < _Iterations; j++) {
			float ratio = (float)j / _Iterations;
			parallax += tex2D(_DepthMap, IN.uv_DepthMap + lerp(0, _Parallax, ratio) * normalize(IN.viewDirTangent)) * lerp(1, 0, ratio);
		}
		parallax /= _Iterations;
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb + (parallax * _ParColor);
	
		o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));

	
	}
	ENDCG

	}

	Fallback "Transparent/Cutout/VertexLit"
}