//https://digitalerr0r.wordpress.com/2009/05/16/xna-shader-programming-tutorial-20-depth-of-field/

Texture2D diffusetx;
sampler diffusesampler = sampler_state
{
	Texture = (diffusetx);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};
Texture2D nonlineartx;
sampler nonLinearSampler= sampler_state
{
	Texture = (nonlineartx);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};
Texture2D albedotx;
sampler albedoSampler = sampler_state
{
	Texture = (albedotx);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};
#define SMCOUNT 4
Texture2D shadowtx[SMCOUNT] : register(t0);
sampler shadowsampler = sampler_state
{
	//Texture = (diffusetx);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	//MAGFILTER = LINEAR;
	//MINFILTER = LINEAR;
	//MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

float3 tempNegLightDir = float3(0, 1, 0);
float tempAmbient = 0.3f;
float4x4 inverseProjection;
float4x4 inverseView;
float4x4 shadowInverseProjection;
float4x4 shadowInverseView;
float4x4 svp[SMCOUNT];
float4x4 sv[SMCOUNT];
float4x4 sp[SMCOUNT];
float sfar[SMCOUNT];
float4x4 inverseViewRotation;
float3 farFrustumCorners[4];
float3 farFrustumCornersPlus0[4];
float3 farFrustumCornersPlus1[4];
float3 farFrustumCornersPlus2[4];
float3 farFrustumCornersPlus3[4];
float3 camPos;
int drawmode = 0;
float diffuseWidth;
float diffuseHeight;
float shadowDepthBias[SMCOUNT];
float nonLinearCutoff = 1; //this should be able to go higher as your near and far planes come together
float nonLinearCutoffs[4];
float2 nonLinearMinsPlusRanges[4];
float4x4 nonLinearMatrices[4];
float4x4 inverseNonLinearMatrices[4];
float2 linearMinsPlusRanges[4];
float softSampleDists[4];
float far = 300;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV:TEXCOORD0;
	float3 ray:TEXCOORD1;
	float3 rays[4]:TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position.x = input.Position.x - (1.0f / diffuseWidth / 2.0f);
	output.Position.y = input.Position.y + (1.0f / diffuseHeight / 2.0f);
	output.Position.z = input.Position.z;
	output.Position.w = 1.0f;
	output.UV = input.TexCoord;
	int rayI = input.TexCoord.y *2 + input.TexCoord.x;
	if (input.TexCoord.y == 1)
		rayI = 3 - input.TexCoord.x;
	output.ray = farFrustumCorners[rayI];
	output.rays[0] = farFrustumCornersPlus0[rayI];
	output.rays[1] = farFrustumCornersPlus1[rayI];
	output.rays[2] = farFrustumCornersPlus2[rayI];
	output.rays[3] = farFrustumCornersPlus3[rayI];

	return output;
}
float3 decode(float2 enc)
{
	float scale = 1.7777;
	float3 nn =
		float3(enc.xy,0) * float3(2 * scale, 2 * scale, 0) +
		float3(-scale, -scale, 1);
	float  g = 2.0 / dot(nn.xyz, nn.xyz);
	float3 n;
	n.xy = g*nn.xy;
	n.z = g - 1;
	return n;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 tex = tex2D(diffusesampler, input.UV);

	//linear when far or non-linear when close
	float4 stex;
	float4 quickViewSpace = float4(tex.w * input.ray, 0);
	float3 wposB;
	float3 wposA;

	//if (quickViewSpace.z > nonLinearCutoff)//linear
	{
		float4 worldSpace;
		if (tex.w == 1) {
			discard;
		}
		worldSpace = mul(quickViewSpace, inverseView);
		//worldSpace /= worldSpace.w;
		wposA = worldSpace.xyz + camPos;
	}
	//float3 nonLinearOptions[4];
	float4 nonLinearTex = tex2D(nonLinearSampler, input.UV);
	//return nonLinearTex;
	//return nonLinearTex;
	int count = 4;
	float3 wpos = wposA;
	float4 projPosition;
	float4 viewPosition;
	float depth = -1;
	float4 depths2 = float4(0,0,0,1);
	float dp;
	//depths2.rgba = nonLinearTex * 100;
	for (int i = 0; i < count; ++i) //should only execute once
	{
		if (-quickViewSpace.z > nonLinearCutoffs[i])
		{
			continue;
		}
		int j = i;
		if (nonLinearTex[i] == 1) //imprecision means we should assume cascadei actually ended already
		{
			if (i == count - 1)
				continue; //avoid out of bounds
			j = i + 1;
			//depths2.g = 1;
			//depths2.r = nonLinearTex[j] * 100;
		}
		if (nonLinearTex[i] == 0) //imprecision means we should assume cascadei actually ended already
		{
			if (i == 0)
				continue; //avoid out of bounds
			j = i - 1;
			//depths2.b = 1;
			//depths2.r = nonLinearTex[j] * 100;
		}
		//if (nonLinearTex[j] == 1 || nonLinearTex[j] == 0) //jump backwards since there is nothing here, woops
		//	j = i;
		/*if (nonLinearTex[j] == 0)
		{
			j = i - 1;
			if (i == 0)
			{
				continue;
			}
		}*/
		//for (int k = 0; k < passes; ++k)
		//{
			depth = nonLinearTex[j];
			//if (depth == 0)
			//	depths2.r = 1;
			//if (depth == 1)
			//	depths2.g = 1;
			//float z = depth;// -(depth + nonLinearMinsPlusRanges[i].x) * nonLinearMinsPlusRanges[i].y; //+ min * range
			//float x = input.UV.x * 2 - 1;
			//float y = (1 - input.UV.y) * 2 - 1;
			//projPosition = float4(x, y, z, 1.0f);
			//return projPosition;
			//viewPosition = mul(projPosition, inverseNonLinearMatrices[j]);
			//viewPosition /= viewPosition.w;
			float z = (depth* linearMinsPlusRanges[j].y) +linearMinsPlusRanges[j].x;
			float fulldepth = z / far;
			//return newdepth;
			viewPosition = float4(fulldepth * input.rays[j], 1);
			//depths2.rgb = viewPosition;
			float4 worldPosition = mul(viewPosition, inverseView);
			worldPosition /= worldPosition.w;
			//wposB = worldPosition.xyz;
			//nonLinearOptions[i] = wposB;
		//}
			//wpos = worldPosition.xyz;
			wpos = worldPosition.xyz;// +camPos;
		break;
	}
	//return float4(wpos, 1);
	//return depths2;
		//float chunk = nonLinearCutoff - (-quickViewSpace.z);
		//float index = round(clamp(chunk / nonLinearCutoff, 0, 1));
		//float3 options[2];
		//options[0] = wposA;
		//options[1] = wpos;
		//wpos = options[index];
		//return index;// float4(wpos, 1);
		//return float4(wpos, 1);
		//	wpos = lerp(wposA, wposB, saturate(nonLinearCutoff - (-quickViewSpace.z)));
		//	float3 wpos = wposA;

	//select cascade
	float4 sglob[SMCOUNT];
	float4 sloc[SMCOUNT];
	bool badz[SMCOUNT][4];
	bool baduv[SMCOUNT][4];
	float3 worldNormal = normalize(decode(tex.xy));// *2 - 1;
	float3 up = float3(0, 1, 0);
	if (worldNormal.y > 0.98f || worldNormal.y < 0.98f)
		up = float3(1, 0, 0);
	float3 tangent = cross(worldNormal, up);
	float3 binormal = cross(worldNormal, tangent);
	float3 softSamples[4] = { tangent, -tangent, binormal, -binormal };
			//return float4(tangent * .5f+.5f, 1);
	int cascade = 4;
	for (int i = 0; i < SMCOUNT; ++i)
	{
		float4 shadowVPos = mul(float4(wpos, 1), sv[i]);
		float4 shadowProjPos = mul(shadowVPos, sp[i]);
		//sloc[i][j] = shadowVPos.z;
		bool buz = (shadowProjPos.z < 0 || shadowProjPos.z > 1); //behind near clip or beyond far clip
		float2 suv = shadowProjPos.xy / shadowProjPos.w;
		suv = suv * .5f + .5f;
		suv.y = 1 - suv.y;
		float slop = 0.01f;
		float edgeMin = 0 + slop;
		float edgeMax = 1 - slop;
		bool buv = (suv.x < edgeMin || suv.x > edgeMax || suv.y < edgeMin || suv.y > edgeMax); //outside of this cascades shadow map
		if(!buv && !buz)
			cascade = min(i, cascade);
	}
	if (cascade == 4) {
		float light = dot(worldNormal, tempNegLightDir);
		//light = min(light, minl);

		float value = lerp(tempAmbient, 1, light * .5f + .5f);

		return float4(value, value, value, 1);// float4(suv, 0, 1);// float4(tex.rgb, 1);// float4(wpos, 1);// float4(wpos, 1);
	}
	for (int i = 0; i < SMCOUNT; ++i)
	{
		for (int j = 0; j < 4; ++j)
		{
			float4 shadowVPos = mul(float4(wpos + softSamples[j] * softSampleDists[cascade], 1), sv[i]);
			float4 shadowProjPos = mul(shadowVPos, sp[i]);
			sloc[i][j] = shadowVPos.z;
			badz[i][j] = (shadowProjPos.z < 0 || shadowProjPos.z > 1); //behind near clip or beyond far clip
			float2 suv = shadowProjPos.xy / shadowProjPos.w;
			suv = suv * .5f + .5f;
			suv.y = 1 - suv.y;
			float slop = 0.01f;
			float edgeMin = 0 + slop;
			float edgeMax = 1 - slop;
			baduv[i][j] = (suv.x < edgeMin || suv.x > edgeMax || suv.y < edgeMin || suv.y > edgeMax); //outside of this cascades shadow map
			//stex = tex3D(sm, float3(suv, i));
			//stex = tex2D(shadowsampler, suv);// float4(suv, 0, 0));
			//shadowtx[i].Sample(shadowsampler, suv);
			float4 stex = shadowtx[i].Sample(shadowsampler, suv);
			sglob[i][j] = -stex.r * sfar[i];
			//break;
		}
	}
	int4 cascades = -1;
	//for (int i = 0; i < SMCOUNT; ++i)
	//{
	//	for (int j = 0; j < 4; ++j)
	//	{
	//		if (!baduv[i][j] && !badz[i][j])
	//		{
	//			//cascade = i;
	//			cascades[j] = min(i, cascades[j]);
	//			break;
	//		}
	//	}
	//}
	float minl = 1;
	//if (cascade < 0)
	//{
	//	minl = 0;
		//float4 debug = 0;
		//if (cascade == 0)debug.r = 1;
		//else if (cascade == 1) debug.g = 1;
		//else if (cascade == 2) debug.b = 1;
		//else if (cascade == 3) debug.rgb = 1;
		//return debug;
	//}
	//else //if(sglob[cascade] > sloc[cascade] + shadowDepthBias[cascade])
	{
		for (int i = 0; i < 4; ++i)
		{
			for (int j = 0; j < 4; ++j)
			{
				if (cascades[j] == -1 && !baduv[i][j] && !badz[i][j] && sglob[i][j] > sloc[i][j] + shadowDepthBias[i])
				{
					minl -= 0.25f;
					cascades[j] = j;
				}
			}
		}
	}
	float light = dot(worldNormal, tempNegLightDir);
	if (light > 0)
	{
		light = lerp(light * 0.25f, light, minl);
	}
	//else {
	//	light = min(light, minl);
	//}
	
	float value = lerp(tempAmbient, 1, light * .5f + .5f);

	return float4(value, value, value,1);// float4(suv, 0, 1);// float4(tex.rgb, 1);// float4(wpos, 1);// float4(wpos, 1);
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