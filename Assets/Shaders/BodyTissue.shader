Shader "Unlit/BodyTissue"
{
    Properties
    {
        _HeadColor ("Head Color", color) = (1,0,0,1)
        _TailColor ("Tail Color", color) = (1,0,0,1)
        _Face ("Face", 2D) = "black" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "white" {}
        
        _GradientPower ("Gradient Power", Range(0.1, 5.0)) = 1.0
        _BodyLength ("Length", Range(1, 20)) = 1.0
        _BodyThickness ("Thickness", Range(1.0, 3.0)) = 1.0

        _Cap ("Cap", Range(-1.0, 1.6)) = 0.0

        _EatingProgress("Eating Progress", Range(0.0, 1.0)) = 0.5
        _FoodSize("Food Size", Range(0.0, 2.0)) = 1.0
        _FoodRange("Food Range", Range(0.0, 0.5)) = 0.03
        _FogColor("FogColor", color) = (0.9294118,0.4901961,0.4392157,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                float3 worldNormal : NORMAL;
                float4 worldPos : TEXCOORD1;
                float4 localPos : TEXCOORD2;
            };

            sampler2D _Face;
            float4 _Face_ST;
            sampler2D _Ramp;
            
            float _BodyLength;
            float _Cap;
            fixed _BodyThickness;

            fixed4 _HeadColor;
            fixed4 _TailColor;
            fixed _GradientPower;

            fixed _EatingProgress;
            fixed _FoodSize;
            fixed _FoodRange;

            fixed4 _FogColor;

            // bone animation
            uniform float4x4 _Frames[10];

            v2f vert (appdata v)
            {
                v2f o;
                float3 localPos = v.vertex;
                o.localPos = v.vertex;
                
                fixed originalBodyHeight = 3;
                localPos.xy *= _BodyThickness;

                fixed eatingProgress = (1-_EatingProgress);
                fixed p = (localPos.z + 1.5) / 3.0;
                fixed mid = (eatingProgress * 2 + _FoodRange)/2;
                fixed food = smoothstep(-_FoodRange, _FoodRange, _FoodRange-abs(mid - p)) * _FoodSize + 1;
                localPos.xy *= food;

                // skinning
                uint numFrame = 10;
                float offset = -1.5;
                float progress = clamp((localPos.z + 0.95) / 2.0 * numFrame + offset, 0, numFrame - 1);
                uint currIndex = clamp(floor(progress), 0, numFrame - 1);
                uint nextIndex = clamp(ceil(progress), 0, numFrame - 1);

                float weight0 = abs(progress - nextIndex);
                float weight1 = abs(progress - currIndex);

                float cap = abs(localPos.z) - 1.0;
                float c = step(0, cap);
                localPos.z = cap * _Cap * sign(localPos.z) * c + localPos.z;

                float3 worldPos0 = mul(_Frames[currIndex], float4(localPos, 1.0)).xyz;
                float3 worldPos1 = mul(_Frames[nextIndex], float4(localPos, 1.0)).xyz;

                float3 worldPos = lerp(worldPos0, worldPos1, weight1);
                o.worldPos = float4(worldPos, 1.0);

                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.vertex = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _Face);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float lerpFactor = pow(saturate(i.localPos.z/3.0+0.5), _GradientPower);
                fixed4 col = lerp( _TailColor, _HeadColor, lerpFactor);
                fixed4 face = tex2D(_Face, i.uv);
                col.rgb = face.rgb * face.a + col * (1-face.a);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = saturate(dot(normalize(i.worldNormal), lightDir));
                float4 lighting = float4(tex2D(_Ramp, float2(ramp, 0.5)).rgb, 1.0);
                col *= lighting;
                
                float viewDistance = length(i.worldPos.xyz - _WorldSpaceCameraPos);

                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                    col.rgb = lerp(_FogColor, col.rgb, saturate(unityFogFactor));
                #endif
                return col;
            }
            ENDCG
        }
    }
}
