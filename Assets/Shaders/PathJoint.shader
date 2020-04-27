Shader "Unlit/PathJoint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", color) = (1,0,0,1)
        _VeinColor ("Vein Color", color) = (1,0,0,1)
        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"= "Transparent+10" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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

            fixed4 _Color;
            fixed4 _VeinColor;
            fixed4 _FogColor;

            uniform float4x4 _CylinderInverseTransform[20];
            uniform float4 _CylinderDimension[20];
            uniform int _CylinderNum;

            uniform float4x4 _ZoneInverseTransform[5];
            uniform float _ZoneRadius[5];
            uniform int _ZoneNum;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
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

            float sdSphere(float3 p, float s)
            {
                return length(p)-s;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                float4 worldPosition = float4(i.worldPos.xyz, 1);
                float3 normal = i.normal;
                float2 uv = i.uv;

                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor));
                #endif

                bool bye = false;

                for (uint i=0; i<_CylinderNum; i++)
                {
                    fixed overlap = sdCappedCylinder(mul(_CylinderInverseTransform[i], worldPosition), _CylinderDimension[i].z, _CylinderDimension[i].x);
                    if (overlap < -0.05) {
                        bye = true;
                        break;
                    }
                }
                if (bye) discard;

                for (uint i=0; i<_ZoneNum; i++)
                {
                    fixed overlap = sdSphere(mul(_ZoneInverseTransform[i], worldPosition), _ZoneRadius[i]);
                    if (overlap < -0.07) {
                        bye = true;
                        break;
                    }
                }
                if (bye) discard;
                return col;
            }
            ENDCG
        }
    }
}
