Shader "Ragdoll Engine/Specular (URP)"
{
    Properties
    {
        [MainTexture] _MainTex ("Color Texture", 2D) = "white" {}
        _SpecularTexture ("Specular Texture", 2D) = "white" {}
        [Normal] _NormalTexture ("Normal Texture", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0
        _FalloffTexture ("Falloff Texture", 2D) = "white" {}
        _FresnelPower ("Fresnel Power", Float) = 5
        _FresnelStrength ("Fresnel Strength", Float) = 0
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
                float4 positionCS   : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            TEXTURE2D(_SpecularTexture);
            TEXTURE2D(_NormalTexture);
            TEXTURE2D(_FalloffTexture);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half _NormalStrength;
                half _FresnelPower;
                half _FresnelStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w);

                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertexSH = SampleSHVertex(input.normalOS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Albedo
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Specular
                half4 specularTex = SAMPLE_TEXTURE2D(_SpecularTexture, sampler_MainTex, input.uv);
                half3 specular = specularTex.rgb;
                half smoothness = specularTex.a;

                // Normal Map
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalTexture, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormalScale(normalMap, _NormalStrength);
                half3 bitangent = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent, input.normalWS);
                half3 normalWS = mul(normalTS, tangentToWorld);
                normalWS = NormalizeNormalPerPixel(normalWS);

                // Fresnel
                half3 viewDir = SafeNormalize(input.viewDirWS);
                half fresnel = pow(1.0 - abs(dot(normalWS, viewDir)), _FresnelPower);
                half4 falloff = SAMPLE_TEXTURE2D(_FalloffTexture, sampler_MainTex, input.uv);
                half fresnelEffect = fresnel * _FresnelStrength * falloff.r;
                albedo.rgb += fresnelEffect;

                // Surface Data
                SurfaceData surfaceData;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = 0.0h;
                surfaceData.specular = specular;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = 0.0h;
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
                inputData.bakedGI = SampleSHPixel(input.vertexSH, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // Final Color
                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                return color;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }

    FallBack "Universal Render Pipeline/Lit"
}
