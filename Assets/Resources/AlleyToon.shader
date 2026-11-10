Shader "TrashRac/AlleyToon"
{
    Properties
    {
        _Color ("Base color", Color) = (1,1,1,1)
        _Ink ("Ink", Color) = (0.055,0.044,0.088,1)
        _Outline ("Outline", Range(0,0.04)) = 0.008
        _Glow ("Glow", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "INK"
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Outline;
            fixed4 _Ink;

            float4 vert(appdata_base input) : SV_POSITION
            {
                float3 axisScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22));
                input.vertex.xyz += normalize(input.vertex.xyz) * _Outline / max(axisScale, 0.001);
                return UnityObjectToClipPos(input.vertex);
            }

            fixed4 frag() : SV_Target
            {
                return _Ink;
            }
            ENDCG
        }

        Pass
        {
            Name "COLOR"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Glow;
            float _StockroomWake;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 world : TEXCOORD1;
            };

            Varyings vert(appdata_base input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.normal = UnityObjectToWorldNormal(input.normal);
                output.world = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }

            fixed4 frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normal);
                float lambert = dot(n, normalize(float3(-0.54, 0.73, -0.53)));
                float mid = smoothstep(0.10, 0.13, lambert);
                float light = smoothstep(0.65, 0.68, lambert);
                float3 cool = _Color.rgb * float3(0.49, 0.41, 0.72) + float3(0.038, 0.024, 0.079);
                float3 warm = _Color.rgb * float3(1.17, 1.10, 0.95);
                float3 toon = lerp(lerp(cool, _Color.rgb, mid), warm, light);
                float edge = smoothstep(0.43, 0.56, dot(n, normalize(float3(0.88, 0.38, 0.26))));
                toon += edge * float3(0.075, 0.060, 0.126);
                float3 toDoor = float3(-3.72, 1.74, 1.93) - input.world;
                float spill = 1 - smoothstep(0.75, 3.0, length(toDoor));
                float catchLight = smoothstep(-0.20, 0.68, dot(n, normalize(toDoor + 0.001)));
                toon += _StockroomWake * spill * catchLight * float3(0.28, 0.13, 0.035);
                return fixed4(lerp(toon, _Color.rgb * 1.14, _Glow), 1);
            }
            ENDCG
        }
    }
}
