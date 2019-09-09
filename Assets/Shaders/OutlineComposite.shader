Shader "Unlit/OutlineComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Noise Scale", Range(0.01, 1.0)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Noise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _PrePassTex;
            sampler2D _BlurredTex;
            float _Intensity;
            fixed4 _OutlineColor;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = v.uv;
                o.uv1 = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = _Scale * snoise(float3(i.uv0.xy, sin(_Time.y * 0.01) * _Scale));
                i.uv1 = i.uv0 + float2(sin(i.uv0.x * 30) * 0.003 + noise, sin(i.uv0.x * 30) * 0.003 + noise);

                fixed4 col = tex2D(_MainTex, i.uv1);
                fixed4 edge = max(0, tex2D(_BlurredTex, i.uv1) - tex2D(_PrePassTex, i.uv1));

                if (edge.a > 0.1) {
                    return _OutlineColor;
                }
                return col;
            }
            ENDCG
        }
    }
}
