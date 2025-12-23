Shader "Custom/FadeInstanced" 
{
	Properties
	{
		_MainTex ( "Texture", 2D    ) = "white" {}
		_Color   ( "Color",   Color ) = ( 1, 1, 1, 1 )
		_Offset  ( "Offset",  Float ) = 1.0
	}

	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert  
			#pragma multi_compile_instancing
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4    _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				fixed4 color  : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos   : SV_POSITION;
				float2 uv    : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4,  _Color  )
				UNITY_DEFINE_INSTANCED_PROP( float,   _Offset )
			UNITY_INSTANCING_BUFFER_END( Props )

			v2f vert( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );

				float3 pos = v.vertex.xyz;
				pos.y += 0.5;

				o.pos = UnityObjectToClipPos( float4(pos, 1.0 ) );//v.vertex );
                o.uv = TRANSFORM_TEX( v.uv, _MainTex );
                o.color = v.color * UNITY_ACCESS_INSTANCED_PROP( Props, _Color );

				return o;
			}

			fixed4 frag ( v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i );
				float offset = UNITY_ACCESS_INSTANCED_PROP( Props, _Offset );
				fixed4 col   = tex2D( _MainTex, i.uv ) * i.color;

				float  alpha = smoothstep( 1.0, 1.0 - offset, i.uv.y );
				col.a *= alpha;
				
				return col;
			}
			ENDCG
		}
	}
}