// Skybox panorámico (equirectangular) con zoom vertical alrededor del horizonte.
// _Zoom > 1 agranda lo que está cerca del horizonte (árboles, etc.) para que el fondo se sienta más cerca.
Shader "Skybox/Panoramic Zoom"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _MainTex ("Spherical (HDR)", 2D) = "grey" {}
        _Zoom ("Zoom (agranda el fondo)", Range(1, 4)) = 2
        _HorizonV ("Horizonte en la textura (V)", Range(0, 1)) = 0.494
        _HorizonOffset ("Subir/bajar horizonte (grados)", Range(-30, 30)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Zoom;
            float _HorizonV;
            float _HorizonOffset;

            struct appdata { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            float3 RotateAroundY(float3 v, float degrees)
            {
                float a = degrees * UNITY_PI / 180.0;
                float s, c; sincos(a, s, c);
                return float3(c * v.x - s * v.z, v.y, s * v.x + c * v.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(RotateAroundY(v.vertex.xyz, _Rotation));
                o.dir = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);

                // Igual que el Skybox/Panoramic de Unity (latitude-longitude).
                float lon = atan2(d.x, d.z);
                float elev = asin(clamp(d.y, -1, 1)) - radians(_HorizonOffset); // -pi/2..pi/2

                float u = 0.5 - lon / (2.0 * UNITY_PI);
                float vRaw = 0.5 + elev / UNITY_PI;
                // Zoom vertical centrado en el horizonte de la textura.
                float v = saturate(_HorizonV + (vRaw - 0.5) / _Zoom);

                // Evita la costura del mipmap donde u salta de 1 a 0.
                float2 uvA = float2(u, v);
                float2 uvB = float2(frac(u + 0.5) - 0.5, v);
                float2 dxA = ddx(uvA), dyA = ddy(uvA);
                float2 dxB = ddx(uvB), dyB = ddy(uvB);
                bool useB = dot(dxB, dxB) + dot(dyB, dyB) < dot(dxA, dxA) + dot(dyA, dyA);
                half4 tex = tex2Dgrad(_MainTex, uvA, useB ? dxB : dxA, useB ? dyB : dyA);

                half3 c = tex.rgb * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;
                return half4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
