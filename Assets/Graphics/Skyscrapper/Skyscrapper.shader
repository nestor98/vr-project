Shader "Custom/Skyscrapper"
{
    Properties
    {
        _MainTex ("Map", 2D) = "white" {}
        _NumHorizontalWindows ("Number of horizontal windows", Int) = 8
        _NumVerticalWindows ("Number of vertical windows", Int) = 8
        _BorderSize ("Window Border Size", Float) = 0.1
        _IlluminatedRatio ("Ratio of illuminated windows", Float) = 0.1
        _IlluminatedWindowColor ("Illuminated Window Color", Color) = (1.0, 1.0, 0.7, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        int _NumHorizontalWindows;
        int _NumVerticalWindows;
        float _BorderSize;
        float _IlluminatedRatio;
        fixed4 _IlluminatedWindowColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        float rand(float2 co){
            return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 a = float2(IN.uv_MainTex.x * _NumHorizontalWindows, IN.uv_MainTex.y * _NumVerticalWindows);
            float2 b = floor(a);
            float2 d = frac(a);
            float e = rand(b);
            if (any(d < _BorderSize) || any(d > (1.0f - _BorderSize))) {
                // Border
                o.Albedo = float3(0.0f, 0.0f, 0.0f);
                o.Metallic = 1.0f;
                o.Smoothness = 0.1f;
                o.Alpha = 1.0f;
            } else {
                // Window
                if (e < _IlluminatedRatio) {
                    // Illuminated
                    o.Albedo = float3(1.0f, 1.0f, 0.0f);
                    o.Emission = _IlluminatedWindowColor.rgb * tex2D(_MainTex, d).rgb;
                    o.Metallic = 0.0f;
                    o.Smoothness = 1.0f;
                    o.Alpha = 1.0f;
                } else {
                    // Not illuminated
                    o.Albedo = float3(1.0f, 1.0f, 1.0f);
                    o.Metallic = 1.0f;
                    o.Smoothness = 1.0f;
                    o.Alpha = 1.0f;
                }
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
