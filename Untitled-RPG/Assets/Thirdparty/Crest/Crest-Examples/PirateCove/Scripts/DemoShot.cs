// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

[CreateAssetMenu(fileName = "Shot00", menuName = "Crest/Demo/Shot", order = 10000)]
public class DemoShot : ScriptableObject
{
    public AnimationClip _cameraAnimation;
    public string _demoText = "<Shot caption>";

    public virtual void OnPlay()
    {
    }

    public virtual void UpdateShot(float shotTime, float remainingTime)
    {
    }

    public virtual void OnStop()
    {
    }
}
