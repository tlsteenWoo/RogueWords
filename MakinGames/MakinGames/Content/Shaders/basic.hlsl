//http://rbwhitaker.wikidot.com/first-shader

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 eyePos;

float4 DiffuseColor = float4(1, 1, 1, 1);
float3 AmbientColor = float3(.1f, .1f, .1f);
float3 EmissiveColor = float3(0, 0, 0);
float depthfactor = 0.1;
float2 uvscale = float2(1,1);
float2 walluv = float2(1, 1);
float3 l = normalize(float3(1,-1,0));
float3 l2 = normalize(float3(-1, -1, -1));
float3 l3 = normalize(float3(0, 1, 0));
//float intensityA = 0.9f;
//float intensityB = 0.4f;
//float intensityC = 0.1f;
float3 colorA = float3(1, 1, 1);
float3 colorB = float3(1, 1, 1);
float3 colorC = float3(1, 1, 1);
bool enableLighting = true;
bool enableA = true;
bool enableB = true;
bool enableC = true;
float specPow = 1;
bool flipSpec = false;
int drawmode = 0;

Texture2D diffusetx;
sampler diffusesampler = sampler_state
{
	Texture = (diffusetx);
MAGFILTER = LINEAR;
MINFILTER = LINEAR;
MIPFILTER = LINEAR;
AddressU = Wrap;
AddressV = Wrap;
};
Texture2D walltx;
sampler wallsampler = sampler_state
{
	Texture = (walltx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};
//Texture2D ctx;
//sampler diffusesampler = sampler_state
//{
//	Texture = (diffusetx);
//	MAGFILTER = LINEAR;
//	MINFILTER = LINEAR;
//	MIPFILTER = LINEAR;
//	AddressU = Wrap;
//	AddressV = Wrap;
//};
//sampler diffusesampler = sampler_state
//{
//	Texture = (diffusetx);
//	MAGFILTER = POINT;
//	MINFILTER = POINT;
//	MIPFILTER = POINT;
//	AddressU = Wrap;
//	AddressV = Wrap;
//};
Texture2D normaltx;
sampler normalsampler = sampler_state
{
	Texture = (normaltx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal: NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float2 Depth : TEXCOORD2;
	float3 wpos:TEXCOORD3;
	float3 vpos:TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.Normal = mul(input.Normal, World);
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.wpos = worldPosition.xyz;
	output.vpos = viewPosition;

	return output;
}

struct PixelShaderOutput
{
	float4 color:COLOR0;
	float4 depth : COLOR1;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	//float3 rel= input.Normal * dot(input.rawpos.xyz, input.Normal);
	float3 worldnorm = normalize(input.Normal);
	float3 vec = float3(0,1,0);
	if (abs(dot(vec, worldnorm)) > 0.95f)
		vec = float3(1, 0, 0);
	float3 tang = cross(vec, worldnorm);
	float3 binorm = cross(tang, worldnorm);
	float2 uv = float2(dot(input.wpos, tang), dot(input.wpos, binorm));
	float4 tex = float4(1,1,1,1);
	if (abs(worldnorm.y) > 0.98f)
		tex = tex2D(diffusesampler, uv * uvscale);
	else
		tex = tex2D(wallsampler, uv * walluv);
	//float4 normtex = tex2D(normalsampler, uv * uvscale);
	//float3 normsample = 2 * normtex.rgb - 1.0f;
	//float3x3 tbn = float3x3(tang, binorm, worldnorm);
	//float3 norm = mul(normsample, tbn);
	float depth = input.Depth.x / input.Depth.y;
	float camdepth = saturate(-input.vpos.z*depthfactor);
	//float ndl = saturate(-dot(norm, l));
	//float ndl2 = saturate(-dot(norm, l2));
	//float3 v = eyePos - input.wpos;
	//if (flipSpec)
	//	v = cross(cross(v, norm), v);
	//float3 h = normalize(-l + v);
	//float spec1 = saturate(dot(norm, h));
	////spec1 = pow(spec1, specPow);
	//return float4(spec1, spec1, spec1, 1);
	//return float4(norm * .5f + .5f, 1);
	//return float4(depth, depth, depth, 1);
	//return float4(tang * .5f + .5f, 1);
	//float4 color = lerp(AmbientColor, tex * AmbientColor, 1 - camdepth);
	//float spec1f = pow(spec1, specPow);
	float3 lighting = AmbientColor;
	if (enableA) {
		float ndl = saturate(dot(worldnorm, -l));
		lighting += ndl * colorA;
	}
	if (enableB) {
		float ndl2 = saturate(dot(worldnorm, -l2));
		lighting += ndl2 * colorB;
	}
	if (enableC) {
		float ndl3 = saturate(dot(worldnorm, -l3));
		lighting += ndl3 * colorC;
	}
	float3 color = tex.rgb * DiffuseColor.rgb * lighting + EmissiveColor;
	//color += float3(1,1,1) * spec1f;
	switch (drawmode)
	{
	case 0: //DIFFUSE
		output.color = float4(color, DiffuseColor.a * tex.a);// tex.w * AmbientColor.w);
		break;
	case 1: //NORMAL
		output.color = float4(worldnorm * .5f + .5f, 1);
		break;
	case 2: //DEPTH
		output.color = float4(depth, depth, depth, 1);
		break;
	case 3: //LIGHTING
		output.color = float4(lighting, 1);
		break;
	case 4: //ALBEDO
		output.color = tex;
		break;
	case 5: //ALBEDO Tinted
		output.color = tex * DiffuseColor;
		break;
	default:
		output.color = float4(1, 0, 1, 1);
		break;
	}
	output.depth = float4(depth,0,0,1);// depth;

	return output;
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