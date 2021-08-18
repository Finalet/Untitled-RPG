﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Debug draw a line trace from this gameobjects position, in this gameobjects forward direction.
    /// </summary>
    [AddComponentMenu(Internal.Constants.MENU_PREFIX_DEBUG + "Visualise Ray Trace")]
    public class VisualiseRayTrace : MonoBehaviour
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        RayTraceHelper _rayTrace = new RayTraceHelper(50f, 2f);

        void Update()
        {
            if (OceanRenderer.Instance == null || OceanRenderer.Instance.CollisionProvider == null)
            {
                return;
            }

            // Even if only a single ray trace is desired, this still must be called every frame until Trace() returns true
            _rayTrace.Init(transform.position, transform.forward);
            if (_rayTrace.Trace(out float dist))
            {
                var endPos = transform.position + transform.forward * dist;
                Debug.DrawLine(transform.position, endPos, Color.green);
                VisualiseCollisionArea.DebugDrawCross(endPos, 2f, Color.green, 0f);
            }
            else
            {
                Debug.DrawLine(transform.position, transform.position + transform.forward * 50f, Color.red);
            }
        }
    }
}
