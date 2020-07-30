﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Registers a custom input to the dynamic wave simulation. Attach this GameObjects that you want to influence the sim to add ripples etc.
    /// </summary>
    [ExecuteAlways]
    public class RegisterDynWavesInput : RegisterLodDataInput<LodDataMgrDynWaves>
    {
        public override float Wavelength => 0f;

        public override bool Enabled => true;

        protected override Color GizmoColor => new Color(0f, 1f, 0f, 0.5f);

        protected override string ShaderPrefix => "Crest/Inputs/Dynamic Waves";
    }
}
