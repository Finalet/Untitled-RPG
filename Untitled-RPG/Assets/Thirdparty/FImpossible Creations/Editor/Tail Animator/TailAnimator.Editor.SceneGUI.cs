using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        protected virtual void OnSceneGUI()
        {
            if (Application.isPlaying) return;
            if (!Get.DrawGizmos) return;

            if (Get._Editor_Category == TailAnimator2.ETailCategory.Setup)
                if (Get.BaseTransform)
                    if (!FEngineering.VIsZero(Get.EndBoneJointOffset))
                    {
                        Get.RefreshTransformsList();

                        if (Get._TransformsGhostChain.Count > 0)
                        {
                            Undo.RecordObject(Get, "Changing position of tail joint offset");
                            Transform root = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1];

                            Vector3 off = root.TransformVector(Get.EndBoneJointOffset);
                            Vector3 pos = root.position + off;
                            Vector3 transformed = FEditor_TransformHandles.PositionHandle(pos, Get.BaseTransform.rotation, .3f, true, false);

                            if (Vector3.Distance(transformed, pos) > 0.00001f)
                            {
                                Vector3 diff = transformed - pos;
                                Get.EndBoneJointOffset = root.InverseTransformVector(off + diff);
                                SerializedObject obj = new SerializedObject(Get);
                                if (obj != null) { obj.ApplyModifiedProperties(); obj.Update(); }
                            }
                        }
                    }
        }
    }
}
