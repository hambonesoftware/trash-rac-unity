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

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            Varyings vert(appdata_base input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.normal = UnityObjectToWorldNormal(input.normal);
                return output;
            }

            fixed4 frag(Varyings input) : SV_Target
            {
                float lambert = dot(normalize(input.normal), normalize(float3(-0.4, 0.8, -0.6)));
                float mid = smoothstep(0.04, 0.065, lambert);
                float light = smoothstep(0.56, 0.585, lambert);
                float3 cool = _Color.rgb * float3(0.60, 0.58, 0.80);
                float3 warm = _Color.rgb * float3(1.13, 1.04, 0.91);
                float3 toon = lerp(lerp(cool, _Color.rgb, mid), warm, light);
                return fixed4(lerp(toon, _Color.rgb * 1.14, _Glow), 1);
            }
            ENDCG
        }
    }
}
