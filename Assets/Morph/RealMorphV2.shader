Shader "Custom/RealMorphV2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MorphTex ("Morph Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Float) = 1.
        _TimeOffset ("Time Offset", Float) = 0.
        [HDR] _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _MainColor("Color Tint", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(0,3)) = 1
		_RimBrightness("Rim Brightness", Range(0, 3)) = 1
		_Alpha("Alpha", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha	
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
                float3 worldNormal : NORMAL;
                float triangleID : TEXCOORD1;
                float2 morphUV : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float3 worldVertex: TEXCOORD4;
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MorphTex;
            float4 _MorphTex_ST;
            float _TimeScale;
            float _TimeOffset;
            float4 _GlowColor;
            float4 _MainColor;
			float4 _RimColor; 
			half _RimPower;
			half _RimBrightness;
			fixed _Alpha;
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
                float fakeTime = (_Time.y - _TimeOffset) * _TimeScale / 2;
                float3 v;
                g2f o;

                o.vertex = UnityObjectToClipPos
                (
                    v = lerp(IN[0].vertex, _PerTriangleData[triangleID].v0, fakeTime)
                );
                o.uv = IN[0].uv;
                o.worldVertex = mul(unity_ObjectToWorld, v);
                o.worldNormal = UnityObjectToWorldNormal(_PerTriangleData[triangleID].normal1);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldVertex.xyz));
                o.morphUV = _PerTriangleData[triangleID].uv0;
                o.triangleID = triangleID;
                triangleStream.Append(o);

                o.vertex = UnityObjectToClipPos
                (
                    v = lerp(IN[1].vertex, _PerTriangleData[triangleID].v1, fakeTime)
                );
                o.uv = IN[1].uv;
                o.worldVertex = mul(unity_ObjectToWorld, v);
                o.worldNormal = UnityObjectToWorldNormal(_PerTriangleData[triangleID].normal2);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldVertex.xyz));
                o.morphUV = _PerTriangleData[triangleID].uv1;
                o.triangleID = triangleID;
                triangleStream.Append(o);

                o.vertex = UnityObjectToClipPos
                (
                    v = lerp(IN[2].vertex, _PerTriangleData[triangleID].v2, fakeTime)
                );
                o.uv = IN[2].uv;
                o.worldVertex = mul(unity_ObjectToWorld, v);
                o.worldNormal = UnityObjectToWorldNormal(_PerTriangleData[triangleID].normal3);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldVertex.xyz));
                o.morphUV = _PerTriangleData[triangleID].uv2;
                o.triangleID = triangleID;
                triangleStream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                float fakeTime = (_Time.y - _TimeOffset) * _TimeScale % 2;
                fixed4 col;

                if (fakeTime < 1)
                {
                    col = lerp(tex2D(_MainTex, i.uv), _GlowColor, fakeTime);
                }
                else
                {
                    fixed4 tex = tex2D(_MorphTex, i.morphUV);
                    half rim = 1 - saturate(dot(i.viewDir, i.worldNormal));
                    fixed4 rimColor = _RimColor * pow(rim, _RimPower);
                    col = tex * _MainColor + rimColor;
                    col.a = _Alpha;
                    col.rgb *= _RimBrightness;

                    col = lerp(_GlowColor, col, fakeTime - 1);
                }

                return col;
            }
            ENDCG
        }
    }
}
