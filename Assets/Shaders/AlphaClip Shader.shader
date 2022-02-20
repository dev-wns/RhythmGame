Shader "Unlit/AlphaClip Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", Range(0, 1)) = .1
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 100
            
            Cull Off
            Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alpha

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
            float4 _MainTex_ST;
            half _Offset;

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
                fixed4 c = tex2D(_MainTex, i.uv) * i.c;
                half rgbSquared = dot(c.rgb, c.rgb);
                if (rgbSquared <= _Offset)
                    discard;

                return c;

                // half alpha = c.r + c.g + c.b - _Offset;
                // clip(alpha);
                // 
                // return fixed4(c.rgb, alpha);
            }
            ENDCG
        }
    }
}
