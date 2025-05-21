Shader "Custom/VertexColorURP"
{
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                half4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half4 color        : COLOR;
                float3 normalWS    : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 vertexSH     : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Position transforms
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                // Normal transforms
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;

                // Pass vertex color
                output.color = input.color;
                output.vertexSH = SampleSHVertex(input.normalOS);
    
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Prepare surface data
                SurfaceData surfaceData;
                surfaceData.albedo = input.color.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.specular = 0.0;
                surfaceData.smoothness = 0.0;
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.emission = 0.0;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = input.color.a;
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;

                // Prepare input data
                InputData inputData;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = 0.0;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SampleSHPixel(input.vertexSH,inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // Combine everything with URP lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }

        // Shadow casting pass
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        
        // Depth only pass
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}