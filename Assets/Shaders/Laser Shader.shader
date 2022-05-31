Shader "Unlit/Laser Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        LaserTex("LaserTex", 2D) = "white" {}
        Speed("Speed", float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        LOD 100

        Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 c : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 c : COLOR;
            };

            sampler2D _MainTex;
            sampler2D LaserTex;
            float4 _MainTex_ST;
            float Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.c = v.c;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv + float2(-_Time.x * Speed, 0)) * tex2D(LaserTex, i.uv) * i.c;
                return col;
            }
            ENDCG
        }
    }
}
