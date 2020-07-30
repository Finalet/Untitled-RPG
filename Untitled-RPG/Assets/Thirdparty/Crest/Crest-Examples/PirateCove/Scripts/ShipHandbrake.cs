// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipHandbrake : MonoBehaviour
{
    [SerializeField] bool _activeOnStart = true;
    [SerializeField] float _timeToRelease = 1f;

    Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if(_activeOnStart)
        {
            _rb.isKinematic = true;
        }
    }

    private void Update()
    {
        _timeToRelease -= Time.deltaTime;

        if(_timeToRelease <= 0f)
        {
            enabled = false;
            _rb.isKinematic = false;
        }
    }

    public void Release()
    {
        _rb.isKinematic = false;
    }
}
