//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

void ApplyTranslucency(inout float3 color, float3 viewDirectionWS, Light light, float amount)
{
	float VdotL = saturate(dot(-viewDirectionWS, light.direction));
	VdotL = pow(VdotL, 4);

	//Translucency masked by shadows and grass mesh bottom
	float tMask = VdotL * light.shadowAttenuation * light.distanceAttenuation;

	//Fade the effect out as the sun approaches the horizon (75 to 90 degrees)
	half sunAngle = dot(float3(0, 1, 0), light.direction);
	half angleMask = saturate(sunAngle * 6.666); /* 1.0/0.15 = 6.666 */

	tMask *= angleMask;

	float3 tColor = color + BlendOverlay((light.color), color);
	color = lerp(color, tColor, tMask * (amount * 4.0));
}

float3 SpecularHighlight(Light light, half smoothness, half3 normalWS, half3 viewDirectionWS) {

	float3 halfVec = SafeNormalize(float3(light.direction) + float3(viewDirectionWS));
	half NdotH = max(0, saturate(dot(normalWS, halfVec)));

#if _ADVANCED_LIGHTING
	half perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
	half roughnesss = max(PerceptualRoughnessToRoughness(perceptualRoughness), HALF_MIN);

	half roughnesss2MinOne = (roughnesss * roughnesss) - 1.0h;
	half normalizationTerm = roughnesss * 4.0h + 2.0h;

	float d = NdotH * NdotH * roughnesss2MinOne + 1.00001f;

	half LoH = saturate(dot(light.direction, halfVec));
	half LoH2 = LoH * LoH;

	half3 specularReflection = (roughnesss * roughnesss) / ((d * d) * max(0.1h, LoH2) * normalizationTerm);

	specularReflection *= light.distanceAttenuation * light.shadowAttenuation * smoothness;
#else
	float nh = max(0, dot(normalWS, halfVec));

	smoothness += 0.0001;
	half3 specularReflection = pow(nh, smoothness * 128)  * (smoothness) * 2;
#endif

	return light.color * specularReflection;

}

//Blinn-phong shading with SH
half3 SimpleLighting(Light light, half3 normalWS, half3 viewDirectionWS, half3 bakedGI, half3 albedo, half amount, half3 emission)
{
	light.color *= (light.distanceAttenuation * light.shadowAttenuation);

	half3 diffuseColor = bakedGI + LightingLambert(light.color, light.direction, normalWS);
	half3 specularColor = SpecularHighlight(light, amount, normalWS, viewDirectionWS);
	//half3 specularColor = LightingSpecular(light.color, light.direction, float3(0,0,1), viewDirectionWS, 1, amount);

	half3 color = (albedo * diffuseColor) + emission;

	color += specularColor;

	//return specularColor;

	return color;
}

// General function to apply lighting based on the configured mode
half3 ApplyLighting(SurfaceData surfaceData, Light mainLight, half3 vertexLight, float2 lightmapUV, half3 normalWS, half3 positionWS, half translucency)
{
#ifdef LIGHTMAP_ON
	// Normal is required in case Directional lightmaps are baked
	half3 bakedGI = SampleLightmap(lightmapUV, normalWS);
#else
	// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
	// are also defined in case you want to sample some terms per-vertex.
	half3 bakedGI = SampleSH(normalWS);
#endif

	half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

#if _ADVANCED_LIGHTING
	// BRDFData holds energy conserving diffuse and specular material reflections and its roughness.
	BRDFData brdfData;
	InitializeBRDFData(surfaceData.albedo, 0.0 /* metallic */, 0, surfaceData.smoothness, surfaceData.alpha, brdfData);

	// Mix diffuse GI with environment reflections.
	half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);

	// LightingPhysicallyBased computes direct light contribution.
	color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

	//No using PBR specular highlight, since it's bounds to reflecitivty. Using a custom function for it.
	color += SpecularHighlight(mainLight, surfaceData.specular.r, normalWS, viewDirectionWS);
#else
	//Simple diffuse and specular shading
	MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI, half4(0, 0, 0, 0));

	half3 color = SimpleLighting(mainLight, normalWS, viewDirectionWS, bakedGI, surfaceData.albedo.rgb, surfaceData.specular.r, surfaceData.emission);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	//Apply light color, previously calculated in vertex shader
	color += vertexLight;
#endif // Vertex lights

	// Additional lights loop per-pixel
#if _ADDITIONAL_LIGHTS

	// Returns the amount of lights affecting the object being renderer.
	// These lights are culled per-object in the forward renderer
	uint additionalLightsCount = GetAdditionalLightsCount();
	for (uint i = 0u; i < additionalLightsCount; ++i)
	{
		// Similar to GetMainLight, but it takes a for-loop index. This figures out the
		// per-object light index and samples the light buffer accordingly to initialized the
		// Light struct. If _ADDITIONAL_LIGHT_SHADOWS is defined it will also compute shadows.
		Light light = GetAdditionalLight(i, positionWS);

#if _ADVANCED_LIGHTING
		// Same functions used to shade the main light.
		color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);

		// Apply translucency for additional lights?
		//ApplyTranslucency(color, viewDirectionWS, light, translucency);
#else
		//Diffuse + specular lighting
		color += SimpleLighting(light, normalWS, viewDirectionWS, bakedGI, surfaceData.albedo.rgb, surfaceData.smoothness, surfaceData.emission);
#endif
	}
#endif //Additional lights

	ApplyTranslucency(color, viewDirectionWS, mainLight, translucency);

	// Emission (wind gust tint)
	color += surfaceData.emission;

	return color;
}

half3 AddBRDF(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS)
{
	half NdotL = saturate(dot(normalWS, light.direction));
	half3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation * NdotL);

	return DirectBDRF(brdfData, normalWS, light.direction, viewDirectionWS) * radiance;
}