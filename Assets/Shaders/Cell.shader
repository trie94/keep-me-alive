Shader "Unlit/Cell"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}

        _Position("Position", Vector) = (0, 0, 0, 0)

        _NoiseFreq("Noise Frequency", Range(0, 1)) = 1.0
        _NoiseScale("Noise Scale", Range(0, 3)) = 1.0
        _NoiseHeight("Noise Height", Range(0, 1)) = 0.1
        _Offset("Head Offset", Range(-1, 1)) = 0.0

        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)

        _Alpha("Alpha", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
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
                float3 worldNormal : NORMAL;
                float3 smearVal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Ramp;
            fixed4 _Color;
            fixed4 _FogColor;

            float4 _Position;
            float3 _Velocity;

            fixed _NoiseFreq;
            fixed _NoiseScale;
            fixed _NoiseHeight;
            fixed _Offset;

            fixed _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _Face);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 localOffset = _Position.xyz - worldPos.xyz;
                o.worldPos = worldPos;
                
                float3 normalizedVelocity;
                if (length(_Velocity) == 0) {
                    normalizedVelocity = float3(0, 0, 0);
                    } else {
                    normalizedVelocity = normalize(_Velocity);
                }

                float dirDot = abs(dot(normalizedVelocity, normalize(localOffset)) + _Offset);
                fixed3 smearOffset = _Velocity.xyz * (dirDot + snoise(worldPos.xyz * _NoiseFreq) * _NoiseScale) * _NoiseHeight;
                worldPos.xyz -= smearOffset;

                o.smearVal = smearOffset;
                o.vertex = UnityWorldToClipPos(worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
                fixed4 col = _Color;
                fixed4 face = tex2D(_Face, i.uv);

                col.rgb = face.rgb * face.a + col * (1-face.a);
                col = col * lighting;

                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor*0.6));
                #endif

                col.a = _Alpha;
                return col;
            }
            ENDCG
        }
    }
}
