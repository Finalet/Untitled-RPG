using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/FireBreath")]
    public class FireBreath : MonoBehaviour
    {
        ParticleSystem.EmissionModule emission;
        ParticleSystem.MainModule particles;

        public float rateOverTime = 500f;

        void Start()
        {
            var Particles = GetComponent<ParticleSystem>();
            particles = Particles.main;
            emission = Particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
        }

        public void Activate(bool value)
        {
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(value ? rateOverTime : 0);
        }

        public void FireBreathColor(ColorVar newcolor)
        {
            particles.startColor = new ParticleSystem.MinMaxGradient(newcolor);
        }

        public void FireBreathColor(Color newcolor)
        {
            particles.startColor = new ParticleSystem.MinMaxGradient(newcolor);
        }
    }
}
