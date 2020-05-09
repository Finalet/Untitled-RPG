using MalbersAnimations.Events;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>Manage the Blend Shapes of a Mesh</summary>
    public class BlendShape : MonoBehaviour
    {
        public BlendShapePreset preset;
        public bool LoadPresetOnStart = true;
        [RequiredField]
        public SkinnedMeshRenderer mesh;
        public SkinnedMeshRenderer[] LODs;

        [Range(0, 100)]
        public float[] blendShapes;                    //Value of the Blend Shape

        public bool random;
        public int PinnedShape;
        
        /// <summary>Does the mesh has Blend Shapes? </summary>
        public bool HasBlendShapes
        {
            get { return mesh && mesh.sharedMesh.blendShapeCount > 0; }
        }

        /// <summary>Returns the current Blend Shapes Values</summary>
        public virtual float[] GetBlendShapeValues()
        {
            if (HasBlendShapes)
            {
                float[] BS = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < BS.Length; i++)
                {
                    BS[i] = mesh.GetBlendShapeWeight(i);
                }
                return BS;
            }
            return null;
        }

      
        public void SmoothBlendShape(BlendShapePreset preset)
        {
            StopAllCoroutines();
            preset.SmoothBlend(this, mesh);
        }

        private void Start()
        {
            if (LoadPresetOnStart)
            {
                LoadBlendShapePreset();
                return;
            }

            if (random) RandomizeShapes();
        }


        private void Reset()
        {
            mesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (mesh)
            {
                blendShapes = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = mesh.GetBlendShapeWeight(i);
                }
            }
        }

        public void SaveBlendShapePreset()
        {
            if (preset)
            {
                preset.blendShapes = new float[blendShapes.Length];

                for (int i = 0; i < preset.blendShapes.Length; i++)
                {
                    preset.blendShapes[i] = blendShapes[i];
                }
                Debug.Log("Preset: "+preset.name+" Saved");
            } 
        }

        public void LoadBlendShapePreset()
        {
            LoadBlendShapePreset(preset);
        }

        public void LoadBlendShapePreset(BlendShapePreset preset)
        {
            if (preset)
            {
                blendShapes = new float[preset.blendShapes.Length];

                for (int i = 0; i < preset.blendShapes.Length; i++)
                {
                    blendShapes[i] = preset.blendShapes[i];
                }

                Debug.Log("Preset: " + preset.name + " Loaded");
                UpdateBlendShapes();
            }
        }

        public virtual void SetShapesCount()
        {
            if (mesh)
            {
                blendShapes = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = mesh.GetBlendShapeWeight(i);
                }
            }
        }



        /// <summary>
        /// Set Random Values to the Mesh Blend Shapes
        /// </summary>
        public virtual void RandomizeShapes()
        {
            if (HasBlendShapes)
            {
                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = Random.Range(0, 100);
                    mesh.SetBlendShapeWeight(i, blendShapes[i]);
                }

//#if UNITY_EDITOR
//                UnityEditor.EditorUtility.SetDirty(mesh);
//#endif

                UpdateLODs();
            }
        }

        public virtual void SetBlendShape(string name, float value)
        {
            if (HasBlendShapes)
            {
                PinnedShape = mesh.sharedMesh.GetBlendShapeIndex(name);
                if (PinnedShape != -1)
                {
                    mesh.SetBlendShapeWeight(PinnedShape, value);
                }
            }
        }

        public virtual void SetBlendShape(int index, float value)
        {
            if (HasBlendShapes)
            {
                mesh.SetBlendShapeWeight(PinnedShape = index, value);
            }
        }

        public virtual void _PinShape(string name)
        {
            PinnedShape = mesh.sharedMesh.GetBlendShapeIndex(name);
        }

        public virtual void _PinShape(int index)
        {
            PinnedShape = index;
        }

        public virtual void _PinnedShapeSetValue(float value)
        {
            if (PinnedShape != -1)
            {
                value = Mathf.Clamp(value, 0, 100);
                blendShapes[PinnedShape] = value;
                mesh.SetBlendShapeWeight(PinnedShape, value);
                UpdateLODs(PinnedShape);
            }
        }


        public virtual void UpdateBlendShapes()
        {
            if (mesh && blendShapes != null)
            {
                int Length = Mathf.Min(mesh.sharedMesh.blendShapeCount, blendShapes.Length);

                for (int i = 0; i < Length; i++)
                {
                    mesh.SetBlendShapeWeight(i, blendShapes[i]);
                }

                UpdateLODs();
            }
        }

        /// <summary>Update the LODs Values</summary>
        protected virtual void UpdateLODs()
        {
            for (int i = 0; i < blendShapes.Length; i++)
            {
                UpdateLODs(i);
            }
        }

        /// <summary>Updates Only a Shape in all LODS
        protected virtual void UpdateLODs(int index)
        {
            if (LODs != null)
            {
                foreach (var lods in LODs)
                {
                    if (lods != null && lods.sharedMesh.blendShapeCount > index)
                        lods.SetBlendShapeWeight(index, blendShapes[index]);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {

            MEventListener listener = GetComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent BlendS = MalbersTools.GetInstance<MEvent>("Blend Shapes");


            if (listener.Events.Find(item => item.Event == BlendS) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = BlendS,
                    useVoid = false, useString = true, useInt = true, useFloat = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, _PinShape);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, _PinShape);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseFloat, _PinnedShapeSetValue);
                listener.Events.Add(item);

                Debug.Log("<B>Blend Shapes</B> Added to the Event Listeners");
            }
        }
#endif
    }
}