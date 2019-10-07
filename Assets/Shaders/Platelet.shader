Shader "Unlit/Platelet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

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
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                float rim : TEXCOORD1;
                float4 noise : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            sampler2D _MainTex;
            // float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                // move vertex using the texture
                float4 noise = snoise_grad(v.vertex.xyz * 2.0);
                o.noise = noise;
                float3 direction = noise.xyz - dot(noise.xyz, v.normal.xyz) * v.normal.xyz;

                float spike = pow(saturate(lerp(-1, 1, (noise.w * 0.5 + 0.5))), 3.0);

                v.vertex += v.normal * spike;
                v.vertex.xyz += direction * spike * 0.01;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);

                // half rim = saturate(1.0 - dot(normalize(v.normal), _WorldSpaceCameraPos));
                half rim = saturate(1.0 - dot(normalize(v.normal), _WorldSpaceLightPos0));
                o.rim = rim;
                
                TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float4 lighting = saturate(dot(lightDir, i.worldNormal)) + 0.5;
                fixed4 col = i.noise.wwww * 0.5 + 0.5;
                float attenuation = SHADOW_ATTENUATION(i);
                return col * attenuation * lighting;
                // return col;
            }
            ENDCG
        }
    }
}
