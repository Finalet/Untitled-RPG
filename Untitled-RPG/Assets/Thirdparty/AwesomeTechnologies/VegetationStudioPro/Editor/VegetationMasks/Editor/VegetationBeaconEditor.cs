using AwesomeTechnologies.External.CurveEditor;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [CustomEditor(typeof(VegetationBeacon))]
    public class VegetationBeaconEditor : VegetationStudioProBaseEditor
    {
        public VegetationBeacon VegetationBeacon;
        private int _vegetationTypeIndex;
        private InspectorCurveEditor _distanceCurveEditor;


        public void OnEnable()
        {
            var settings = InspectorCurveEditor.Settings.DefaultSettings;
            _distanceCurveEditor = new InspectorCurveEditor(settings) { CurveType = InspectorCurveEditor.InspectorCurveType.Distance };
        }

        public void OnDisable()
        {
            _distanceCurveEditor.RemoveAll();
        }

        public override void OnInspectorGUI()
        {
            HelpTopic = "vegetation-beacon";
            base.OnInspectorGUI();

            VegetationBeacon = (target as VegetationBeacon);
            if (VegetationBeacon)
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Settings", LabelStyle);
                EditorGUI.BeginChangeCheck();
                if (VegetationBeacon != null)
                {
                    VegetationBeacon.Radius = EditorGUILayout.Slider("Radius", VegetationBeacon.Radius, 0.1f, 150f);
                    EditorGUILayout.LabelField("Falloff curve", LabelStyle);
                    if (_distanceCurveEditor.EditCurve(VegetationBeacon.FalloffCurve, this))
                    {
                        SetMaskDirty();
                    }

                    EditorGUILayout.HelpBox("This curve sets the spawn chance of the localized vegetation. left center, right edges of area. Top is high spawn chance.", MessageType.Info);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetMaskDirty();
                    }

                    GUILayout.EndVertical();


                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Localized vegetation placement", LabelStyle);

                    if (GUILayout.Button("Add vegetation type"))
                    {
                        VegetationTypeSettings newVegetationTypeSettings = new VegetationTypeSettings();
                        VegetationBeacon.VegetationTypeList.Add(newVegetationTypeSettings);
                        _vegetationTypeIndex = VegetationBeacon.VegetationTypeList.Count - 1;
                        SetMaskDirty();
                    }

                    string[] packageNameList = new string[VegetationBeacon.VegetationTypeList.Count];
                    for (int i = 0; i <= VegetationBeacon.VegetationTypeList.Count - 1; i++)
                    {
                        packageNameList[i] = (i + 1).ToString() + ". Item";
                    }


                    if (VegetationBeacon.VegetationTypeList.Count > 0)
                    {
                        if (_vegetationTypeIndex > VegetationBeacon.VegetationTypeList.Count - 1) _vegetationTypeIndex = VegetationBeacon.VegetationTypeList.Count - 1;
                        _vegetationTypeIndex = EditorGUILayout.Popup("Selected item", _vegetationTypeIndex, packageNameList);

                        //Settings for selected VegetationType


                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginVertical("box");

                        VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Index = (VegetationTypeIndex)EditorGUILayout.EnumPopup("Vegetation type", VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Index);
                        VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Density = EditorGUILayout.Slider("Density", VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Density, 0f, 1f);
                        VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Size = EditorGUILayout.Slider("Size", VegetationBeacon.VegetationTypeList[_vegetationTypeIndex].Size, 0f, 2f);

                        if (GUILayout.Button("Delete selected item"))
                        {
                            VegetationBeacon.VegetationTypeList.RemoveAt(_vegetationTypeIndex);
                        }

                        GUILayout.EndVertical();

                        if (EditorGUI.EndChangeCheck())
                        {
                            SetMaskDirty();
                        }
                    }
                }
            }

            GUILayout.EndVertical();
        }

        public Vector3 GetTerrainPosition(Vector3 position)
        {
                Ray ray = new Ray(position + new Vector3(0, 2000f, 0), Vector3.down);

                var hits = Physics.RaycastAll(ray);
                for (int j = 0; j <= hits.Length - 1; j++)
                {
                    if (hits[j].collider is TerrainCollider)
                    {
                        return hits[j].point;                       
                    }
                }
            return position;
        }

        void SetMaskDirty()
        {
            VegetationBeacon.UpdateVegetationMask();
            EditorUtility.SetDirty(VegetationBeacon);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(VegetationBeacon.gameObject.scene);
        }

        public void OnSceneGUI()
        {
            VegetationBeacon vegetationBeacon = (target as VegetationBeacon);

            if (vegetationBeacon)
            {
                Vector3 terrainPosition = GetTerrainPosition(vegetationBeacon.transform.position);
                EditorGUI.BeginChangeCheck();
                vegetationBeacon.Radius = Handles.RadiusHandle(Quaternion.identity, terrainPosition, vegetationBeacon.Radius, true);

                Handles.color = new Color(1, 0, 0, 0.2f);
                Handles.DrawSolidDisc(terrainPosition, Vector3.up, vegetationBeacon.Radius);
                Handles.color = Color.red;

                if (EditorGUI.EndChangeCheck())
                {
                    SetMaskDirty();
                }
            }
        }
    }
}
