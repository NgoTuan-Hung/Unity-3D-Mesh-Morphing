Shader "Custom/TextureShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;

        void vert(inout appdata_full v)
        {
            // No vertex transformation here
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Sample textures
            float4 texColor = tex2D(_MainTex, IN.uv_MainTex);

            // Output final color with normal
            o.Albedo = texColor.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
