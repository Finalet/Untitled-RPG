using UnityEngine;
using System.Collections;
using MalbersAnimations.Utilities;

namespace MalbersAnimations
{
    /// <summary>Works with the Step manager ... get the terrain below the animal </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Step Trigger")]
    public class StepTrigger : MonoBehaviour
    {
        [RequiredField] 
        public StepsManager m_StepsManager;
       
        [Tooltip("Particle System for the Tracks")]
        public ParticleSystem Tracks;


        public float WaitNextStep = 0.2f;
        public AudioSource StepAudio; 


        private LayerMask GroundLayer => m_StepsManager.GroundLayer.Value;

        WaitForSeconds wait;
        bool waitrack;                      // Check if is time to put a track; 
        public bool HasTrack { get; set; }

        void Awake()
        {
           if (m_StepsManager == null)  m_StepsManager = transform.root.FindComponent<StepsManager>();
           

            if (m_StepsManager == null) //If there's no  StepManager Remove the Stepss
            {
                Destroy(gameObject);
                return;
            }
             

            if (m_StepsManager.Active == false) //If there's no  StepManager Remove the Stepss
            {
                gameObject.SetActive(false);
                return;
            }

            StepAudio = GetComponent<AudioSource>();

            if (StepAudio == null)
            {
                StepAudio = gameObject.AddComponent<AudioSource>();
            }

            StepAudio.spatialBlend = 1;  //Make the Sound 3D
            if (m_StepsManager) StepAudio.volume = m_StepsManager.StepsVolume;    

            wait = new WaitForSeconds(WaitNextStep); 
        }


        void OnTriggerEnter(Collider other)
        {
            if (m_StepsManager != null) //If there's no  StepManager Remove the Stepss
            {
                if (!MTools.CollidersLayer(other, GroundLayer)) return; //Ignore layers that are not Ground

                if (!waitrack)
                {
                    StartCoroutine(WaitForStep());     //Wait Half a Second before making another Step
                    m_StepsManager.EnterStep(this);
                    HasTrack = true;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!MTools.CollidersLayer(other, GroundLayer)) return; //Ignore layers that are not Ground
            HasTrack = false; // if the feet is on the air then can put a track
        }

        [ContextMenu("Find Audio Source")]
        private void FindAudioSource()
        {
            StepAudio = GetComponent<AudioSource>();
            if (StepAudio)
            {
                StepAudio.spatialBlend = 1;  //Make the Sound 3D
                if (m_StepsManager) StepAudio.volume = m_StepsManager.StepsVolume;
                StepAudio.maxDistance = 5;
                StepAudio.minDistance = 1;
                StepAudio.playOnAwake = false;
            }
        }

        IEnumerator WaitForStep()
        {
            waitrack =  true;
            yield return wait;
            waitrack = false;
        }
    }
}