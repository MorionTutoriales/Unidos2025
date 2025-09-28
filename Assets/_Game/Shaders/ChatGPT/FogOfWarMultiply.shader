Shader "URP/FogOfWarMultiply"
{
    Properties
    {
        _Mask("Mask (R=visibility 0..1)", 2D) = "black" {}
        _MinFactor("Dark Min (0=negro,1=sin oscurecer)", Range(0,1)) = 0.0
        _NoiseTex("Noise (optional)", 2D) = "gray" {}
        _NoiseScale("Noise Scale", Float) = 1.0
        _NoiseInfluence("Noise Influence", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // multiplicar con lo ya renderizado
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

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                half  _MinFactor;
                float _NoiseScale;
                half  _NoiseInfluence;
            CBUFFER_END

            TEXTURE2D(_Mask);     SAMPLER(sampler_Mask);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            v2f vert(appdata v){
                v2f o;
                float3 w = TransformObjectToWorld(v.vertex.xyz);
                o.pos = TransformWorldToHClip(w);
                o.uv  = v.uv;
                return o;
            }

            half4 frag(v2f i):SV_Target
            {
                half vis = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv).r; // 0..1
                half factor = lerp(_MinFactor, 1.0h, vis);

                half n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv * _NoiseScale).r;
                factor *= lerp(1.0h, n, _NoiseInfluence);

                return half4(factor, factor, factor, 1);
            }
            ENDHLSL
        }
    }
}
