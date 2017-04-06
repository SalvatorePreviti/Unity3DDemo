Shader "Custom/TapeShader" {
	Properties {
		_charactersInTexture("Characters in texture", int) = 7
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
			float2 uv_MainTex;
			float4 screenPos;
		};

		static const int CellsPerChunk = 32;
		static const int CharactersInTexture = 7;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _TapeMemory[CellsPerChunk];

		void surf (Input IN, inout SurfaceOutputStandard o) {
			int cellIndex = IN.uv_MainTex.y * CellsPerChunk;

			int xchar = _TapeMemory[cellIndex];

			float2 uv = float2(
				(IN.uv_MainTex.x + xchar % CharactersInTexture) / CharactersInTexture,
				1 - ((1 - (IN.uv_MainTex.y * CellsPerChunk - cellIndex)) + xchar / CharactersInTexture) / CharactersInTexture
			);

			fixed4 c = tex2D (_MainTex, uv) * _Color;

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
