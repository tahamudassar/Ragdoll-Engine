Shader "Ragdoll Engine/Glass"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MatCap ("MatCap", 2D) = "white" {}
        _FresnelPower("Fresnel Power", Float) = 5
        _FresnelStrength("Fresnel Strength", Float) = 1
        _Smoothness("Smoothness", Range(0,1)) = 0.9
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _MatCap;
            float4 _MainTex_ST;
            float4 _Color;
            float _FresnelPower;
            float _FresnelStrength;
            float _Smoothness;
            float _Metallic;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample base color
                half4 albedo = tex2D(_MainTex, IN.uv) * _Color;

                // MatCap
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float3 worldNormal = normalize(IN.worldNormal);
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, worldNormal);
                float2 matcapUV = viewNormal.xy * 0.5 + 0.5;
                albedo.rgb *= tex2D(_MatCap, matcapUV).rgb;

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(worldNormal, viewDir)), _FresnelPower) * _FresnelStrength;
                albedo.rgb += fresnel;

                // Lighting (URP Lit)
                SurfaceData surfaceData;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = 0.0h;
                surfaceData.specular = 0.0h;
                surfaceData.smoothness = 1.0;
                surfaceData.normalTS = 0.0h;
                surfaceData.emission = 0.0h;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = albedo.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = worldNormal;
                inputData.viewDirectionWS = viewDir;
                inputData.shadowCoord = 0;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = 0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                // Alpha
                color.a = clamp(albedo.a, 0, 1);

                // Premultiplied alpha for better glass look
                color.rgb *= color.a;

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
