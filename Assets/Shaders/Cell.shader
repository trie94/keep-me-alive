Shader "Unlit/Cell"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}

        _PrevPosition("Prev Position", Vector) = (0, 0, 0, 0)
        _Position("Position", Vector) = (0, 0, 0, 0)

        _NoiseFreq("Noise Frequency", Range(0, 1)) = 1.0
        _NoiseScale("Noise Scale", Range(0, 3)) = 1.0
        _NoiseHeight("Noise Height", Range(0, 1)) = 0.1
        _Offset("Head Offset", Range(-1, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" /*"OutlineTarget"="True"*/}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
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
                float3 worldNormal : NORMAL;
                float squish : TEXCOORD1;
                SHADOW_COORDS(2)
            };

            sampler2D _Face;
            sampler2D _Ramp;
            fixed4 _Color;

            float4 _PrevPosition;
            float4 _Position;
            float3 _Velocity;

            fixed _NoiseFreq;
            fixed _NoiseScale;
            fixed _NoiseHeight;
            fixed _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                TRANSFER_SHADOW(o);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                // float3 worldOffset = _Position.xyz - _PrevPosition.xyz;
                float3 localOffset = worldPos.xyz - _Position.xyz;

                float dirDot = abs(dot(normalize(_Velocity), normalize(localOffset)) + _Offset);
                // fixed3 unitVec = fixed3(1, 1, 1) * _NoiseHeight;
                o.squish = dirDot;

                fixed3 smearOffset = _Velocity.xyz * (dirDot + snoise(worldPos.xyz * _NoiseFreq) * _NoiseScale) * _NoiseHeight;
                worldPos.xyz -= smearOffset;
                
                o.vertex = UnityWorldToClipPos(worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(i.worldNormal, lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
                float attenuation = SHADOW_ATTENUATION(i);
                fixed4 col = _Color;
                fixed4 face = tex2D(_Face, i.uv);

                col.rgb = face.rgb * face.a + col * (1-face.a);
                col = col * lighting * attenuation;
                // col.a = 0.8;
                return col;
                // return fixed4(i.squish * 0.5 + 0.5, 0,0,1);
            }
            ENDCG
        }
    }
}
