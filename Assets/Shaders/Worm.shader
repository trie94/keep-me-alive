Shader "Unlit/Worm"
{
    Properties
    {
        _HeadColor ("Head Color", color) = (1,0,0,1)
        _TailColor ("Tail Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        
        _GradientPower ("Gradient Power", Range(0.1, 5.0)) = 1.0
        _BodyLength ("Length", Range(1, 6)) = 1.0
        _Cap ("Cap", Range(-0.5, 1.0)) = 0.0

        _Speed ("Speed", Range(1.0, 70.0)) = 0.1
        _Wobble ("Wobble", Range(0.0, 1.0)) = 0.1

        _Deform ("Deform", Range(-1.5, 3.0)) = 0.1
        _DeformPower ("Deform Power", Range(0.1, 1.0)) = 0.35
        _Flip("Flip", Int) = 1

        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                float4 worldPos : TEXCOORD1;
                float4 localPos : TEXCOORD2;
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Ramp;
            
            float _BodyLength;
            float _Cap;

            fixed4 _HeadColor;
            fixed4 _TailColor;
            fixed _GradientPower;
            fixed4 _FogColor;

            float _Wobble;
            float _Speed;

            float _Deform;
            float _DeformPower;
            fixed _Flip;

            v2f vert (appdata v)
            {
                v2f o;
                o.localPos = v.vertex;
                v.vertex.xy = v.vertex.xy * pow(saturate(v.vertex.z + _Deform), _DeformPower);
                if (abs(v.vertex.z) <= _Cap) {
                    v.vertex.z *= _BodyLength;
                } else {
                    v.vertex.z += (_BodyLength-1) * sign(v.vertex.z);
                }
                
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos;

                float y = sin(v.vertex.z + (_Time.x * _Speed)) * _Wobble;
                v.vertex.y += y * _Flip;

                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Face);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float lerpFactor = pow(saturate(i.localPos.z/3.0+0.5), _GradientPower);
                fixed4 col = lerp( _TailColor, _HeadColor, lerpFactor);
                fixed4 face = tex2D(_Face, i.uv);
                col.rgb = face.rgb * face.a + col * (1-face.a);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
                
                col = col * lighting;
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);

                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor));
                #endif
                return col;
            }
            ENDCG
        }
    }
}
