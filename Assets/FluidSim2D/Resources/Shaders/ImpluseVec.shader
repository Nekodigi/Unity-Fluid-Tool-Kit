// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FluidSim/ImpluseVec" 
{
	SubShader 
	{
    	Pass 
    	{
			ZTest Always
			//Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float2 _Point;
			uniform float _Radius;
			uniform float4 _Fill;
			uniform sampler2D _Source;
			float _Aspect;
			
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
				float2 diff = _Point - IN.uv;
				diff.x *= _Aspect;
				float d = length(diff);
			    
				float impulse = 0;
			    
			    if(d < _Radius) 
			    {
			        float a = (_Radius - d) * 0.5;
					impulse = min(a, 1.0);
			    } 

				float4 source = tex2D(_Source, IN.uv);
			  
				return lerp(source, _Fill, impulse);//lerp(source, _Fill, impulse)
			}
			
			ENDCG

    	}
	}
}