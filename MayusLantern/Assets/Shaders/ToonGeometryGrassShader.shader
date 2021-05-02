Shader "Toon/Grass Geometry Shader"{

    Properties
    {
        _TopColor("Blade Top Color", Color) = (1,1,0,1)
        _BottomColor("Blade Bottom Color", Color) = (0,1,0,1)
        _GrassHeight("Grass Blade Height", Float) = 1
        _GrassWidth("Grass Blade Width", Float) = 0.06
        _RandomHeight("Grass Height Randomness", Float) = 0.25
        _WindSpeed("Wind Speed", Float) = 100
        _WindStrength("Wind Strength", Float) = 0.05
        _InteractorRadius("Interaction Radius", Float) = 0.3
        _InteractorStrength("Interaction Strength", Float) = 5
        _BladeRadius("Blade Radius", Range(0,1)) = 0.6
        _BladeForward("Blade Forward Amount", Float) = 0.38
        _BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2
        _AmbientStrength("Ambient Strength", Range(0,1)) = 0.5
        _MinCamDistance("Min Camera Distance", Float) = 40
        _MaxCamDistance("Max Camera Distance", Float) = 60
    }

    HLSLINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma require geometry
        #pragma geometry geo
            
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile_fog   
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ LIGHTMAP_ON
        
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        #define GrassSegments 5 //segments per blade
        #define GrassBlades 4 //blades per object vertex

        struct Attributes
        {
            float positionOS : POSITION;
            float3 norm : NORMAL;
            float2 uv : TEXCOORD0;
            float3 color : COLOR;
            float4 tangent : TANGENT;
        };

        struct VertToGeo
        {
            float pos : SV_POSITION;
            float3 norm : NORMAL;
            float2 uv : TEXCOORD0;
            float3 color : COLOR;
            float4 tangent : TANGENT;
        };
        
        CBUFFER_START(UnityPerMaterial)
            half _GrassHeight, _GrassWidth, _WindSpeed, _InteractorRadius, _InteractorStrength;
            float _WindStrength, _BladeRadius;
            float _RandomHeight, _BladeForward, _BladeCurve;
            float _MinCamDistance, _MaxCamDistance;

            uniform float _PositionMoving;
        CBUFFER_END

        VertToGeo vert(Attributes v)
        {
            float3 v0 = v.vertex.xyz;

            VertToGeo OUT;
            OUT.pos = v.vertex;
            OUT.norm = v.normal;
            OUT.uv = v.texcoord;
            OUT.color = v.color;

            OUT.norm = TransformObjectToWorldNormal(v.normal);
            OUT.tangent = v.tangent;

            return OUT;
        }

        struct GeoToFrag
        {
            float pos : SV_POSITION;
            float3 norm : NORMAL;
            float2 uv : TEXCOORD0;
            float3 diffuseColor : COLOR;
            float3 worldPos : TEXCOORD3;
            float fogFactor : TEXCOORD5;
        };

        float rand(float3 co){
            return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
        }


        float3x3 AngleAxisMatrix(float angle, float3 axis){
            float c, s;
            sincos(angle, s, c);

            float t = 1 - c;
            float x = axis.x;
            float y = axis.y;
            float z = axis.z;
    
            return float3x3(
                t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                t * x * z - s * y, t * y * z + s * x, t * z * z + c
                );
        }

        float4 GetShadowPositionHClip(float3 input, float3 normal)
        {
            float3 positionWS = TransformObjectToWorld(input.xyz);
            float3 normalWS = TransformObjectToWorldNormal(normal);
            float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS,normalWS,0));

    #if UNITY_REVERSED_Z
            positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
            positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif

            return positionCS;
        }

        GeoToFrag GrassVertex(float3 vertexPos, float width, float height, float offset, float curve, float2 uv, float3x3 rotationMatrix, float3 faceNormal, float3 color)
        {
            GeoToFrag OUT;

            float offsetVerts = vertexPos + mul(rotationMatrix, float3(width, height, curve) + float3(0,0,offset)));
            OUT.pos = GetShadowPositionHClip(offsetVerts, faceNormal);
            OUT.norm = faceNormal;
            OUT.diffuseColor = color;
            OUT.uv = uv;
            
            VertexPositionInputs vertexInput = GetVertexPositionInputs(vertexPos + mul(rotationMatrix, float3(width, height, curve)));

            OUT.worldPos = vertexInput.positionWS;

            float fogFactor = ComputeFogFactor(OUT.pos.z);
            OUT.fogFactor = fogFactor;

            return OUT;
        }
    
    ENDHLSL

    SubShader
    {
        Tags 
        {
            "RenderType" = "Geometry" 
            "LightMode" = "ForwardBase" 
            "RenderPipeline" = "UniversalRenderPipeline" 
        }

        Pass
        {
        HLSLPROGRAM
            
            
        ENDHLSL    
         
        

        }
    }
}
