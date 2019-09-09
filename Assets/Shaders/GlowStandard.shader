Shader "Custom/GlowStandard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "OutlineTarget"="True" }
        LOD 200

        CGPROGRAM
        #pragma surface surf ToonRamp
        
        sampler2D _Ramp;
        sampler2D _MainTex;
        fixed4 _Color;

        half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            lightDir = normalize(lightDir);
            float ramp = saturate(dot(s.Normal, lightDir)) * (atten);
            float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
            
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * lighting;
            c.a = 1;
            return c;
        }
        
        struct Input
        {
            float2 uv_MainTex : TEXCOORD0;
        };
        
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = _Color * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
