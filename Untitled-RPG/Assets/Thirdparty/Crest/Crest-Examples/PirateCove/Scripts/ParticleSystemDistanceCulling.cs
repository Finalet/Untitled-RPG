using UnityEngine;

/// <summary>
/// Simple script to play / stop any child particle systems based on viewer distance from this GO.
/// </summary>
public class ParticleSystemDistanceCulling : MonoBehaviour
{
    public Transform _viewpoint;
    public float _maxDistance = 50.0f;

    bool _emitting = true;

    ParticleSystem[] _particleSystems;

    private void Start()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        var dist2 = (_viewpoint.transform.position - transform.position).sqrMagnitude;
        var maxDist2 = _maxDistance * _maxDistance;

        if (_emitting && dist2 > maxDist2)
        {
            _emitting = false;

            foreach (var system in _particleSystems)
            {
                system.Stop();
            }
        }
        else if (!_emitting && dist2 < maxDist2)
        {
            _emitting = true;

            foreach (var system in _particleSystems)
            {
                system.Play();
            }
        }
    }
}
