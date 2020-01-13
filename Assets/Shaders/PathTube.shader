Shader "Unlit/PathTube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color1 ("Color1", color) = (1,0,0,1)
        _Color2 ("Color2", color) = (0,1,0,1)
        _PulseColor ("Pulse Color", color) = (1,0,0,1)
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        
        _Tiling ("Tiling", Range(0, 20)) = 1
		_WarpScale ("Warp Scale", Range(0, 1)) = 0
		_WarpTiling ("Warp Tiling", Range(0, 10)) = 1
        _DistanceBetweenLines ("Distance Between Lines", Range(0, 1)) = 0.5

        _PulseFreq ("Pulse Freq", Range(0, 10)) = 0.5
        _PulseBrightness ("Pulse Brightness", Range(0, 1)) = 0.5
        _PulseSpeed ("Pulse Speed", Range(0, 50)) = 0.5
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
                float4 worldPos : TEXCOORD1;
                float4 grabPos: TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 localPos : TEXCOORD4;
                float3 worldNormal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _BackgroundTexture;

            fixed4 _Color1;
			fixed4 _Color2;
            fixed4 _PulseColor;
            sampler2D _Ramp;

			int _Tiling;
			float _WarpScale;
			float _WarpTiling;
            float _DistanceBetweenLines;
            float _PulseFreq;
            float _PulseBrightness;
            float4 _PulseDirection;
            float _PulseSpeed;

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
                o.localPos = v.vertex.xyz;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
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
                float4 worldPosition = float4(i.worldPos.xyz, 1);
                float3 normal = i.normal;
                float2 uv = i.uv;
                float3 localPosition = i.localPos;
                float3 worldNormal = i.worldNormal;
                float4 grabPosition = i.grabPos;

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

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);

                float pos = uv.y * _Tiling;
                pos += sin(snoise(uv.xxy * _WarpTiling)) * _WarpScale;
	            fixed value = floor(frac(pos) + _DistanceBetweenLines);

                float pulseDir = dot(_PulseDirection, normalize(worldPosition.xyz - _WorldSpaceCameraPos));
                float pulse = pow(saturate(sin(pulseDir - _Time.y * _PulseFreq) * _PulseBrightness), _PulseSpeed);
                fixed4 lineColor = lerp(_Color1, _Color2, saturate(abs(0.5-uv.x) *_Tiling - 1));
                lineColor = lineColor + pulse * _PulseColor;

                // if (abs(uv.x-0.5)>0.2) 
                fixed4 color = lerp(lineColor, _Color2, value);

                float viewDistance = length(worldPosition.xyz - _WorldSpaceCameraPos);
                viewDistance = clamp(viewDistance * 0.03, 0, 1);

                fixed4 background = tex2Dproj(_BackgroundTexture, grabPosition);
                color.rgb = lerp(color.rgb, background.rgb, viewDistance);

				return color;
            }
            ENDCG
        }
    }
}
