Shader "Custom/FadeNote" 
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
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4    _MainTex_ST;
			
			fixed4    _Color;
			float     _Offset;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				fixed4 color  : COLOR;
			};

			struct v2f
			{
				float4 pos   : SV_POSITION;
				float2 uv    : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert( appdata v )
			{
				v2f o;
				o.pos = UnityObjectToClipPos( v.vertex );
				o.uv  = TRANSFORM_TEX( v.uv, _MainTex );
				return o;
			}

			fixed4 frag ( v2f i ) : SV_Target
			{
				fixed4 col   = tex2D( _MainTex, i.uv ) * _Color;
				float  alpha = smoothstep( 1.0, 1.0 - _Offset, i.uv.y );
				col.a *= alpha;
				
				return col;
			}
			ENDCG
		}
	}
}