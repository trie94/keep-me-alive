Shader "Unlit/PlateletBody"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
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

            sampler2D _MainTex;
            sampler2D _Ramp;
            sampler2D _BackgroundTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
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

                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance/15, 0, 1);
                fixed4 background = tex2D(_BackgroundTexture, i.worldPos.xy);

                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = lerp(col.rgb, background.rgb, viewDistance);
                
                return col * lighting;
            }
            ENDCG
        }
    }
}
