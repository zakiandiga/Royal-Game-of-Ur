// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Unlit/Surface"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Transparency("Transparency", Range(0,0.5)) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
		ZWrite Off
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 world : POSITION1;
				float4 center : POSITION2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			float _Transparency;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.world = mul(unity_ObjectToWorld, v.vertex);
				o.center = mul(unity_ObjectToWorld, fixed4(0, 0, 0, 1));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float len = length(i.world.xyz - i.center.xyz);
				//len = normalize(len);
				fixed4 col = tex2D(_MainTex, i.uv);
				col = _TintColor;
				col.a = clamp(((_Transparency/len)*0.01)-0.05,0,0.2);

				return col;
			}
			ENDCG
		}
	}
}
