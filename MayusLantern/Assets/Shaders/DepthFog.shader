Shader "Unlit/DepthFake"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, .5)
		_Offset("Offset", Range(0,3)) = 0.5
	

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque"  "Queue" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

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
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
					float4 scrPos : TEXCOORD1;//
				};

				float4 _Tint;
				uniform sampler2D _CameraDepthTexture; //Depth Texture
				sampler2D _MainTex;//
				float4 _MainTex_ST;
				float  _Offset;//

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.scrPos = ComputeScreenPos(o.vertex); // grab position on screen
					UNITY_TRANSFER_FOG(o,o.vertex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					
					half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); // depth
					half4 foamLine = (_Offset * (depth - i.scrPos.w));// foam line by comparing depth and screenposition
					
					half4 col = foamLine* _Tint;
					col = saturate(col);// clamp to prevent weird artifacts

					return col;
				}
				ENDCG
			}
		}
}