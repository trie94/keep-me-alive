Shader "Unlit/LobbyPathTube"
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
        _PulseDirection("Pulse Direction", Vector) = (0,0,0,1)

        _WiggleSpeed ("Wiggle Speed", Range(0, 20)) = 0.5
        _WiggleAmount ("Wiggle Amount", Range(0, 2)) = 0.2

        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"= "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
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
            #pragma multi_compile_fog

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

            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _PulseColor;
            fixed4 _FogColor;

            sampler2D _Ramp;

            int _Tiling;
            float _WarpScale;
            float _WarpTiling;
            float _DistanceBetweenLines;
            float _PulseFreq;
            float _PulseBrightness;
            float4 _PulseDirection;
            float _PulseSpeed;

            float _WiggleSpeed;
            float _WiggleAmount;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                float pulseDir = dot(_PulseDirection, normalize(worldPos.xyz - _WorldSpaceCameraPos));
                float pulse = pow(saturate(sin(pulseDir - _Time.y * _PulseFreq) * _PulseBrightness), _PulseSpeed);
                float amount = sin(v.vertex.z + (_Time.x * _WiggleSpeed) * pulse) * _WiggleAmount + 1;
                v.vertex.xz *= amount;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;
                o.worldPos = worldPos;
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 worldPosition = float4(i.worldPos.xyz, 1);
                float3 normal = i.normal;
                float2 uv = i.uv;
                float3 localPosition = i.localPos;
                float4 grabPosition = i.grabPos;

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);

                float pos = uv.x * _Tiling;
                pos += sin(snoise(uv.xyy * _WarpTiling)) * _WarpScale;
                fixed value = floor(frac(pos) + _DistanceBetweenLines);

                float pulseDir = dot(_PulseDirection, normalize(i.worldPos.xyz - _WorldSpaceCameraPos));
                float pulse = pow(saturate(sin(pulseDir - _Time.y * _PulseFreq) * _PulseBrightness), _PulseSpeed);

                fixed4 lineColor = lerp(_Color1, _Color2, saturate(abs(0.5-uv.y) *_Tiling - 1));
                lineColor = lineColor + pulse * _PulseColor;
                fixed4 col = lerp(lineColor, _Color2, value);

                col = lerp(col, fixed4(1,1,1,1), clamp(localPosition.z / 2, 0, 1));
                col.a = clamp(lerp(0, 1, localPosition.z+1/2) + 0.5, 0.5, 1);
                return col;
            }
            ENDCG
        }
    }
}
