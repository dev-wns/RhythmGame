Shader "Custom/AlphaClip Shader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            
            fixed alpha = c.r + c.g + c.b - .1f;
            clip(alpha);
            
            o.Emission = c.rgb;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
