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
Texture2D depthtx;
sampler depthSampler = sampler_state
{
	Texture = (depthtx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};
Texture2D blurtx;
sampler blurSampler = sampler_state
{
	Texture = (blurtx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

float near;
float far;
float range;
float distance;
float screenWidth;
float screenHeight;
int drawmode;


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
	float2 uv = pos;// *0.5f + 0.5f;
    float4 diffuse = tex2D(diffusesampler, uv);
	float depth = tex2D(depthSampler, uv).r;
	float4 blur = tex2D(blurSampler, uv);
	float fDepth = depth;
	float fSceneZ = (-near*far) / (fDepth - far);
	float blurFactor = saturate(abs(fSceneZ - distance) / range);
	switch (drawmode)
	{
	case 0: //DEPTH OF FIELD
		return lerp(diffuse, blur, blurFactor);
		break;
	case 1: //DEPTH
		return float4(depth, depth, depth, 1);
		break;
	case 2: //DIFFUSE
		return diffuse;
		break;
	case 3: //DIFFUSE
		return blur;
		break;
	case 4: //DIFFUSE
		return float4(blurFactor,blurFactor,blurFactor,1);
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