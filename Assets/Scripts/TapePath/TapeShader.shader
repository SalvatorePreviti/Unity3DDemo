Shader "Custom/TapeShader" {
	Properties {
		_Color ("Color", Color) = (1,0,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float4 screenPos;
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
		 
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			//fixed cellPos = fmod(IN.screenPos.z, 1);

			//c.r = 0;

			float p = abs(fmod(IN.uv_MainTex.y, 1.0));

			float sep = p - 0.2 > 0 ? p + frac(IN.screenPos.z) : 0;
			if (sep > 1)
				sep = 1;




			//c.r = frac(IN.screenPos.y);
			c.rgb *= sep;

			o.Albedo = c.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
