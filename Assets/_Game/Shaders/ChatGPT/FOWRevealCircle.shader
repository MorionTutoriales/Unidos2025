Shader "Hidden/FOW_PaintMaxCircle"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            sampler2D _MainTex; // máscara actual
            float2 _CenterUV;
            float  _Radius;
            float  _Feather;

            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i):SV_Target
            {
                float cur = tex2D(_MainTex, i.uv).r;
                float d   = distance(i.uv, _CenterUV);
                float t   = saturate(1.0 - smoothstep(_Radius - _Feather, _Radius, d)); // 1 centro → 0 fuera
                float outR = max(cur, t); // acumula lo revelado
                return float4(outR, outR, outR, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
