Shader "Unlit/Heart"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        _Pulse ("Pulse", 2D) = "white" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        _Face ("Face", 2D) = "black" {}
        _Color ("Color", color) = (1,0,0,1)

        _PulseSpeed("Pluse Speed", Range(10, 100)) = 30.0
        _MinSize("Min Size", Range(-0.5, 1)) = 1.0
        _MaxSize("Max Size", Range(0, 2)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        
        GrabPass
        {
            "_BackgroundTexture"
        }

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float pulse: TEXCOORD1;
                float3 worldNormal : NORMAL;
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Pulse;
            fixed4 _Color;
            float _PulseSpeed;
            float _MinSize;
            float _MaxSize;

            sampler2D _BackgroundTexture;
            sampler2D _Ramp;
            
            v2f vert (appdata v)
            {
                v2f o;
                float pulseInterval = 1+sin(_Time.x * _PulseSpeed);
                float pulseAmount = tex2Dlod(_Pulse, float4(pulseInterval, 0, 0, 0)).x;
                pulseAmount = clamp(pulseAmount, 0, _MaxSize);
                o.pulse = pulseAmount;
                v.vertex *= (_MinSize+pulseAmount);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _Face);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
                fixed4 face = tex2D(_Face, i.uv);
                
                fixed4 col = _Color;
                col.rgb = face.rgb * face.a + col * (1-face.a) * lighting;
                col += i.pulse * 0.4;
                return col;
            }
            ENDCG
        }
    }
}
