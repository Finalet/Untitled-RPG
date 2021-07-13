using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DoNotModifyShaderEditor : ShaderGUI
{
  public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
  {
    EditorGUILayout.HelpBox("Do not edit/modify skybox properties from here. You should change all " +
      "sky properties using the 'Sky Profile' linked from your SkySystemController in the scene.", MessageType.Error);

    base.OnGUI(materialEditor, properties);
  }
}
