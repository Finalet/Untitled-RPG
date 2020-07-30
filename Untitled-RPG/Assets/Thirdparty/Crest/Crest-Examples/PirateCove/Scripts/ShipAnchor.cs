// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipAnchor : MonoBehaviour
{
    public float _springConstant = 10000f;
    public float _springConstantRotational = 10000000f;

    Rigidbody _rb;
    Vector3 _anchorPosWS;
    Vector3 _anchorForwardWS;

    private void Start()
    {
        _anchorPosWS = transform.position;
        _anchorForwardWS = transform.forward;
        _anchorPosWS.y = _anchorForwardWS.y = 0f;

        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var pos = transform.position;
        pos.y = 0f;
        var force = (_anchorPosWS - pos) * _springConstant;
        _rb.AddForce(force);

        var forward = transform.forward;
        forward.y = 0f;
        var torque = -Vector3.Cross(_anchorForwardWS, forward) * _springConstantRotational;
        _rb.AddTorque(torque);
    }
}
