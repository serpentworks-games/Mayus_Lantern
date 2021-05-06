Shader "Toon/Ice Refracted" {
	Properties{
		_Color("Main Color", Color) = (0.49,0.94,0.64,1)// 
		_TColor("Top Color", Color) = (0.49,0.94,0.64,1)// 
		_BottomColor("Bottom Color", Color) = (0.23,0,0.95,1)// bottom gradient
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}	
		_Offset("Gradient Offset", Range(0,4)) = 3.2 
		_RimBrightness("Rim Brightness", Range(3,6)) = 3.2 
		_BumpAmt("Distortion", range(0,128)) = 10
		_BumpMap("Normalmap", 2D) = "bump" {}
		[Toggle(ALPHA)] _ALPHA("Enable Alpha?", Float) = 0
	}

		SubShader{
		Tags{ "RenderType" = "Transparent" }

		UsePass "Toon/Ice Effect/FORWARD"
		UsePass "FX/Glass/Ice Refraction/BASE"
		
	}

		Fallback "Toon/Lit"
}
