﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Tags this object as an ocean depth provider. Renders depth every frame and should only be used for dynamic objects.
    /// For static objects, use an Ocean Depth Cache.
    /// </summary>
    [ExecuteAlways]
    public class RegisterSeaFloorDepthInput : RegisterLodDataInput<LodDataMgrSeaFloorDepth>
    {
        public override bool Enabled => true;

        public bool _assignOceanDepthMaterial = true;

        public override float Wavelength => 0f;

        protected override Color GizmoColor => new Color(1f, 0f, 0f, 0.5f);

        protected override string ShaderPrefix => "Crest/Inputs/Depth";

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_assignOceanDepthMaterial)
            {
                var rend = GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Crest/Inputs/Depth/Ocean Depth From Geometry"));
            }
        }
    }
}
