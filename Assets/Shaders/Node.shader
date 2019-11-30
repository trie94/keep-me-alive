Shader "Unlit/Node"
{
    Properties
    {
        _MainColor ("Color", color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        
        Pass
        {
            ZWrite On
            ColorMask 0
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
            };

            fixed4 _MainColor;
            uniform fixed _Weight;
            uniform fixed _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _MainColor;
                col.r = _Weight;
                col.a = _Opacity;
                return col;
            }
            ENDCG
        }
    }
}
