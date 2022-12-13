Shader "Unlit/Sum Sprite Shader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
        _SubTex("Texture", 2D) = "black" {}
        _AlphaColor("AlphaColor", Color) = (0,0,0,0)
        _AlphaColorCutoff("AlphaColorCutoff", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        //Pass
        //{
        //    SetTexture[_MainTex]
        //    {
        //        combine texture
        //    }

        //    SetTexture[_SubTex]
        //    {
        //        combine texture lerp(texture) previous
        //    }
        //}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SubTex;
            float4 _SubTex_ST;

            float4 _AlphaColor;
            float _AlphaColorCutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _SubTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col0 = tex2D(_MainTex, i.uv0);
                fixed4 col1 = tex2D(_SubTex,  i.uv1);
                //col1 = col1.rgba * col1.a;
                //if ((abs(col1.r - _AlphaColor.r) < _AlphaColorCutoff) &&
                //    (abs(col1.g - _AlphaColor.g) < _AlphaColorCutoff) &&
                //    (abs(col1.b - _AlphaColor.b) < _AlphaColorCutoff))
                //{
                //    col1.a = 0;
                //}

                return fixed4( lerp(col0, col1, col1.a).rgb, 1);
                //return col1 * col1.a;
                //fixed r, g, b;
                //if (0.125 < col1.r) r = col1.r;
                //else                r = col0.r;
                //
                //if (0.125 < col1.g) g = col1.g;
                //else                g = col0.g;
                //
                //if (0.125 < col1.b) b = col1.b;
                //else                b = col0.b;

                //return fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
