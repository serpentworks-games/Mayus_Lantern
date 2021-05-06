Shader "Unlit/SimpleLava"
{
	Properties
	{
		[Header(Main)]
		_Color("Main Tint Start", Color) = (0.5, 0.1,0.1, 1)
		_Color2("Main Tint End", Color) = (0.6, 0.4,0.1, 1)
		_Offset("Start/End Tint Offset", Range(0,10)) = 1
		_MainTex("Main Texture", 2D) = "white" {}
		_Color3("Top Layer Tint", Color) = (1, 1,0, 1)
		_Scale("Scale Main", Range(0,1)) = 0.3
		_SpeedMainX("Speed Main X", Range(-10,10)) = 0.4
		_SpeedMainY("Speed Main Y", Range(-10,10)) = 0.4		
		_Strength("Brightness Under Lava", Range(0,10)) = 2
		_StrengthTop("Brightness Top Lava", Range(0,10)) = 3
		_Cutoff("Cutoff Top", Range(0,1)) = 0.9
		_TopBlur("Top Blur", Range(0,1)) = 0.1

		[Space(10)]
		[Header(Edge)]
		_EdgeC("Edge Color", Color) = (1, 0.5, 0.2, 1)
		_EdgeBlur("Edge Blur", Range(0,1)) = 0.5
		_Edge("Edge Thickness", Range(0,20)) = 8

		[Space(10)]
		[Header(Distortion)]
		_DistortTex("Distort Texture", 2D) = "white" {}
		_ScaleDist("Scale Distortion", Range(0,1)) = 0.5
		_SpeedDistortX("Speed Distort X", Range(-10,10)) = 0.2
		_SpeedDistortY("Speed Distort Y", Range(-10,10)) = 0.2
		_Distortion("Distort Strength", Range(0,1)) = 0.2
		_VertexDistortion("Extra Vertex Color Distortion", Range(0,1)) = 0.3
		
		[Space(10)]
		[Header(Vertex Movement)]
		_Speed("Wave Speed", Range(0,1)) = 0.5
		_Amount("Wave Amount", Range(0,1)) = 0.6
		_Height("Wave Height", Range(0,1)) = 0.1
		
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent" }
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog
#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float4 color: COLOR;
	};

	struct v2f
	{
		float2 uv : TEXCOORD3;
		UNITY_FOG_COORDS(1)
		float4 vertex : SV_POSITION;
		float4 scrPos : TEXCOORD2;//
		float4 worldPos : TEXCOORD4;//
		float4 color :COLOR;
	};

	sampler2D _CameraDepthTexture; 

	float4 _Color, _Color2, _Color3, _EdgeC;
	sampler2D _MainTex, _DistortTex;
	float _Speed, _Amount, _Height, _Edge, _Scale, _ScaleDist;
	float  _Offset, _Strength, _Distortion, _Cutoff, _StrengthTop;
	float _EdgeBlur, _VertexDistortion;
	float _SpeedDistortX, _SpeedDistortY;
	float _SpeedMainX, _SpeedMainY;
	float _TopBlur;

	v2f vert(appdata v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		
		//wave movement multiplied by vertex color
		v.vertex.y += (sin(_Time.z * _Speed + (v.vertex.x * v.vertex.z * _Amount)) * _Height) *v.color.r;

		o.vertex = UnityObjectToClipPos(v.vertex);

		// world position for textures
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);

		// vertex colors
		o.color = v.color;

		// screen position for depth
		o.scrPos = ComputeScreenPos(o.vertex);

		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		
		// uv distortion scaled
		float2 uvDistort = i.worldPos.xz * _ScaleDist;
		// moving over time
		float speedDistortX = _Time.x * _SpeedDistortX;
		float speedDistortY = _Time.x * _SpeedDistortY;
		float2 speedDistortCombined = float2(speedDistortX, speedDistortY);

		// distortion textures at different scales
		float d = tex2D(_DistortTex, uvDistort + speedDistortCombined).r;
		float d2 = tex2D(_DistortTex, (i.worldPos.xz * (_ScaleDist * 0.5)) + speedDistortCombined).r;
		// combined
		float layereddist = saturate((d + d2)*0.5 ) ;
		
		// uv main scaled
		float2 uvMain = i.worldPos.xz * _Scale;
		// plus distortion
		uvMain += layereddist * _Distortion;

		// moving over time
		float speedMainX = _Time.x * _SpeedMainX;
		float speedMainY = _Time.x * _SpeedMainY;
		float2 speedMainCombined = float2(speedMainX, speedMainY);

		// main uv moving with extra distortion based on vertex colors
		uvMain += speedMainCombined +(i.color.r * _VertexDistortion);

		// main texture fading with vertex color
		half4 col = tex2D(_MainTex, uvMain)*i.color.r;

		// add layered distortion 
		col += layereddist;
	
		// top layer
		float top = smoothstep(_Cutoff, _Cutoff + _TopBlur, col) * i.color.r;
	
		// depth edge detection
		half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); 
		half4 edgeLine = 1 - saturate(_Edge* (depth - i.scrPos.w));

		// cutoff edge based on main texture
		float edge = smoothstep(1- col , 1- col+ _EdgeBlur, edgeLine);
		
		// lerp start and end color over main texture, multiply for brightness
		float4 color = lerp(_Color, _Color2, col * _Offset) * _Strength;
		
		// take edge out of main color
		color *= (1 - edge);
		// take the top out of main color
		color *= (1 - top);
		// fade using the vertex color
		color *= i.color.r;
		// add edge back in colored, multiply for brightness
		color += (edge *_EdgeC) * _StrengthTop;

		// add top back in colored, multiply for brightness
		color += top * _Color3 *_StrengthTop;

		return color;
		}
		ENDCG
	}
	}
}