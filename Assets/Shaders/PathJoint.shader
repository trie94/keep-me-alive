Shader "Unlit/PathJoint"
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
        // Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        // Cull Front
        Cull Off

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
            #include "Helper.cginc"

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
                float4 worldPos : TEXCOORD1;
                float4 grabPos: TEXCOORD2;
                float3 normal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _BackgroundTexture;

            fixed4 _ColorFront;
            fixed4 _ColorBack;

            uniform float4x4 _CylinderInverseTransform[20];
            uniform float4 _CylinderDimension[20];
            uniform int _CylinderNum;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            float sdCylinder(float3 p, float3 c )
            {
                return length(p.xy-c.xy)-c.z;
            }

            float sdCappedCylinder(float3 p, float h, float r )
            {
                float2 d = abs(float2(length(p.xy),p.z)) - float2(r,h);
                return min(max(d.x,d.y),0.0) + length(max(d,0.0));
            }

            float sdSphere(float3 p, float s )
            {
                return length(p)-s;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance * 0.05, 0, 1);

                fixed4 color = lerp(_ColorFront, _ColorBack, viewDistance);

                // fixed4 background = tex2Dproj(_BackgroundTexture, i.grabPos);
                // color.rgb = lerp(color.rgb, background.rgb, viewDistance);
                float4 worldPosition = float4(i.worldPos.xyz, 1);
                float3 normal = i.normal;

                for (uint i=0; i<_CylinderNum; i++)
                {
                    fixed overlap = sdCappedCylinder(mul(_CylinderInverseTransform[i], worldPosition), _CylinderDimension[i].z, _CylinderDimension[i].x);
                    if (overlap < 0) {
                        // return fixed4(0,0,0,0);
                        discard;
                    }
                }
                return float4(normal * 0.5 + 0.5, 1);
                return _ColorFront;
            }
            ENDCG
        }
    }
}
