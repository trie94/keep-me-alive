Shader "Unlit/PlateletLimb"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}

        _Length ("Limb Length", Range(2, 5)) = 3
        _Thickness ("Limb Thickness", Range(0.1, 2)) = 0.5

        _YOffset ("Height Offset", Range(-1, 2)) = 2
        _Speed ("Limb Speed", Range(0.1, 2)) = 0.5
        _NoiseFreq ("Noise Frequency", Range(0.1, 2)) = 1
        _NoiseIntensity ("Noise Intensity", Range(0.001, 2)) = 1
        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
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
                float ramp : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
                SHADOW_COORDS(4)
            };

            sampler2D _Ramp;

            uniform fixed _Length;
            uniform fixed _Thickness;

            fixed4 _Color;
            fixed4 _FogColor;

            float _YOffset;
            uniform float _Speed;
            float _NoiseFreq;
            float _NoiseIntensity;

            uniform float3 _HandPosition;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.y *= _Length;
                v.vertex.xz *= pow(1-v.vertex.y + 0.25, 2) * _Thickness;

                float height = v.vertex.y - _YOffset;
                float sway = sin(v.vertex.y * _NoiseFreq + _Time.y * _Speed) * _NoiseIntensity;
                v.vertex.xz += sway * saturate(height);

                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos;

                o.uv = v.uv;

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                float ramp = saturate(dot(lightDir, worldNormal));
                o.ramp = ramp;
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 lighting = float4(tex2D(_Ramp, float2(i.ramp, 0.5)).rgb, 1.0);
                fixed4 col = _Color * lighting;
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor*0.6));
                #endif
                return col;
            }
            ENDCG
        }
    }
}
