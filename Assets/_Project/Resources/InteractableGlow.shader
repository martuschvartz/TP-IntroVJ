// Brillo de "esto se puede apretar". Lo agrega PlayerInteractor por codigo como un
// material extra sobre lo que estas mirando (no hay que ponerlo a mano en ningun objeto).
// - Pasada 1: brillo en los bordes del objeto (fresnel), aditivo.
// - Pasada 2: contorno por fuera (casco invertido), de ancho constante en pantalla.
// Las dos laten con _PulseSpeed.
Shader "WhatLiesBehind/InteractableGlow"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 0.8, 0.35, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2
        _RimIntensity ("Rim Intensity", Range(0, 4)) = 1.4
        _FillIntensity ("Fill Intensity", Range(0, 1)) = 0.12
        _OutlineWidth ("Outline Width (px)", Range(0, 12)) = 4
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 4
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+10" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _RimPower;
            half _RimIntensity;
            half _FillIntensity;
            half _OutlineWidth;
            half _PulseSpeed;
            half _PulseAmount;
        CBUFFER_END

        half Pulse()
        {
            return 1.0h - _PulseAmount * (0.5h + 0.5h * sin(_Time.y * _PulseSpeed));
        }
        ENDHLSL

        // Brillo en los bordes, encima del objeto.
        Pass
        {
            Name "Rim"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 viewWS : TEXCOORD1; };

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(positionWS);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.viewWS = GetWorldSpaceViewDir(positionWS);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half ndv = saturate(dot(normalize(input.normalWS), normalize(input.viewWS)));
                half rim = pow(1.0h - ndv, _RimPower) * _RimIntensity + _FillIntensity;
                return half4(_Color.rgb * rim * Pulse(), 1);
            }
            ENDHLSL
        }

        // Contorno por fuera.
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings o;
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalCS = mul((float3x3)UNITY_MATRIX_VP, TransformObjectToWorldNormal(input.normalOS));
                float2 offset = normalize(normalCS.xy + 1e-5) / _ScreenParams.xy * 2.0 * _OutlineWidth;
                positionCS.xy += offset * positionCS.w;
                o.positionCS = positionCS;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half p = Pulse();
                return half4(_Color.rgb * (0.6h + 0.6h * p), saturate(0.55h + 0.45h * p));
            }
            ENDHLSL
        }
    }
}
