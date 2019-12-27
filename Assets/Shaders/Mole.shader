Shader "Unlit/Mole"
{
    Properties
    {
        _Color ("Color", color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
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

            #include "UnityCG.cginc"
            #include "Noise.cginc"

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
            };

            fixed4 _Color;
            sampler2D _BackgroundTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance/15, 0, 1);
                fixed4 background = tex2D(_BackgroundTexture, i.worldPos.xy);
                col.rgb = lerp(col.rgb, background.rgb, viewDistance);

                return col;
            }
            ENDCG
        }
    }
}
