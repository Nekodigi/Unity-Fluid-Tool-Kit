// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/GUI" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
    	Pass 
    	{
			ZTest Always

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature BLEND_FROM_RGB
			#pragma shader_feature DENSITY_GRADIENT

			sampler2D _MainTex;
			sampler2D _Obstacles;
			sampler2D _Gradient;
			float4 _FluidColor, _ObstacleColor;
		
			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = UnityObjectToClipPos(v.vertex);
    			OUT.uv = v.texcoord.xy;
    			return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{
				//float3 col = tex2D(_Gradient, float2(tex2D(_MainTex, IN.uv).x, 0));//tex2D(_MainTex, IN.uv).x/4.0

				float4 col = tex2D(_MainTex, IN.uv);

				#ifdef DENSITY_GRADIENT
				col = tex2D(_Gradient, float2(tex2D(_MainTex, IN.uv).w, 0));//+ float4(IN.uv.x, 0, 0, 0);
				#endif

			 	float obs = tex2D(_Obstacles, IN.uv).x;
			 	
			 	float4 result = lerp(col, _ObstacleColor, obs);

				#ifdef BLEND_FROM_RGB
			 	result.a = (result.r+result.g+result.b)/3.0;
				#endif

				return result;
			}
			
			ENDCG

    	}
	}
}
