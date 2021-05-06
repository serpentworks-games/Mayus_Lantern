// uses explanation of "melting shader" vertex displacement and normal recalculation from http://diary.conewars.com/vertex-displacement-shader/
Shader "Toon/Lit Merge" {
	Properties {
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Top Texture", 2D) = "white" {}
		_MainTexSide("Side/Bottom Texture", 2D) = "white" {}
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}
		_Normal("Normal/Noise", 2D) = "bump" {}
		_Scale("Top Scale", Range(-2,2)) = 1
		_SideScale("Side Scale", Range(-2,2)) = 1
		_NoiseScale("Noise Scale", Range(-2,2)) = 1
		_TopSpread("TopSpread", Range(-2,2)) = 1
		_EdgeWidth("EdgeWidth", Range(0,0.5)) = 1
		_RimPower("Rim Power", Range(-2,20)) = 1
		_RimColor("Rim Color Top", Color) = (0.5,0.5,0.5,1)
		_RimColor2("Rim Color Side/Bottom", Color) = (0.5,0.5,0.5,1)
		[PerRendererData]_MergeY("Merge Y", Float) = 0.0 // propertyblock
		_MergeW("Merge W", Float) = 0.0
		_MergeWidth("Merge Width", Float) = 0.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
CGPROGRAM
#pragma surface surf ToonRamp vertex:vert addshadow

sampler2D _Ramp;

// custom lighting function that uses a texture ramp based
// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass
inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
{
	#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
	#endif
	
	half d = dot (s.Normal, lightDir)*0.5 + 0.5;
	half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
	
	half4 c;
	c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
	c.a = 0;
	return c;
}



sampler2D _MainTex, _MainTexSide, _Normal;
float4 _Color, _RimColor, _RimColor2;
float _RimPower;
float  _TopSpread, _EdgeWidth;
float _Scale, _SideScale, _NoiseScale;

float _MergeY;
float _MergeW;
float _MergeWidth;


struct Input {
	float2 uv_MainTex : TEXCOORD0;
	float3 worldPos;
	float3 worldNormal;
	float3 viewDir;
};

float4 getNewVertPosition(float4 objectSpacePosition, float3 objectSpaceNormal)
{
	float4 worldSpacePosition = mul(unity_ObjectToWorld, objectSpacePosition);

	// we need to find the normal of the vertex in world space to know which way to push the verts
	// need to make normal a float4. opengl/metal wont build with mul(float4x4, float3)
	float4 worldSpaceNormal = mul(unity_ObjectToWorld, float4(objectSpaceNormal, 0));

	// we have a 'melt' value, but it's actually the reverse of what we want
	// also it will affect verts above the distance we melt by sucking them in (because it's negative)
	float mergePoint = (worldSpacePosition.y - _MergeY) / _MergeWidth;

	// saturate is like Clamp01, giving us a nice value between 0 and 1,
	// then we flip it so it's greater the closer to the floor
	mergePoint = 1 - saturate(mergePoint);

	// push the vert out forwards and sideways by how much 'melt' is
	worldSpacePosition.xz += worldSpaceNormal.xz * mergePoint * _MergeW;

	// all that's left now is to update the actual vertex data that's sent to the surf function
	// we take our modified world space vert and put it back into object space
	return mul(unity_WorldToObject, worldSpacePosition);
}

void vert(inout appdata_full v)
{
	float4 vertPosition = getNewVertPosition(v.vertex, v.normal);

	// calculate the bitangent (sometimes called binormal) from the cross product of the normal and the tangent
	float4 bitangent = float4(cross(v.normal, v.tangent), 0);

	// how far we want to offset our vert position to calculate the new normal
	float vertOffset = 0.01;

	float4 v1 = getNewVertPosition(v.vertex + v.tangent * vertOffset, v.normal);
	float4 v2 = getNewVertPosition(v.vertex + bitangent * vertOffset, v.normal);

	// now we can create new tangents and bitangents based on the deformed positions
	float4 newTangent = v1 - vertPosition;
	float4 newBitangent = v2 - vertPosition;

	// recalculate the normal based on the new tangent & bitangent
	v.normal = cross(newTangent, newBitangent);

	v.vertex = vertPosition;
}


void surf (Input IN, inout SurfaceOutput o) {
	
	// clamp (saturate) and increase(pow) the worldnormal value to use as a blend between the projected textures
	float3 blendNormal = saturate(pow(IN.worldNormal * 1.4, 4));

	// normal noise triplanar for x, y, z sides
	float3 xn = tex2D(_Normal, IN.worldPos.zy * _NoiseScale);
	float3 yn = tex2D(_Normal, IN.worldPos.zx * _NoiseScale);
	float3 zn = tex2D(_Normal, IN.worldPos.xy * _NoiseScale);

	// lerped together all sides for noise texture
	float3 noisetexture = zn;
	noisetexture = lerp(noisetexture, xn, blendNormal.x);
	noisetexture = lerp(noisetexture, yn, blendNormal.y);

	// triplanar for top texture for x, y, z sides
	float3 xm = tex2D(_MainTex, IN.worldPos.zy * _Scale);
	float3 zm = tex2D(_MainTex, IN.worldPos.xy * _Scale);
	float3 ym = tex2D(_MainTex, IN.worldPos.zx * _Scale);

	// lerped together all sides for top texture
	float3 toptexture = zm;
	toptexture = lerp(toptexture, xm, blendNormal.x);
	toptexture = lerp(toptexture, ym, blendNormal.y);

	// triplanar for side and bottom texture, x,y,z sides
	float3 x = tex2D(_MainTexSide, IN.worldPos.zy * _SideScale);
	float3 y = tex2D(_MainTexSide, IN.worldPos.zx * _SideScale);
	float3 z = tex2D(_MainTexSide, IN.worldPos.xy * _SideScale);

	// lerped together all sides for side bottom texture
	float3 sidetexture = z;
	sidetexture = lerp(sidetexture, x, blendNormal.x);
	sidetexture = lerp(sidetexture, y, blendNormal.y);

	// rim light for fuzzy top texture
	half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal * noisetexture));

	// rim light for side/bottom texture
	half rim2 = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));

	// dot product of world normal and surface normal + noise
	float worldNormalDotNoise = dot(o.Normal + (noisetexture.y + (noisetexture * 0.5)), IN.worldNormal.y);

	// if dot product is higher than the top spread slider, multiplied by triplanar mapped top texture
	// step is replacing an if statement to avoid branching :
	// if (worldNormalDotNoise > _TopSpread{ o.Albedo = toptexture}
	float3 topTextureResult = step(_TopSpread, worldNormalDotNoise) * toptexture;

	// if dot product is lower than the top spread slider, multiplied by triplanar mapped side/bottom texture
	float3 sideTextureResult = step(worldNormalDotNoise, _TopSpread) * sidetexture;

	// if dot product is in between the two, make the texture darker
	float3 topTextureEdgeResult = step(_TopSpread, worldNormalDotNoise) * step(worldNormalDotNoise, _TopSpread + _EdgeWidth) *  -0.15;

	// final albedo color
	o.Albedo = topTextureResult + sideTextureResult + topTextureEdgeResult;
	o.Albedo *= _Color;
	// adding the fuzzy rimlight(rim) on the top texture, and the harder rimlight (rim2) on the side/bottom texture
	o.Emission = step(_TopSpread, worldNormalDotNoise) * _RimColor.rgb * pow(rim, _RimPower) + step(worldNormalDotNoise, _TopSpread) * _RimColor2.rgb * pow(rim2, _RimPower);

	
}
ENDCG

	} 

	Fallback "Diffuse"
}
