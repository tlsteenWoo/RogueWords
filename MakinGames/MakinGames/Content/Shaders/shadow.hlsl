//
//
////~~~~~~~~~~~~~SHADOW~~~~~~~~~~~~~~~~
//void InstanceVertexShaderFunctionSHADOW(inout float4 position : POSITION0, out float2 depth : TEXCOORD0,
//	in float4 world : POSITION1, in float4 scale : TEXCOORD1)
//{
//	float4 posW = mul(position * scale + world, sw);
//	float4 posV = mul(posW, sv);
//	position = mul(posV, sp);
//	depth = posV.z;
//}
//float4 PixelShaderFunctionSHADOW(in float4 position: POSITION0, in float depth : TEXCOORD0) : COLOR1
//{
//	//float depth = shadow.x / shadow.y;
//	float linearDepth = -depth / shadowFar;
//return float4(linearDepth, 0, 0, 1);
//}
//
//technique Instanced
//{
//	pass Pass1
//	{
//#if SM4
//		VertexShader = compile vs_4_0 InstanceVertexShaderFunction();
//		PixelShader = compile ps_4_0 PixelShaderFunction();
//#else
//		VertexShader = compile vs_2_0 InstanceVertexShaderFunction();
//		PixelShader = compile ps_2_0 PixelShaderFunction();
//#endif
//	}
//	pass Pass2
//	{
//#if SM4
//		VertexShader = compile vs_4_0 InstanceVertexShaderFunctionSHADOW();
//		PixelShader = compile ps_4_0 PixelShaderFunctionSHADOW();
//#else
//		VertexShader = compile vs_2_0 InstanceVertexShaderFunctionSHADOW();
//		PixelShader = compile ps_2_0 PixelShaderFunctionSHADOW();
//#endif
//	}
//};