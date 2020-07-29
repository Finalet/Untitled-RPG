#if AMPLIFY_SHADER_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using AmplifyShaderEditor;

namespace AmplifyShaderEditor
{
    [Serializable]
    [NodeAttributes("Vegetation Studio - Touch React", "Vegetation Studio", "Add touch react to your vegetation shader")]
    public class TouchReactNode : ParentNode
    {
        public static readonly string TouchReactAdjustVertex = "TouchReactAdjustVertex( {0}3({1},{2},{3}) )";
        protected override void CommonInit(int uniqueId)
        {
            base.CommonInit(uniqueId);
            AddInputPort(WirePortDataType.FLOAT3, false, "Vertex Position");
            AddOutputPort(WirePortDataType.FLOAT3, "Local Vertex Offset");
        }

        public override void OnInputPortConnected(int inputPortId, int otherNodeId, int otherPortId, bool activateNode = true)
        {
            base.OnInputPortConnected(inputPortId, otherNodeId, otherPortId, activateNode);
            UpdateConnection(inputPortId);
        }

        void UpdateConnection(int portId)
        {
            m_inputPorts[portId].MatchPortToConnection();
            //WirePortDataType outputType = (UIUtils.GetPriority(m_inputPorts[0].DataType) > UIUtils.GetPriority(m_inputPorts[1].DataType)) ? m_inputPorts[0].DataType : m_inputPorts[1].DataType;
            WirePortDataType outputType = m_inputPorts[0].DataType;        
            m_outputPorts[0].ChangeType(outputType, false);
        }

        public override string GenerateShaderForOutput(int outputId, ref MasterNodeDataCollector dataCollector,
            bool ignoreLocalvar)
        {
            string valueInput1 = m_inputPorts[0].GenerateShaderForOutput(ref dataCollector, WirePortDataType.FLOAT4, ignoreLocalvar, true);
            //dataCollector.AddToPragmas(UniqueId, "multi_compile TOUCH_BEND_ON __");

            List<string> touchReactAdjustVertexFunctionStringList = new List<string>();
            //touchReactAdjustVertexFunctionStringList.Add("#ifdef TOUCH_BEND_ON");
            touchReactAdjustVertexFunctionStringList.Add("sampler2D	_TouchReact_Buffer;");
            touchReactAdjustVertexFunctionStringList.Add("float4 _TouchReact_Pos;");
            //touchReactAdjustVertexFunctionStringList.Add("#endif");
            touchReactAdjustVertexFunctionStringList.Add(" ");
            touchReactAdjustVertexFunctionStringList.Add("float3 TouchReactAdjustVertex(float3 pos)");
            touchReactAdjustVertexFunctionStringList.Add("{");
            //touchReactAdjustVertexFunctionStringList.Add("#ifdef TOUCH_BEND_ON");

            touchReactAdjustVertexFunctionStringList.Add("   float3 worldPos = mul(unity_ObjectToWorld, float4(pos,1));");
            touchReactAdjustVertexFunctionStringList.Add("   float2 tbPos = saturate((float2(worldPos.x,-worldPos.z) - _TouchReact_Pos.xz)/_TouchReact_Pos.w);");
            touchReactAdjustVertexFunctionStringList.Add("   float2 touchBend  = tex2Dlod(_TouchReact_Buffer, float4(tbPos,0,0));");
            touchReactAdjustVertexFunctionStringList.Add("   touchBend.y *= 1.0 - length(tbPos - 0.5) * 2;");
            touchReactAdjustVertexFunctionStringList.Add("   if(touchBend.y > 0.01)");
            touchReactAdjustVertexFunctionStringList.Add("   {");
            touchReactAdjustVertexFunctionStringList.Add("      worldPos.y = min(worldPos.y, touchBend.x * 10000);");
            touchReactAdjustVertexFunctionStringList.Add("   }");
            touchReactAdjustVertexFunctionStringList.Add("");
            touchReactAdjustVertexFunctionStringList.Add("   float3 changedLocalPos = mul(unity_WorldToObject, float4(worldPos,1)).xyz;");

            touchReactAdjustVertexFunctionStringList.Add("   return changedLocalPos - pos;");
            //touchReactAdjustVertexFunctionStringList.Add("#else");
            //touchReactAdjustVertexFunctionStringList.Add("   return float3(0,0,0);");
            //touchReactAdjustVertexFunctionStringList.Add("#endif");
            touchReactAdjustVertexFunctionStringList.Add("}");

            dataCollector.AddFunction(TouchReactAdjustVertex,touchReactAdjustVertexFunctionStringList.ToArray(),true);

            return "TouchReactAdjustVertex(" + valueInput1 + ".xyz)";
        }
    }
}

#endif