Shader "Toon/Lit TriPlanar Snow" {
	Properties{
		[Header(Main)]	
		_Noise("Snow Noise", 2D) = "gray" {}	
		_NoiseScale("Noise Scale", Range(0,2)) = 0.1
		_NoiseWeight("Noise Weight", Range(0,2)) = 0.1
		_ToonRamp("Main Color", Color) = (0.5,0.5,0.5,1)
		_Mask("Mask", 2D) = "white" {}// This mask prevents bleeding of the RT, make it white with a transparent edge around all sides, set to CLAMP, not repeat
		[Space]
		[Header(Tesselation)]
		_MaxTessDistance("Max Tessellation Distance", Range(10,100)) = 50
		_Tess("Tessellation", Range(1,32)) = 20
		[Space]
		[Header(Snow)]
		_Color("Snow Color", Color) = (0.5,0.5,0.5,1)
		_PathColor("Snow Path Color", Color) = (0.5,0.5,0.7,1)
		_MainTex("Snow Texture", 2D) = "white" {}		
		_SnowHeight("Snow Height", Range(0,2)) = 0.3
		_SnowDepth("Snow Path Depth", Range(-2,2)) = 0.3
		_EdgeColor("Snow Edge Color", Color) = (0.5,0.5,0.5,1)
		_Edgewidth("Snow Edge Width", Range(0,0.2)) = 0.1
		_SnowTextureOpacity("Snow Texture Opacity", Range(0,2)) = 0.3
		_SnowTextureScale("Snow Texture Scale", Range(0,2)) = 0.3
		[Space]
		[Header(Sparkles)]
		_SparkleScale("Sparkle Scale", Range(0,10)) = 10
		_SparkCutoff("Sparkle Cutoff", Range(0,10)) = 0.1
		_SparkleNoise("Sparkle Noise", 2D) = "gray" {}
		[Space]
		[Header(Extra Textures)]
		_MainTexBase("Base Texture", 2D) = "white" {}
		_Scale("Base Scale", Range(0,2)) = 1
		[Space]
		[Header(Rim)]
		_BaseColor("Base Color", Color) = (0.5,0.5,0.5,1)
		_RimPower("Rim Power", Range(0,20)) = 20
		_RimColor("Rim Color Snow", Color) = (0.5,0.5,0.5,1)
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM


			// custom lighting function that uses a texture ramp based
			// on angle between light direction and normal
	#pragma surface surf ToonRamp vertex:vert addshadow nolightmap tessellate:tessDistance fullforwardshadows
	#pragma target 4.0
	#pragma require tessellation tessHW
	#include "Tessellation.cginc"
			float4 _ToonRamp;
			inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
			{
		#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize(lightDir);
		#endif
				float d = dot(s.Normal, lightDir);			
				float3 ramp = smoothstep(0, d + 0.06, d) + _ToonRamp;

				half4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * (ramp) * (atten * 2);
				c.a = 0;
				return c;
			}
			uniform float3 _Position;
			uniform sampler2D _GlobalEffectRT;
			uniform float _OrthographicCamSize;

			float _Tess;
			float _MaxTessDistance;
			// big thanks to Cone Wars devs for explaining tesselation http://diary.conewars.com/tessellation-melt-shader-part-3/
			float ColorCalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess, float4 color)
			{
				float3 worldPosition = mul(unity_ObjectToWorld, vertex).xyz;
				float dist = distance(worldPosition, _WorldSpaceCameraPos);
				float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0);

				if (color.r < 0.4) {
					f = 0.001;
				}
				return f * tess;
			}

			float4 ColorDistanceBasedTess(float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tess, float4 v0c, float4 v1c, float4 v2c)
			{
				float3 f;
				f.x = ColorCalcDistanceTessFactor(v0, minDist, maxDist, tess, v0c);
				f.y = ColorCalcDistanceTessFactor(v1, minDist, maxDist, tess, v1c);
				f.z = ColorCalcDistanceTessFactor(v2, minDist, maxDist, tess, v2c);

				return UnityCalcTriEdgeTessFactors(f);
			}

			float4 tessDistance(appdata_full v0, appdata_full v1, appdata_full v2)
			{
				float minDist = 10.0;
				float maxDist = _MaxTessDistance;

				return ColorDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess, v0.color, v1.color, v2.color);
			}

			sampler2D _MainTex, _MainTexBase, _Noise, _SparkleNoise;
			float4 _Color, _RimColor;
			float _RimPower;
			float _Scale, _SnowTextureScale, _NoiseScale;
			float4 _EdgeColor;
			float _Edgewidth;
			float _SnowHeight, _SnowDepth;
			float4 _PathColor, _BaseColor;
			sampler2D _Mask;
			float _NoiseWeight;
			float _SparkleScale, _SparkCutoff;
			float _SnowTextureOpacity;

			struct Input {
				float2 uv_MainTex : TEXCOORD0;
				float3 worldPos; // world position built-in value
				float3 viewDir;// view direction built-in value we're using for rimlight
				float4 vertexColor : COLOR;
				float4 screenPos;// screen position built-in value
			};

			void vert(inout appdata_full v)
			{	
				
				float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
				// Effects RenderTexture Reading
				float2 uv = worldPosition.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;
				// Mask to prevent bleeding
				float mask = tex2Dlod(_Mask, float4(uv , 0,0)).a;
				float4 RTEffect = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
				RTEffect *= mask;
				// Snow Noise in worldSpace
				float SnowNoise = tex2Dlod(_Noise, float4(worldPosition.xz * _NoiseScale * 5, 0, 0));
			
				// move vertices up where snow is
				v.vertex.xyz += normalize(v.normal) * (saturate((v.color.r * _SnowHeight) + (SnowNoise * _NoiseWeight * v.color.r)));
				// move down where there is a trail from the render texture particles
				v.vertex.xyz -= normalize(v.normal) * (RTEffect.g * saturate(v.color.r)) *  _SnowDepth;

			}



			void surf(Input IN, inout SurfaceOutput o) {
				// Effects RenderTexture Reading
				float2 uv = IN.worldPos.xz - _Position.xz;
				uv /= (_OrthographicCamSize * 2);
				uv += 0.5;

				float mask = tex2D(_Mask, uv).a;
				float4 effect = tex2D(_GlobalEffectRT, float2 (uv.x, uv.y));
				effect *= mask;

				// clamp (saturate) and increase(pow) the worldnormal value to use as a blend between the projected textures
				float3 blendNormal = saturate(pow(WorldNormalVector(IN, o.Normal) * 1.4,4));

				// normal noise triplanar for x, y, z sides
				float3 xn = tex2D(_Noise, IN.worldPos.zy * _NoiseScale);
				float3 yn = tex2D(_Noise, IN.worldPos.zx * _NoiseScale);
				float3 zn = tex2D(_Noise, IN.worldPos.xy * _NoiseScale);

				// lerped together all sides for noise texture
				float3 noisetexture = zn;
				noisetexture = lerp(noisetexture, xn, blendNormal.x);
				noisetexture = lerp(noisetexture, yn, blendNormal.y);

				// triplanar for snow texture for x, y, z sides
				float3 xm = tex2D(_MainTex, IN.worldPos.zy * _SnowTextureScale);
				float3 zm = tex2D(_MainTex, IN.worldPos.xy * _SnowTextureScale);
				float3 ym = tex2D(_MainTex, IN.worldPos.zx * _SnowTextureScale);

				// lerped together all sides for snow texture
				float3 snowtexture = zm;
				snowtexture = lerp(snowtexture, xm, blendNormal.x);
				snowtexture = lerp(snowtexture, ym, blendNormal.y);

				// triplanar for base texture, x,y,z sides
				float3 x = tex2D(_MainTexBase, IN.worldPos.zy * _Scale);
				float3 y = tex2D(_MainTexBase, IN.worldPos.zx * _Scale);
				float3 z = tex2D(_MainTexBase, IN.worldPos.xy * _Scale);

				// lerped together all sides for base texture
				float3 baseTexture = z;
				baseTexture = lerp(baseTexture, x, blendNormal.x);
				baseTexture = lerp(baseTexture, y, blendNormal.y);

				// rim light for snow, blending in the noise texture 
				half rim = 1.0 - dot((IN.viewDir), BlendNormals(o.Normal, noisetexture));

				
				// primary texture only on red vertex color with the noise texture
				float vertexColoredPrimary = step(0.6* noisetexture,IN.vertexColor.r);
				float3 snowTextureResult = vertexColoredPrimary * snowtexture;

				// edge for primary texture
				float vertexColorEdge = (step((0.6 - _Edgewidth)* noisetexture,IN.vertexColor.r)) * (1 - vertexColoredPrimary);
				// edge for secondary texture

				// basetexture only where there is no red vertex paint
				float3 baseTextureResult = baseTexture * (1 - (vertexColoredPrimary + vertexColorEdge));

				// final albedo color by adding everything together
				float3 mainColors = (baseTextureResult *  _BaseColor) + ((snowTextureResult * _SnowTextureOpacity)+ (vertexColoredPrimary * _Color)) + (vertexColorEdge * _EdgeColor);
				//lerp the colors using the RT effect path 
				o.Albedo = lerp(mainColors, _PathColor * effect.g, saturate(effect.g * 2 * vertexColoredPrimary));
				
				// sparkles, static multiplied by a simple screenpos version
				float sparklesStatic = tex2D(_SparkleNoise, IN.uv_MainTex * _SparkleScale * 5) ;
				float sparklesResult = tex2D(_SparkleNoise, (IN.uv_MainTex * IN.screenPos) * _SparkleScale) * sparklesStatic;
				o.Albedo += step(_SparkCutoff, sparklesResult) *vertexColoredPrimary;
	
				// add a glow on the snow part
				o.Emission = vertexColoredPrimary * _RimColor * pow(rim, _RimPower) ;


			}
			ENDCG

		}

			Fallback "Diffuse"
}