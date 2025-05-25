
Shader "Ragdoll Engine/Metallic (URP)"
{
    Properties
    {
        [MainTexture] _MainTex("Color Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _MetallicTexture("Metallic Texture", 2D) = "white" {}
        [KeywordEnum(R, G, B, A)] _MetallicChannel("Metallic Channel", Float) = 3
        [Toggle] _InvertMetallic("Invert Metallic", Float) = 0
        _Metallic("Metallic", Range(0, 1)) = 0
        _SmoothnessTexture("Smoothness Texture", 2D) = "white" {}
        [KeywordEnum(R, G, B, A)] _SmoothnessChannel("Smoothness Channel", Float) = 3
        [Toggle] _InvertSmoothness("Invert Smoothness", Float) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0
        [Normal] _NormalTexture("Normal Texture", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 0
        _EmissionTexture("Emission Texture", 2D) = "black" {}
        _FalloffTexture("Falloff Texture", 2D) = "white" {}
        _FresnelPower("Fresnel Power", Float) = 5
        _FresnelStrength("Fresnel Strength", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Shader Features
            #pragma shader_feature_local _METALLICCHANNEL_R _METALLICCHANNEL_G _METALLICCHANNEL_B _METALLICCHANNEL_A
            #pragma shader_feature_local _SMOOTHNESSCHANNEL_R _SMOOTHNESSCHANNEL_G _SMOOTHNESSCHANNEL_B _SMOOTHNESSCHANNEL_A
            #pragma shader_feature_local _INVERTMETALLIC_ON
            #pragma shader_feature_local _INVERTSMOOTHNESS_ON

            // URP Features
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float4 tangentWS    : TEXCOORD3;
                float3 viewDirWS    : TEXCOORD4;
                float3 vertexSH     : TEXCOORD5;
                float3 fogCoord	    : TEXCOORD6;
                float4 positionCS   : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            TEXTURE2D(_MetallicTexture);
            TEXTURE2D(_SmoothnessTexture);
            TEXTURE2D(_NormalTexture);
            TEXTURE2D(_EmissionTexture);
            TEXTURE2D(_FalloffTexture);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Metallic;
                half _Smoothness;
                half _NormalStrength;
                half _FresnelPower;
                half _FresnelStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Position transforms
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                // Normal and tangent transforms
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w);

                // View direction
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                 output.vertexSH = SampleSHVertex(input.normalOS);
    
    return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Albedo
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                // Metallic
                half4 metallicTex = SAMPLE_TEXTURE2D(_MetallicTexture, sampler_MainTex, input.uv);
                half metallic = _Metallic;
                #if defined(_METALLICCHANNEL_R)
                    metallic *= metallicTex.r;
                #elif defined(_METALLICCHANNEL_G)
                    metallic *= metallicTex.g;
                #elif defined(_METALLICCHANNEL_B)
                    metallic *= metallicTex.b;
                #else
                    metallic *= metallicTex.a;
                #endif
                #ifdef _INVERTMETALLIC_ON
                    metallic = 1.0 - metallic;
                #endif

                // Smoothness
                half4 smoothnessTex = SAMPLE_TEXTURE2D(_SmoothnessTexture, sampler_MainTex, input.uv);
                half smoothness = _Smoothness;
                #if defined(_SMOOTHNESSCHANNEL_R)
                    smoothness *= smoothnessTex.r;
                #elif defined(_SMOOTHNESSCHANNEL_G)
                    smoothness *= smoothnessTex.g;
                #elif defined(_SMOOTHNESSCHANNEL_B)
                    smoothness *= smoothnessTex.b;
                #else
                    smoothness *= smoothnessTex.a;
                #endif
                #ifdef _INVERTSMOOTHNESS_ON
                    smoothness = 1.0 - smoothness;
                #endif

                // Normal Map
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalTexture, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormalScale(normalMap, _NormalStrength);
                half3 bitangent = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent, input.normalWS);
                half3 normalWS = mul(normalTS, tangentToWorld);
                normalWS = NormalizeNormalPerPixel(normalWS);

                // Emission
                half3 emission = SAMPLE_TEXTURE2D(_EmissionTexture, sampler_MainTex, input.uv).rgb;

                // Fresnel Effect
                half3 viewDir = SafeNormalize(input.viewDirWS);
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);
                half4 falloff = SAMPLE_TEXTURE2D(_FalloffTexture, sampler_MainTex, input.uv);
                albedo.rgb += falloff.rgb * fresnel * _FresnelStrength;

                // Surface Data
                SurfaceData surfaceData;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = metallic;
                surfaceData.specular = 0.0h;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = albedo.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                // Input Data
                InputData inputData;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDir;
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = 0.0;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SampleSHPixel(input.vertexSH,inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // Final Color
                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                return color;
            }
            ENDHLSL
        }

        // Shadow Casting Pass
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        
        // Depth Only Pass
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        
        // Meta Pass (for lightmapping)
        UsePass "Universal Render Pipeline/Lit/Meta"
    }

    FallBack "Universal Render Pipeline/Lit"
}