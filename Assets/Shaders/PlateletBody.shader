Shader "Unlit/PlateletBody"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        
        _NoiseFreq("Noise Frequency", Range(0, 1)) = 1.0
        _NoiseScale("Noise Scale", Range(0, 3)) = 1.0
        _MoveSpeed("Move Speed", Range(0, 1)) = 1.0
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
                SHADOW_COORDS(3)
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Ramp;

            fixed4 _Color;
            fixed4 _FogColor;

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
                o.uv = TRANSFORM_TEX(v.uv, _Face);

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

                col.rgb = face.rgb * face.a + col * (1-face.a);
                col = col * lighting * attenuation;

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
