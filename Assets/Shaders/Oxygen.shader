Shader "Unlit/Oxygen"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

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
                float3 worldNormal: TEXCOORD1;
                float3 normal: TEXCOORD2;
                float4 localPos: TEXCOORD3;
                float4 worldPos: TEXCOORD4;
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Ramp;
            fixed4 _Color;
            fixed4 _FogColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Face);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.normal = v.normal;
                o.localPos = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);

                float3 viewDir = normalize(ObjSpaceViewDir(i.localPos));
                float rim = saturate(1-dot(viewDir, normalize(i.normal)));
                rim = step(0.6, rim);
                fixed4 rimColor = fixed4(1, 1, 1, 1) * rim;
                float phong = step(0.95, ramp);

                fixed4 col = _Color + phong * 0.5;
                fixed4 face = tex2D(_Face, i.uv);
                col.rgb = face.rgb * face.a + col * (1-face.a);
                col += rimColor * 0.5;
                col = col * lighting;

                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor*0.6));
                #endif
                col.a = 0.75;
                return col;
            }
            ENDCG
        }
    }
}
