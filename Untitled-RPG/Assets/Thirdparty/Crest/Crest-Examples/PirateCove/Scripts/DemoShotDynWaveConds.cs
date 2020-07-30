// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using Crest;
using UnityEngine;

[CreateAssetMenu(fileName = "Shot00", menuName = "Crest/Demo/Shot Dynamic Waves", order = 10000)]
public class DemoShotDynWaveConds : DemoShot
{
    [SerializeField] float _animationPeriod = 4f;
    [SerializeField] float _phase = Mathf.PI;
    [SerializeField] float _waitTime = 3f;
    [SerializeField] bool _oneShot = true;

    ShapeGerstnerBatched[] _gerstners;

    public override void OnPlay()
    {
        base.OnPlay();

        _gerstners = FindObjectsOfType<ShapeGerstnerBatched>();
        // i am getting the array in the reverse order compared to the hierarchy which bugs me. sort them based on sibling index,
        // which helps if the gerstners are on sibling GOs.
        System.Array.Sort(_gerstners, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    public override void UpdateShot(float shotTime, float remainingTime)
    {
        base.UpdateShot(shotTime, remainingTime);

        if(shotTime < _waitTime)
        {
            return;
        }

        float angle = 2f * Mathf.PI * (shotTime - _waitTime) / _animationPeriod;

        if(angle >= Mathf.PI * 2f && _oneShot)
        {
            _gerstners[0]._weight = 1f;
            return;
        }

        foreach(var gerstner in _gerstners)
        {
            gerstner._weight = 0.5f + 0.5f * Mathf.Cos(angle);

            // Add phase difference on animation of multiple gerstners
            angle += _phase;
        }
    }

    public override void OnStop()
    {
        base.OnStop();

        _gerstners = null;
    }
}
