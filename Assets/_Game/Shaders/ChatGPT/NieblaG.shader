Shader "URP/NieblaG"
{
    Properties
    {
        _Mask("Mascara (R=visibilidad 0..1)", 2D) = "black" {}
        _MinFactor("Oscuridad minima (0=negro,1=sin oscurecer)", Range(0,1)) = 0.0

        _NoiseTex("Ruido (opcional)", 2D) = "gray" {}
        _NoiseScale("Escala Ruido", Float) = 1.0
        _NoiseInfluence("Influencia Ruido (legacy)", Range(0,1)) = 0.0

        _NoiseAmount("Fuerza Interferencia", Range(0,1)) = 0.5
        _NoiseSpeedX("Velocidad Ruido X (UV/s)", Float) = 0.2
        _NoiseSpeedY("Velocidad Ruido Y (UV/s)", Float) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend DstColor Zero
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            TEXTURE2D(_Mask);     SAMPLER(sampler_Mask);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                half  _MinFactor;
                float _NoiseScale;
                half  _NoiseInfluence;
                half  _NoiseAmount;
                float _NoiseSpeedX;
                float _NoiseSpeedY;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                float3 w = TransformObjectToWorld(v.vertex.xyz);
                o.pos = TransformWorldToHClip(w);
                o.uv  = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Máscara base 0..1
                half vis = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv).r;

                // Tiempo URP: _TimeParameters.x = time (seconds)
                float t = _TimeParameters.x;

                // UV animado para el ruido
                float2 nUV = i.uv * _NoiseScale + float2(_NoiseSpeedX, _NoiseSpeedY) * t;

                // Ruido 0..1 -> [-1,1]
                half n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, nUV).r;
                half n01 = n * 2.0h - 1.0h;

                // Efecto solo en intermedios: campana con pico en 0.5
                half midAtten = vis * (1.0h - vis);   // [0..0.25]
                half delta = n01 * _NoiseAmount * midAtten * 2.0h; // compensa el 0.25 máximo

                // Distorsión simétrica alrededor de vis, clamp 0..1
                half visWarp = saturate(vis + delta);

                // (Opcional) textura estática extra en sombras, amortiguada por midAtten
                if (_NoiseInfluence > 0.0h)
                {
                    half nStatic = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv * _NoiseScale).r;
                    visWarp = lerp(visWarp, nStatic * visWarp + (1.0h - nStatic) * visWarp, _NoiseInfluence * midAtten);
                }

                // Multiplicador final
                half factor = lerp(_MinFactor, 1.0h, visWarp);
                return half4(factor, factor, factor, 1);
            }
            ENDHLSL
        }
    }
}
