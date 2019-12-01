﻿Shader "Unlit/PlateletBody"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        
        _NoiseFreq("Noise Frequency", Range(0, 1)) = 1.0
        _NoiseScale("Noise Scale", Range(0, 3)) = 1.0
        _MoveSpeed("Move Speed", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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
                SHADOW_COORDS(3)
            };

            sampler2D _Face;
            sampler2D _Ramp;
            sampler2D _BackgroundTexture;

            fixed4 _Color;

            float _NoiseFreq;
            float _NoiseScale;
            float _MoveSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);

                float moveSpeed = _Time.y * _MoveSpeed;

                float xs = _NoiseScale * snoise(float3(worldNormal.xy * _NoiseFreq, moveSpeed));
                float ys = _NoiseScale * snoise(float3(worldNormal.yz * _NoiseFreq, moveSpeed));
                float zs = _NoiseScale * snoise(float3(worldNormal.xz * _NoiseFreq, moveSpeed));

                v.vertex.x += xs;
                v.vertex.y += ys;
                v.vertex.z += zs;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(lightDir, worldNormal));
                o.ramp = ramp;

                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 lighting = float4(tex2D(_Ramp, float2(i.ramp, 0.5)).rgb, 1.0);
                float attenuation = SHADOW_ATTENUATION(i);
                fixed4 col = _Color;
                fixed4 face = tex2D(_Face, i.uv);

                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance/15, 0, 1);

                col.rgb = face.rgb * face.a + col * (1-face.a);
                col = col * lighting * attenuation;

                fixed4 background = tex2D(_BackgroundTexture, i.worldPos.xy);
                col.rgb = lerp(col.rgb, background.rgb, viewDistance);

                return col;
            }
            ENDCG
        }
    }
}