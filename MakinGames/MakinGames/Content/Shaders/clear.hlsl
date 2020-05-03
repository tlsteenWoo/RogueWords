void VertexShaderFunction(in float4 pos : POSITION0)
{
	pos = pos;
}

void PixelShaderFunction(in float4 pos :POSITION0, out float4 color0:COLOR0, out float4 color1:COLOR1)
{
	color0 = float4(0,0,0,1);
	color1 = float4(1,0,0,1);
}

technique Ambient
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
#endif
	}
}