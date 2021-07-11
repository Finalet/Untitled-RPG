#ifndef VOLUMETRICCLOUDS_INCLUDE
#define VOLUMETRICCLOUDS_INCLUDE

float random(float2 Seed, float Min, float Max) {
	float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
	return lerp(Min, Max, randomno);
}


void screenToViewVector_float(float2 UV, out float3 viewVector) {
	
	float3 viewDirectionTemp = mul(unity_CameraInvProjection, float4(UV * 2 - 1, 0.0, -1));
	viewVector = mul(unity_CameraToWorld, viewDirectionTemp);
}

void TAA_float(Texture2D AveragedPastFrames, Texture2D CurrentFrame, float2 inputUV, SamplerState samplerState, float blend, out float4 ColorOut)
{
#if defined(SHADERGRAPH_PREVIEW)
ColorOut = float4(0,0,0,0);
#else
ColorOut = blend * CurrentFrame.SampleLevel(samplerState, inputUV, 0) + (1.0 - blend) * AveragedPastFrames.SampleLevel(samplerState, inputUV, 0);
#endif	
}



float Lerp(float a, float b, float t)
{
	return (1.0 - t) * a + b * t;
}

float InverseLerp(float a, float b, float v)
{
	return (v - a) / (b - a);
}

float Remap(float iMin, float iMax, float oMin, float oMax, float v)
{
	float t = InverseLerp(iMin, iMax, v);
	return Lerp(oMin, oMax, t);
}

struct AtmosphereData
{
	// Pre-Defined
	float atmosThickness;
	float atmosHeight;
	
	// Calculated
	float atmosStartHeight;
	float atmosEndHeight;
	float atmosStartDist;
	float atmosEndDist;
	float atmosThicknessInDir;
};


struct StaticMaterialData {
	SamplerState fogSampler;
	
	float3 rayOrigin;
	float3 sunPos;
	
	float cloudiness;
	float cloudDensity;
	float alphaAccumulation;
	float coneMagnitude;
	
	float3 shapeScale;
	float detailScale;
	
	float detailStrength1;
	float detailStrength2;
	
	float3 baseTimescale;
	float3 detail1Timescale;
	float3 detail2Timescale;
	
	Texture3D noise3D;
	Texture3D detailTexture;
	
};

struct RayData
{
	float3 rayPosition;
	float3 rayDirection;
	float relativeDepth;
};

float sampleCloudShape(StaticMaterialData materialData, RayData rayData, AtmosphereData atmosData)
{
	float threshold = 1.0 - materialData.cloudiness;
		
	float3 offset = _Time.y * 0.0005 * materialData.baseTimescale;
	float3 uvw = rayData.rayPosition * 0.000005 + offset;
	float3 baseUVW = uvw * materialData.shapeScale;
	uint mip = 0;
	float3 noiseSample = materialData.noise3D.SampleLevel(materialData.fogSampler, baseUVW, mip).rgb;
	
	float value = noiseSample.r * 0.6 + noiseSample.g * 0.3 + noiseSample.b * 0.1;
	
	
	float heightPercent = saturate(Remap(atmosData.atmosStartHeight, atmosData.atmosEndHeight, 0.0, 1.0, rayData.rayPosition.y));
	
	
	if (value > threshold)
	{
		uvw *= materialData.detailScale;
		uvw *= 24.0;
		float3 detailOffset = _Time.y * 0.001 * materialData.detail1Timescale;
		uvw += detailOffset;
		float3 detailSample = materialData.detailTexture.SampleLevel(materialData.fogSampler, uvw, mip).rgb;
		float detailValue1 = detailSample.r * 0.6 + detailSample.g * 0.3 + detailSample.b * 0.1;
		value -= materialData.detailStrength1 * detailValue1;
		
		if (value > threshold)
		{
			uvw *= 7.0;
			float3 detailOffset = _Time.y * 0.001 * materialData.detail2Timescale;
			uvw += detailOffset;
			float3 detailSample2 = materialData.detailTexture.SampleLevel(materialData.fogSampler, uvw, mip).rgb;
			float detailValue2 = detailSample2.r * 0.6 + detailSample2.g * 0.3 + detailSample2.b * 0.1;
			value -= materialData.detailStrength2 * detailValue2;
		}
	}
	
	// Round at Bottom and Top
	float roundingAtBottom = saturate(Remap(0.0, 0.07, 0.0, 1.0, heightPercent));
	float roundingAtTop = saturate(Remap(0.10, 1.0, 1.0, 0.8, heightPercent));
	
	value *= (roundingAtBottom * roundingAtTop);
	
	// Soften Density at Bottom and Top and Return.
	if (value > threshold)
	{
		float softenDensityAtBottom = saturate(Remap(0.0, 0.3, 0.0, 1.0, heightPercent));
		float softenDensityAtTop = saturate(Remap(0.9, 1.0, 1.0, 0.0, heightPercent));
		value *= (softenDensityAtBottom * softenDensityAtTop);
		return value * materialData.alphaAccumulation;
	}
	
	return 0;
}


float lightmarchCheap(StaticMaterialData materialData, RayData rayData, AtmosphereData atmosData)
{
	// Setup density, origin, cone sample distances, and travel distances.
	float density = 0;
	float3 cachedRayOrigin = rayData.rayPosition;
	
	float coneDist[5];
	coneDist[0] = 0.02;
	coneDist[1] = 0.08;
	coneDist[2] = 0.2;
	coneDist[3] = 0.5;
	coneDist[4] = 1.0;
	
	float totalDist = (atmosData.atmosThickness * 0.8);
	float meanStepSize = totalDist * 0.2;
	
	
	for (int i = 0; i < 5; i++)
	{
		// Setup randomized lighting cone samples. Performance cost worsens as magnitude increases.
		float3 coneDir = 0;
		if (materialData.coneMagnitude > 0.0)
		{
			float2 xy = float2(rayData.rayPosition.x, rayData.rayPosition.y);
			float2 yz = float2(rayData.rayPosition.y, rayData.rayPosition.z);
			float2 zx = float2(rayData.rayPosition.z, rayData.rayPosition.x);
			coneDir = float3(random(xy, -materialData.coneMagnitude, materialData.coneMagnitude), random(yz, -materialData.coneMagnitude, materialData.coneMagnitude), random(zx, -materialData.coneMagnitude, materialData.coneMagnitude));
		}
		
		// Step the ray forward
		float stepLength = coneDist[i] * totalDist;
		rayData.rayPosition = cachedRayOrigin + ((materialData.sunPos + coneDir) * stepLength);
		
		// Exit early if the ray exits the atmosphere
		if (rayData.rayPosition.y > atmosData.atmosEndHeight || rayData.rayPosition.y < atmosData.atmosStartHeight)
		{
			break;
		}
		
		// Sample the cloud density to determine the lighting influence on this point.
		float valueAtPoint = sampleCloudShape(materialData, rayData, atmosData);
		density += valueAtPoint;
	}
	
	return density * materialData.cloudDensity * meanStepSize;
}



void SampleClouds_float(SamplerState CloudSampler, float3 RayOrigin, float3 RayDir, float3 SunPos, Texture3D BaseTexture3D, Texture3D DetailTexture3D, float AlphaAccumulation, float Cloudiness, float Density, float ConeMagnitude, float3 SunColor, float3 AmbientColor, float BlueNoise, float NumSteps, float CloudLayerHeight, float CloudLayerThickness, float CloudFadeDistance, float3 BaseLayerScale, float BlueNoiseStrength, float LightAlignmentStrength, float detailStrength1, float detailStrength2, float3 baseTimescale, float3 detail1Timescale, float3 detail2Timescale, float LightingIntensity, float DetailScale, out float3 cloudColor, out float alpha)
{
#if defined(SHADERGRAPH_PREVIEW)
alpha = 1;
cloudColor = float3(0, 0, 0);
#else
	// Material Data Setup
	StaticMaterialData materialData;
	
	materialData.fogSampler = CloudSampler;
	
	materialData.rayOrigin = RayOrigin;
	materialData.sunPos = SunPos;
	
	materialData.cloudiness = Cloudiness;
	materialData.cloudDensity = Density * Density;
	materialData.alphaAccumulation = AlphaAccumulation * AlphaAccumulation * 0.1;
	materialData.coneMagnitude = ConeMagnitude;
	
	materialData.noise3D = BaseTexture3D;
	materialData.detailTexture = DetailTexture3D;
	
	materialData.detailScale = DetailScale;
	materialData.shapeScale = BaseLayerScale;
	
	materialData.detailStrength1 = detailStrength1;
	materialData.detailStrength2 = detailStrength2;
	
	materialData.detail1Timescale = detail1Timescale;
	materialData.detail2Timescale = detail2Timescale;
	materialData.baseTimescale = baseTimescale;
	
	
	// Cloud Parameter Setup
	CloudFadeDistance *= 1000.0;
	CloudLayerThickness *= 1000.0;
	CloudLayerHeight *= 1000.0;
	float invCloudFadeDist = 1.0 / CloudFadeDistance;
	
	AtmosphereData atmosData;
	atmosData.atmosThickness = CloudLayerThickness;
	atmosData.atmosHeight = CloudLayerHeight;
	
	// Lighting Paramter Setup
	alpha = 1.0;
	cloudColor = AmbientColor;
	
	
	if (RayDir.y > 0)
	{
		// INITIAL SETUP
		float invRayDir = 1.0 / RayDir.y;
		float depth = atmosData.atmosHeight * invRayDir;
		float maxDepth = (atmosData.atmosHeight + atmosData.atmosThickness) * invRayDir;
		
		atmosData.atmosStartHeight = atmosData.atmosHeight + RayOrigin.y;
		atmosData.atmosEndHeight = atmosData.atmosStartHeight + atmosData.atmosThickness;
		
		atmosData.atmosThicknessInDir = maxDepth - depth;
		
		
		// SAMPLING
		float valueAtPoint = 0;
		float density = 0;
		
		float invNumSteps = 1.0 / NumSteps;
		float meanStepSize = atmosData.atmosThicknessInDir * invNumSteps;
		float fixedStepSize = atmosData.atmosThickness * invNumSteps;
		depth += Remap(0.0, 1.0, -meanStepSize, meanStepSize, BlueNoise) * BlueNoiseStrength;
		
		// Dot to SunPos for Scattering
		float theta = dot(RayDir, materialData.sunPos);
		
		// Initialize rayData
		RayData rayData;
		
		for (int i = 0; i < NumSteps; i++)
		{
			// Sample Cloud Shape
			rayData.rayPosition = RayOrigin + (RayDir * depth);
			rayData.relativeDepth = depth * invCloudFadeDist;
			valueAtPoint = sampleCloudShape(materialData, rayData, atmosData) * fixedStepSize;
			
			// If the cloud exists at this point, sample the lighting
			if (valueAtPoint > 0.00)
			{
				// Lighting and Density Setup
				alpha = max(0, alpha - valueAtPoint);
				density = lightmarchCheap(materialData, rayData, atmosData);
				float lightingEnergy = alpha * fixedStepSize * 0.02;
				
				// Main Lighting
				float beers = exp(-density);
				float powder = 1.0 - exp(-density * 2.0);
				float beerspowder = beers * powder;
				cloudColor += SunColor * beerspowder * lightingEnergy * LightingIntensity;
				
				
				// Scattering
				float remapTheta = Remap(-1.0, 0.0, 0.0, 1.0, theta);
				remapTheta = pow(remapTheta, 6.0);
				cloudColor += SunColor * remapTheta * lightingEnergy * (LightAlignmentStrength * LightAlignmentStrength);
				
				if (alpha <= 0.01)
				{
					break;
				}
			}
			else
			{
				density = 0;
			}
			
			depth += meanStepSize;
		}
		
		// Subtractive Density Lighting
		cloudColor *= exp(-(1.0 - alpha));
		
		// Depth Fog, fixed 15km start distance
		alpha = saturate(Remap(15000.0, CloudFadeDistance, alpha, 1.0, ((depth + maxDepth) * 0.5)));
	}
		
		
#endif
}

#endif