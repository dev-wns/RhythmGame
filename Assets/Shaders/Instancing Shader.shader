Shader "Custom/InstancingShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            ZWrite Off
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color  : COLOR;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                float4 localPos = v.vertex;
                float4 worldPos = mul(unity_ObjectToWorld, localPos);
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = v.uv;
                o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}