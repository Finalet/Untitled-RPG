#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

	struct IndirectShaderData
	{
		float4x4 PositionMatrix;
		float4x4 InversePositionMatrix;
		float4 ControlData;
	};

	#if defined(SHADER_API_GLCORE) || defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PS4) || defined(SHADER_API_XBOXONE)
		StructuredBuffer<IndirectShaderData> IndirectShaderDataBuffer;
		StructuredBuffer<IndirectShaderData> VisibleShaderDataBuffer;
	#endif	
#endif

void setup()
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	#ifdef GPU_FRUSTUM_ON
		unity_ObjectToWorld = VisibleShaderDataBuffer[unity_InstanceID].PositionMatrix;
		unity_WorldToObject = VisibleShaderDataBuffer[unity_InstanceID].InversePositionMatrix;
	#else
		unity_ObjectToWorld = IndirectShaderDataBuffer[unity_InstanceID].PositionMatrix;
		unity_WorldToObject = IndirectShaderDataBuffer[unity_InstanceID].InversePositionMatrix;
	#endif
#endif
}

