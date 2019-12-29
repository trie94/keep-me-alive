Shader "Unlit/PathTube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorFront ("Color Front", color) = (1,0,0,1)
        _ColorBack ("Color Back", color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"= "Transparent+10" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Front

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float4 grabPos: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _BackgroundTexture;

            fixed4 _ColorFront;
            fixed4 _ColorBack;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance * 0.05, 0, 1);

                fixed4 color = lerp(_ColorFront, _ColorBack, viewDistance);
                color.a = min(_ColorFront.a, _ColorBack.a);

                // fixed4 background = tex2Dproj(_BackgroundTexture, i.grabPos);
                // color.rgb = lerp(color.rgb, background.rgb, viewDistance);

                return color;
            }
            ENDCG
        }
    }
}
