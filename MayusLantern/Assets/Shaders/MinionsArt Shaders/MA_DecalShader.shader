// http://www.popekim.com/2012/10/siggraph-2012-screen-space-decals-in.html

Shader "MinionsArt/Decal/DecalShader"
{
	Properties
    {
        _LightStrength("Point/Spot Light Strength", Range(0,1)) = 0.5
    }
        SubShader
        
    {
        Tags {"Queue"="Transparent"}
        Pass
    {
        Tags{ "LightMode" = "ForwardBase" }
 
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        
// uncomment to have selective decals
// NOTE: Also uncomment on the SECOND PASS down below
        Stencil {
        Ref 5
        Comp Equal
        Fail zero
        }
 
// Then add the following to the shader you want the decals to show up on, not this one!:
     Stencil {
          Ref 5
             Comp always
         Pass Replace
         }
 
    CGPROGRAM
 
    #include "UnityCG.cginc"
    #include "AutoLight.cginc"
    #include "Lighting.cginc"
    #pragma multi_compile_fog 
    #pragma multi_compile_fwdbase_fullshadows
    #pragma vertex vert
    #pragma fragment frag
 
        struct v2f
        {
            float4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
            float4 screenUV : TEXCOORD1;
            float3 ray : TEXCOORD2;
            UNITY_FOG_COORDS(4)
            LIGHTING_COORDS(5, 6)
        };
 
        v2f vert(appdata_full v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            o.screenUV = ComputeScreenPos(o.pos);
            o.ray = UnityObjectToViewPos(float4(v.vertex.xyz,0)).xyz * float3(-1,-1,1);
            UNITY_TRANSFER_FOG(o, o.pos);
            TRANSFER_VERTEX_TO_FRAGMENT(o);
            return o;
        }
 
 
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
        UNITY_INSTANCING_BUFFER_END(Props)
 
        sampler2D _CameraDepthTexture;
        sampler2D_float _DirectionalShadowMap;
        SamplerState sampler_DirectionalShadowMap;
 
        fixed4 frag(v2f i) : SV_Target
        {
        i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
 
        //Screenspace UV
        float2 uv = i.screenUV.xy / i.screenUV.w;
        // read depth
        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
        depth = Linear01Depth(depth);
 
 
        // reconstruct world space
        float4 vpos = float4(i.ray * depth, 1);
        float4 wpos = mul(unity_CameraToWorld, vpos);
        float3 opos = mul(unity_WorldToObject, float4(wpos)).xyz;
        clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));
 
        // offset uvs
        i.uv = opos.xz + 0.5;
 
        // add texture from decal script
        fixed4 col = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex), i.uv);
        // discard outside of texture alpha
        clip(col.a - 0.1);
        col *= col.a;
 
        // directional light shadows from commandbuffer
        float shadow = tex2D(_DirectionalShadowMap, uv).r;
 
        // multiply with light color and shadows, ambient
        col *= (_LightColor0 * shadow) + unity_AmbientSky;
 
        // add tinting and transparency
        col.rgb *= _Tint.rgb;
        col *= _Tint.a;
 
        // add in the fog
        UNITY_APPLY_FOG(i.fogCoord, col.rgb);
 
        return saturate(col);
    }
         ENDCG
    }
 
     Pass{
        // secondpass for the extra lights
        Tags{ "LightMode" = "ForwardAdd" }
        // Soft additive 
        Blend OneMinusDstColor One
 
// uncomment to have selective decals
        Stencil{
         Ref 5
         Comp Equal
         Fail zero
        }
 
    
 
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdadd
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"
 
                    float4 _MainTex_ST;
 
                    struct v2f
                    {
                        float4 pos : SV_POSITION;
                        half2 uv : TEXCOORD0;
                        float4 screenUV : TEXCOORD1;
                        float3 ray : TEXCOORD2;
                        float3 normal : NORMAL;
                        float3 lightDir : TEXCOORD3;
                        LIGHTING_COORDS(5, 6)
                    };
 
                    v2f vert(appdata_full v)
                    {
                        v2f o;
                        o.pos = UnityObjectToClipPos(v.vertex);
                        o.uv = v.vertex.xz + 0.5;
                        o.normal = v.normal;
                        o.screenUV = ComputeScreenPos(o.pos);
                        o.lightDir = ObjSpaceLightDir(v.vertex).xyz;
                        o.ray = UnityObjectToViewPos(float4(v.vertex.xyz, 1)).xyz * float3(-1, -1, 1);
                        TRANSFER_VERTEX_TO_FRAGMENT(o);
                        return o;
                    }
 
                    float _LightStrength;
                    sampler2D_float _CameraDepthTexture;
                    sampler2D_float _DirectionalShadowMap;
                    SamplerState sampler_DirectionalShadowMap;
 
                    UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
 
                    UNITY_INSTANCING_BUFFER_END(Props)
 
                    fixed4 frag(v2f i) : SV_Target
                    {
                        i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                        float2 uv = i.screenUV.xy / i.screenUV.w;
 
                        // read depth and reconstruct world position
                        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                        depth = Linear01Depth(depth);
 
                        float4 vpos = float4(i.ray * depth,1);
                        float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
                        float3 opos = mul(unity_WorldToObject, float4(wpos,1)).xyz;
                        // clip off most of the cube
                        clip(float3(0.5,0.5,0.5) - abs(opos.xyz));
 
                        // offset uvs
                        i.uv = opos.xz + 0.5;
                        // add texture from decal script
                        fixed4 col = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex), i.uv);
                        // discard outside of texture alpha
                        clip(col.a - 0.1);
                        col *= col.a;
                        // directional shadows again
                        float shadow = tex2D(_DirectionalShadowMap, uv).r;
                        col.rgb *= saturate(shadow + 0.1);
                        // additional lights attenuation
                        fixed atten = LIGHT_ATTENUATION(i);
 
                        // add in colors and attenuation, multiply with optional light strength
                        col += (_LightColor0 * atten * col.a) * _LightStrength;
                        col *= atten;
                        return col;
                        }
                        ENDCG
                    }
 
    }

		Fallback "VertexLit"
}
