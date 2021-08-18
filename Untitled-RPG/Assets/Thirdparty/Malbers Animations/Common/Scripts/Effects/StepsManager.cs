using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> This will manage the steps sounds and tracks for each animal, on each feet there's a Script StepTriger (Basic)  </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Step Manager")]
    public class StepsManager : MonoBehaviour, IAnimatorListener
    {
        [Tooltip("Enable Disable the Steps Manager")]
        public bool Active = true;
        [Tooltip("Layer Mask used to find the ground")]
        public LayerReference GroundLayer = new LayerReference(1);
        [Tooltip("Global Particle System for the Tracks, to have more individual tracks ")]
        public ParticleSystem Tracks;
        [Tooltip("Particle System for the Dust")]
        public ParticleSystem Dust;
        [Tooltip("This will instantiate a gameObject instead of using the Particle system")]
        public bool instantiateTracks = false;
        public float StepsVolume = 0.2f;
        public int DustParticles = 30;

        [Tooltip("Scale of the dust and track particles")]
        public Vector3 Scale = Vector3.one;

        public AudioClip[] clips;
        [Tooltip("Distance to Instantiate the tracks on a terrain")]
        public float trackOffset = 0.0085f;


        //Is Called by any of the "StepTrigger" Script on a feet when they collide with the ground.
        internal void EnterStep(StepTrigger foot)
        {
            if (!Active) return;

            var Tracks = foot.Tracks != null ? foot.Tracks : this.Tracks;

            if (Tracks && Tracks.gameObject.IsPrefab() && !instantiateTracks)         //If is a prefab clone it!
            {
                Tracks = Instantiate(Tracks,transform, false);
                Tracks.transform.localScale = Scale;
            }

            if (Dust && Dust.gameObject.IsPrefab())
            {
                Dust = Instantiate(Dust, transform, false);             //If is a prefab clone it!
                Dust.transform.localScale = Scale;
            }

            if (foot.StepAudio && clips.Length > 0) //If the track has an AudioSource Component and whe have some audio to play
            {
                foot.StepAudio.clip = clips[Random.Range(0, clips.Length)];  //Set the any of the Audio Clips from the list to the Feet's AudioSource Component
                foot.StepAudio.Play();  //Play the Audio
            }

            //Track and particles
            if (!foot.HasTrack)  // If we are ready to set a new track
            {
                if (Physics.Raycast(foot.transform.position, -transform.up, out RaycastHit footRay, 1, GroundLayer.Value))
                {
                    if (Tracks && !footRay.collider.attachedRigidbody)
                    {
                        if (instantiateTracks)
                        {
                            Instantiate(Tracks, new Vector3(foot.transform.position.x, footRay.point.y + trackOffset, foot.transform.position.z), Quaternion.identity);
                        }
                        else
                        {
                            ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
                            {
                                rotation3D = (Quaternion.FromToRotation(-foot.transform.forward, footRay.normal) * foot.transform.rotation).eulerAngles, //Get The Rotation
                                position = new Vector3(foot.transform.position.x, footRay.point.y + trackOffset, foot.transform.position.z) //Get The Position
                            };

                            Tracks.Emit(ptrack, 1);
                        }
                    }

                    if (Dust)
                    {
                        Dust.transform.position = new Vector3(foot.transform.position.x, footRay.point.y + trackOffset, foot.transform.position.z); //Get The Position
                        Dust.transform.rotation = (Quaternion.FromToRotation(-foot.transform.forward, footRay.normal) * foot.transform.rotation);
                        Dust.transform.Rotate(-90, 0, 0);
                        Dust.Emit(DustParticles);
                    }
                }
            }
        }

        /// <summary>Disable this sfcript, e.g.. deactivate when is sleeping or death </summary>
        public virtual void EnableSteps(bool value) => Active = value;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }
}
