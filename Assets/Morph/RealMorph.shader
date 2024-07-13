Shader "Unlit/RealMorph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MorphTex ("Morph Texture", 2D) = "white" {}
        _Triangle1TravelDistance ("Triangle Travel Distance", Float) = 0.
        _TimeScale ("Time Scale", Float) = 1.
        _TimeOffset ("Time Offset", Float) = 0.
        [HDR] _GlowColor ("Glow Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float triangleID : TEXCOORD1;
                float2 morphUV : TEXCOORD2;
            };

            struct PerTriangleData
            {
                float3 v0 : POSITION;
                float3 v1 : POSITION;
                float3 v2 : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float3 normal1 : NORMAL;
                float3 normal2 : NORMAL;
                float3 normal3 : NORMAL;
                float3 triangle1TravelDirection : POSITION;
                float3 v0MaxPos : POSITION;
                float3 v1MaxPos : POSITION;
                float3 v2MaxPos : POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MorphTex;
            float4 _MorphTex_ST;
            float _Triangle1TravelDistance;
            float _TimeScale;
            float _TimeOffset;
            float4 _GlowColor;
            StructuredBuffer<PerTriangleData> _PerTriangleData;

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triangleStream, uint triangleID: SV_PRIMITIVEID)
            {
                float fakeTime = (_Time.y - _TimeOffset) * _TimeScale % 2;
                float cond = step(1, fakeTime);
                g2f o;
                o.vertex = UnityObjectToClipPos
                (
                    (1 - cond) * (IN[0].vertex + _PerTriangleData[triangleID].triangle1TravelDirection * fakeTime) +
                    cond* lerp(_PerTriangleData[triangleID].v0MaxPos, _PerTriangleData[triangleID].v0, fakeTime - 1)
                );
                o.uv = IN[0].uv;
                o.normal = _PerTriangleData[triangleID].normal1;
                o.morphUV = _PerTriangleData[triangleID].uv0;
                o.triangleID = triangleID;
                triangleStream.Append(o);

                o.vertex = UnityObjectToClipPos
                (
                    (1 - cond) * (IN[1].vertex + _PerTriangleData[triangleID].triangle1TravelDirection * fakeTime) +
                    cond * lerp(_PerTriangleData[triangleID].v1MaxPos, _PerTriangleData[triangleID].v1, fakeTime - 1)
                );
                o.uv = IN[1].uv;
                o.normal = _PerTriangleData[triangleID].normal2;
                o.morphUV = _PerTriangleData[triangleID].uv1;
                o.triangleID = triangleID;
                triangleStream.Append(o);

                o.vertex = UnityObjectToClipPos
                (
                    (1 - cond) * (IN[2].vertex + _PerTriangleData[triangleID].triangle1TravelDirection * fakeTime) +
                    cond * lerp(_PerTriangleData[triangleID].v2MaxPos, _PerTriangleData[triangleID].v2, fakeTime - 1)
                );
                o.uv = IN[2].uv;
                o.normal = _PerTriangleData[triangleID].normal3;
                o.morphUV = _PerTriangleData[triangleID].uv2;
                o.triangleID = triangleID;
                triangleStream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                float fakeTime = (_Time.y - _TimeOffset) * _TimeScale % 2;
                float cond = 1 - step(1, fakeTime);

                fixed4 col = cond * lerp(tex2D(_MainTex, i.uv), _GlowColor, fakeTime) + (1 - cond) * lerp(_GlowColor, tex2D(_MorphTex, i.morphUV), fakeTime - 1);
                return col;
            }
            ENDCG
        }
    }
}
