Shader "Custom/PlaneOfDeath"
{
    Properties
    {
        _GridTex ("Grid Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Scale ("Scale", Float) = 16.0
        _Color1 ("Color 1", Color) = (1, 0, 1, 0)
        _Color2 ("Color 2", Color) = (0, 1, 1, 0)
        _ColorMultiplier ("Color Multiplier", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float2 uv_GridTex;
        };

        sampler2D _GridTex;
        sampler2D _NoiseTex;
        float _Scale;
        fixed4 _Color1;
        fixed4 _Color2;
        float _ColorMultiplier;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {   
            float noise = tex2D(_NoiseTex, IN.uv_GridTex).x;
            fixed3 col = lerp(_Color1, _Color2, noise) * _ColorMultiplier;

            float2 grid_uv = frac(IN.uv_GridTex * _Scale);
            float grid = tex2D(_GridTex, grid_uv).x;

            o.Emission = grid * col;
            o.Albedo = _Color2 * 0.02f;
            o.Metallic = 1.0f;
            o.Smoothness = 0.5f;
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
