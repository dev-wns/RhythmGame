Shader "Unlit/ColorGradient Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorUp   ("ColorUp",   Color) = (1,1,1,1)
        _ColorDown ("ColorDown", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha

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
                fixed4 c :COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 c : COLOR;
                fixed a : COLOR1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorUp;
            float4 _ColorDown;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.c = lerp(_ColorDown, _ColorUp, v.uv.y);
                o.a = lerp(0, 1, v.uv.y);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.c;
            }
            ENDCG
        }
    }
}
