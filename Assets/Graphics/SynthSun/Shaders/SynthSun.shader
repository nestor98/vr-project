Shader "Unlit/SynthSun"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1, 1, 0)
        _BottomColor ("Bottom Color", Color) = (1, 0, 1)
        _Frequency ("Frequency", Float) = 10.0
        _Threshold ("Threshold", Float) = 0.2
        _Exponential ("Exponential", Float) = 1.2
        _Limit ("Limit", Float) = 0.6
        _TimeMultiplier ("Time Multiplier", Float) = 10.0
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Frequency;
            float _Threshold;
            float _Exponential;
            float _Limit;
            float _TimeMultiplier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float circle = step(distance(i.uv, float2(0.5f, 0.5f)), 0.5f);
                float wave = sin(i.uv.y * _Frequency * exp2(i.uv.y * _Exponential) + _Time.x * _TimeMultiplier) * step(i.uv.y, _Limit);
                float wave_factor = step(wave, _Threshold);
                float3 color = lerp(_TopColor, _BottomColor, 1.0f - i.uv.y) * 2.0f;
                if (circle * wave_factor < 0.1f)
                    discard;
                return float4(color, 1.0f);
            }
            ENDCG
        }
        
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Frequency;
            float _Threshold;
            float _Exponential;
            float _Limit;
            float _TimeMultiplier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float circle = step(distance(i.uv, float2(0.5f, 0.5f)), 0.5f);
                float wave = sin(i.uv.y * _Frequency * exp2(i.uv.y * _Exponential) + _Time.x * _TimeMultiplier) * step(i.uv.y, _Limit);
                float wave_factor = step(wave, _Threshold);
                float3 color = lerp(_TopColor, _BottomColor, 1.0f - i.uv.y) * 2.0f;
                if (circle * wave_factor < 0.1f)
                    discard;
                return float4(color, 1.0f);
            }
            ENDCG
        }
    }
}
