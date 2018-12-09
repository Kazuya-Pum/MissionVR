Shader "Custom/Barrior" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Alpha ("Alpha", Range(0,1)) = 0.0
		_EmissionTex ("EmissionTex", 2D) = "white" {}
		_EmiColor ("EmiColor", Color) = (1,1,1,1)
		_AlphaTex ("Alpha", 2D) = "white" {}
		_AlphaTex2 ("Alpha2", 2D) = "white" {}
		_AlphaTex3 ("Alpha3", 2D) = "white" {}
		_MainTiling ("Tiling", Vector     ) = (1, 0, 0, 0)
	}
	SubShader {
		Tags { "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _AlphaTex;
		sampler2D _AlphaTex2;
		sampler2D _AlphaTex3;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		half _Alpha;
		fixed4 _Color;
		fixed4 _EmiColor;
		half4 _MainTiling;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		//UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		//UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex * _MainTiling.x) * _Color;
			fixed4 emi = tex2D (_EmissionTex, IN.uv_MainTex * _MainTiling.x) * _EmiColor;
			fixed4 alpha = tex2D (_AlphaTex, IN.uv_MainTex);
			fixed4 alpha2 = tex2D (_AlphaTex2, IN.uv_MainTex * _MainTiling.x);
			fixed4 alpha3 = tex2D (_AlphaTex3, IN.uv_MainTex * _MainTiling.x);
			fixed t = 0.5 * sin(_Time * 20) + 0.5;
			 
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			fixed4 a = (c.a + _Alpha + alpha2.a * t + alpha3.a * (1.0 - t))* alpha.a;
			o.Alpha = a;
			o.Emission = emi.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
