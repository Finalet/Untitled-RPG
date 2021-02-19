﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Demarcates an AABB area where water is present in the world. If present, ocean tiles will be
    /// culled if they don't overlap any WaterBody.
    /// </summary>
    [ExecuteAlways]
    public partial class WaterBody : MonoBehaviour
    {
#pragma warning disable 414
        [Tooltip("Editor only: run validation checks on Start() to check for issues."), SerializeField]
        bool _runValidationOnStart = true;
#pragma warning restore 414

        public static List<WaterBody> WaterBodies => _waterBodies;
        static List<WaterBody> _waterBodies = new List<WaterBody>();

        public Bounds AABB { get; private set; }

        private void OnEnable()
        {
            CalculateBounds();

            _waterBodies.Add(this);
        }

        private void OnDisable()
        {
            _waterBodies.Remove(this);
        }

        private void CalculateBounds()
        {
            var bounds = new Bounds();
            bounds.center = transform.position;
            bounds.Encapsulate(transform.TransformPoint(Vector3.right / 2f + Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(Vector3.right / 2f - Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(-Vector3.right / 2f + Vector3.forward / 2f));
            bounds.Encapsulate(transform.TransformPoint(-Vector3.right / 2f - Vector3.forward / 2f));

            AABB = bounds;
        }

#if UNITY_EDITOR
        private void Start()
        {
            if (EditorApplication.isPlaying && _runValidationOnStart)
            {
                Validate(OceanRenderer.Instance, ValidatedHelper.DebugLog);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Required as we're not normally executing in edit mode
            CalculateBounds();

            var oldColor = Gizmos.color;
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            var center = AABB.center;
            if (OceanRenderer.Instance != null && OceanRenderer.Instance.Root != null)
            {
                center.y = OceanRenderer.Instance.Root.position.y;
            }
            Gizmos.DrawCube(center, 2f * new Vector3(AABB.extents.x, 1f, AABB.extents.z));
            Gizmos.color = oldColor;
        }
#endif
    }

#if UNITY_EDITOR
    public partial class WaterBody : IValidated
    {
        public bool Validate(OceanRenderer ocean, ValidatedHelper.ShowMessage showMessage)
        {
            // This will also return disabled objects. Safe to use in this case.
            if (Resources.FindObjectsOfTypeAll<OceanRenderer>().Length == 0)
            {
                showMessage
                (
                    $"Water body <i>{gameObject.name}</i> requires an ocean renderer component to be present. Please create a separate Game Object and add an Ocean Renderer component to it.",
                    ValidatedHelper.MessageType.Error, this
                );

                return false;
            }

            if (Mathf.Abs(transform.lossyScale.x) < 2f && Mathf.Abs(transform.lossyScale.z) < 2f)
            {
                showMessage
                (
                    $"Water body {gameObject.name} has a very small size (the size is set by the X & Z scale of its transform). This will be a very small body of water. Is this intentional?",
                    ValidatedHelper.MessageType.Error, this
                );

                return false;
            }

            if (transform.eulerAngles.magnitude > 0.0001f)
            {
                showMessage
                (
                    $"There must be no rotation on the WaterBody GameObject, and no rotation on any parent. Currently the rotation Euler angles are {transform.eulerAngles}.",
                    ValidatedHelper.MessageType.Error, this
                );
            }

            return true;
        }
    }

    [CustomEditor(typeof(WaterBody), true), CanEditMultipleObjects]
    class WaterBodyEditor : ValidatedEditor { }
#endif
}
