//https://digitalerr0r.wordpress.com/2009/05/16/xna-shader-programming-tutorial-20-depth-of-field/

Texture2D diffusetx;
sampler diffusesampler = sampler_state
{
	Texture = (diffusetx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

float screenWidth;
float screenHeight;
int drawmode;
float BlurDistance = 0.003f;


struct VertexShaderInput
{
	float4 Position : POSITION0;
	//float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	//float2 TexCoord : TEXCOORD0;
	//float3 Normal : TEXCOORD1;
	//float2 Depth : TEXCOORD2;
	//float3 wpos:TEXCOORD3;
	//float3 vpos:TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = input.Position;
	//float4 worldPosition = mul(input.Position, World);
	//float4 viewPosition = mul(worldPosition, View);
	//output.Position = mul(viewPosition, Projection);
	//output.TexCoord = input.TexCoord;
	//output.Normal = mul(input.Normal, World);
	//output.Depth.x = output.Position.z;
	//output.Depth.y = output.Position.w;
	//output.wpos = worldPosition.xyz;
	//output.vpos = viewPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.Position.xy / float2(screenWidth, screenHeight);
	float2 Tex = pos;// *0.5f + 0.5f;
	float4 Color = tex2D(diffusesampler, float2(Tex.x + BlurDistance, Tex.y + BlurDistance));
	Color += tex2D(diffusesampler, float2(Tex.x - BlurDistance, Tex.y - BlurDistance));
	Color += tex2D(diffusesampler, float2(Tex.x + BlurDistance, Tex.y - BlurDistance));
	Color += tex2D(diffusesampler, float2(Tex.x - BlurDistance, Tex.y + BlurDistance));
	// We need to devide the color with the amount of times we added
	// a color to it, in this case 4, to get the avg. color
	Color = Color / 4;

	switch (drawmode)
	{
	case 0: //DIFFUSE
		return Color;
		break;
	case 1: //UV
		return float4(Tex, 0, 1);
		break;
	default:
		return float4(1, 0, 1, 1);
		break;
	}
	//return float4(pos, 0, 1);
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