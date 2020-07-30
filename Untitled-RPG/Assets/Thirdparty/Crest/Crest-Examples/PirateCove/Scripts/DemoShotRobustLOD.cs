// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using Crest;
using UnityEngine;

[CreateAssetMenu(fileName = "Shot00", menuName = "Crest/Demo/Shot Robust LOD", order = 10000)]
public class DemoShotRobustLOD : DemoShot
{
    OceanDebugGUI _debugGUI;

    // Cache debug gui state
    //bool _visWaves = false;
    //bool _visFoam = false;
    bool _showGUI = false;
    bool _showTargets = false;

    public override void OnPlay()
    {
        base.OnPlay();

        _debugGUI = FindObjectOfType<OceanDebugGUI>();

        if (_debugGUI)
        {
            // Save visibility state
            //_visWaves = _debugGUI.GetTargetVisible<LodDataMgrAnimWaves>();
            //_visFoam = _debugGUI.GetTargetVisible<LodDataMgrFoam>();

            //_debugGUI.SetTargetVisible<LodDataMgrAnimWaves>(true);
            //_debugGUI.SetTargetVisible<LodDataMgrFoam>(true);

            _showGUI = _debugGUI._guiVisible;
            _showTargets = _debugGUI._showOceanData;

            _debugGUI._guiVisible = false;
            _debugGUI._showOceanData = false;
        }
    }

    public override void UpdateShot(float shotTime, float remainingTime)
    {
        base.UpdateShot(shotTime, remainingTime);

        if (_debugGUI)
        {
            _debugGUI._showOceanData = shotTime > 1f && remainingTime > 1f;
        }
    }

    public override void OnStop()
    {
        base.OnStop();

        if (_debugGUI)
        {
            // Pop visibility state
            //_debugGUI.SetTargetVisible<LodDataMgrAnimWaves>(_visWaves);
            //_debugGUI.SetTargetVisible<LodDataMgrFoam>(_visFoam);

            _debugGUI._guiVisible = _showGUI;
            _debugGUI._showOceanData = _showTargets;

            _debugGUI = null;
        }
    }
}
