//http://rbwhitaker.wikidot.com/first-shader

float4x4 orientation;
float4x4 world;
float4x4 view;
float4x4 projection;
float4x4 viewRotation;
float4x4 nonLinearMatrices[4];
float far;
float2 linearMinsPlusRanges[4];

//~~~~~~~~~~~~~SHADOW~~~~~~~~~~~~~~~~
float4x4 sw;
float4x4 sv;
float4x4 sp;
float shadowFar;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal: NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 Depth : TEXCOORD1;
	float2 NonLinears[4]:TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 posW = mul(input.Position, world);
	float4 posV = mul(posW, view);
	output.Position = mul(posV, projection);
	float4x4 modelView = mul(world, view);
	//output.Normal = mul(input.Normal, (float3x3)modelView);
	output.Normal = mul(input.Normal, orientation);
	output.Depth.z = posV.z;
	output.Depth.xy = output.Position.zw;
	for (int i = 0; i < 4; ++i)
	{
		float4 proj = mul(posV, nonLinearMatrices[i]);
		output.NonLinears[i] = proj.zw;
	}

	return output;
}
VertexShaderOutput InstanceVertexShaderFunction(VertexShaderInput input,
	in float4 ipos : POSITION1, in float4 iscale :TEXCOORD1)
{
	VertexShaderOutput output;

	float4 posW = mul(input.Position * iscale + ipos, world);
	float4 posV = mul(posW, view);
	output.Position = mul(posV, projection);
	output.Depth.z = posV.z;
	output.Depth.xy = output.Position.zw;
	output.Normal = input.Normal;
	//output.Normal = mul(input.Normal, (float3x3)viewRotation);
	for (int i = 0; i < 4; ++i)
	{
		float4 proj = mul(posV, nonLinearMatrices[i]);
		output.NonLinears[i] = proj.zw;
	}

	return output;
}

struct PixelShaderOutput
{
	float4 color:COLOR0;
	float4 depths:COLOR1;
};


float4 encode(float3 n)
{
	float scale = 1.7777;
	float2 enc = n.xy / (n.z + 1);
	enc /= scale;
	enc = enc*0.5 + 0.5;
	return float4(enc, 0, 0);
}
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	float linearDepth = -input.Depth.z / far;
	float nonLinearDepth = input.Depth.x / input.Depth.y;
	float4 color = encode(input.Normal);
	color.z = nonLinearDepth;
	color.w = linearDepth;
	output.color = color;
	for (int i = 0; i < 4; ++i)
	{
		//float value = input.NonLinears[i].x / input.NonLinears[i].y;
		float value = (-input.Depth.z - linearMinsPlusRanges[i].x) / linearMinsPlusRanges[i].y;
		//if (value < -0.1f || value > 1.1f) //cutoff to clarify zones when debugging, but add leeway for sloppy sampling later
			if (value < 0 || value > 2) //cutoff to clarify zones when debugging, but add leeway for sloppy sampling later
		{
			output.depths[i] = 0;
		}
		else
		{
			//output.depths[i] = input.NonLinears[i].x / input.NonLinears[i].y;
			output.depths[i] = value;
		}
	}
	//if (i != 3)
	//	output.depths[3] = 0.01f;//avoid alpha clip may be dumb

	return output;
}

//~~~~~~~~~~~~~SHADOW~~~~~~~~~~~~~~~~
void VertexShaderFunctionSHADOW(inout float4 position : POSITION0, out float2 depth : TEXCOORD0)
{
	float4 posW = mul(position, sw);
	float4 posV = mul(posW, sv);
	position = mul(posV, sp);
	depth = posV.z;
	//depth.xy = position.zw;
}
void InstanceVertexShaderFunctionSHADOW(inout float4 position : POSITION0, out float2 depth:TEXCOORD0,
	in float4 world : POSITION1, in float4 scale : TEXCOORD1)
{
	float4 posW = mul(position * scale + world, sw);
	float4 posV = mul(posW, sv);
	position = mul(posV, sp);
	depth = posV.z;
	//depth.xy = position.zw;
}
float4 PixelShaderFunctionSHADOW(in float4 position: POSITION0, in float2 depth : TEXCOORD0) : COLOR0
{
	float linearDepth = -depth.x / shadowFar;
return float4(linearDepth, 0, 0, 1);
	//float nonLinearDepth = depth.x / depth.y;
	//return float4(nonLinearDepth, 0, 0, 1);
}

technique Default
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
	pass Pass2
	{
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunctionSHADOW();
		PixelShader = compile ps_4_0 PixelShaderFunctionSHADOW();
#else
		VertexShader = compile vs_2_0 VertexShaderFunctionSHADOW();
		PixelShader = compile ps_2_0 PixelShaderFunctionSHADOW();
#endif
	}
}

technique Instanced
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0 InstanceVertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
		VertexShader = compile vs_2_0 InstanceVertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
#endif
	}
	pass Pass2
	{
#if SM4
		VertexShader = compile vs_4_0 InstanceVertexShaderFunctionSHADOW();
		PixelShader = compile ps_4_0 PixelShaderFunctionSHADOW();
#else
		VertexShader = compile vs_2_0 InstanceVertexShaderFunctionSHADOW();
		PixelShader = compile ps_2_0 PixelShaderFunctionSHADOW();
#endif
	}
};