Shader "Unlit/Contour"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DiffuseTexture;

            float checkSame(float4 center, float4 samplef)
            {
                float2 centerNormal = center.xy;
                float centerDepth = center.z;
                float2 sampleNormal = samplef.xy;
                float sampleDepth = samplef.z;
                
                float2 diffNormal = abs(centerNormal - sampleNormal);
                bool isSameNormal = (diffNormal.x + diffNormal.y) < 0.1;
                
                float diffDepth = abs(centerDepth - sampleDepth);
                bool isSameDepth = diffDepth < 0.1;
                return (isSameNormal && isSameDepth) ? 1.0 : 0.0;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 sample1 = tex2D(_MainTex, i.uv + float2(1.0, 1.0) / _ScreenParams.xy);
                float4 sample2 = tex2D(_MainTex, i.uv + float2(-1.0, -1.0) / _ScreenParams.xy);
                float4 sample3 = tex2D(_MainTex, i.uv + float2(-1.0, 1.0) / _ScreenParams.xy);
                float4 sample4 = tex2D(_MainTex, i.uv + float2(1.0, -1.0) / _ScreenParams.xy);

                float edge = checkSame(sample1, sample2) * checkSame(sample3, sample4);
                fixed4 col = fixed4(edge, 1.0, 1.0, 1.0);

                // float4 test = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
