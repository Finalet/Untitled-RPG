using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FC: Experimental class under developement
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Tail Animator Utilities/Tail Animator Wind")]
    public class TailAnimatorWind : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {

        #region Hierarchy Icon

        public string EditorIconPath { get { return "Tail Animator/TailAnimatorWindIconSmall"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        #endregion


        #region Singleton


        public static TailAnimatorWind Instance { get { if (!_instance) GenerateWindComponentInstance(); return _instance; } }
        private static TailAnimatorWind _instance;


        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (_instance != null)
            {
                if (_instance != this)
                {
                    GameObject.Destroy(_instance);
                    _instance = this;
                    DontDestroyOnLoad(gameObject);
                    Debug.Log("[Tail Animator Wind] Override wind component instance! (" + name + ")");
                }
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }


        private static void GenerateWindComponentInstance()
        {
            GameObject windObj = new GameObject("Tail Animator Wind");
            _instance = windObj.AddComponent<TailAnimatorWind>();
        }


        /// <summary>
        /// Generating wind component if needed and adding tail animator component to wind affected components list
        /// </summary>
        public static void Refresh(TailAnimator2 tail)
        {
            if (!_instance) GenerateWindComponentInstance();
            if (_instance.WindAffected == null) _instance.WindAffected = new List<TailAnimator2>();
            if (tail != null) if (!_instance.WindAffected.Contains(tail)) _instance.WindAffected.Add(tail);
        }


        #endregion


        [Header("In playmode you will find this object in DontDestroyOnLoad")]
        [FPD_Header("Main Wind Setings", 2, 4)]
        public Vector3 overrideWind = Vector3.zero;
        public float power = 1f;
        public float additionalTurbulence = 1f;
        public float additionalTurbSpeed = 1f;

        [FPD_Header("Procedural Wind Settings", 6, 4)]
        [Range(0.1f, 1f)]
        public float rapidness = 0.95f;
        [FPD_Suffix(0, 360, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "o")]
        public float changesPower = 90f;
        [Range(0f, 10f)]
        public float turbulenceSpeed = 1f;

        [FPD_Header("World Position Turbulence", 6, 4)]
        public float worldTurb = 1f;
        [Tooltip("If higher no performance cost, it is just a number")]
        public float worldTurbScale = 512;
        public float worldTurbSpeed = 0.25f;

        [FPD_Header("Tail Compoenents Related", 6, 4)]
        [Tooltip("When tail is longer then power of wind should be higher")]
        public bool powerDependOnTailLength = true;
        [Tooltip("Finding all TailAnimato2 compoents at start")]
        public bool collectFromSceneAtStart = false;

        public List<TailAnimator2> WindAffected;

        private Vector3 targetWind = Vector3.zero;
        private Vector3 smoothWind = Vector3.zero;
        private Vector3 windVeloHelper = Vector3.zero;
        private Quaternion windOrientation = Quaternion.identity;
        private Quaternion smoothWindOrient = Quaternion.identity;
        private Quaternion smoothWindOrientHelper = Quaternion.identity;

        private float[] randNumbers;
        private float[] randTimes;
        private float[] randSpeeds;

        private int frameOffset = 2;


        void Update()
        {
            if (frameOffset > 0) { frameOffset--; return; }

            if (collectFromSceneAtStart)
            {
                collectFromSceneAtStart = false;
                GetTailAnimatorsFromScene();
            }

            ComputeWind();

            TailAnimator2 t;
            for (int i = 0; i < WindAffected.Count; i++)
            {
                t = WindAffected[i];

                if (!t.UseWind) continue;
                if (t.WindEffectPower <= 0f) continue;
                if (t.TailSegments.Count <= 0) continue;

                float lengthRatio = 1f;
                if (powerDependOnTailLength)
                {
                    lengthRatio = (t._TC_TailLength * t.TailSegments[0].transform.lossyScale.z) / 5f;
                    if (t.TailSegments.Count > 3) lengthRatio *= Mathf.Lerp(0.7f, 3f, t.TailSegments.Count / 14f);
                }

                if (t.WindWorldNoisePower > 0f)
                {
                    float worldPosTurbulence = (.5f + Mathf.Sin(Time.time * worldTurbSpeed + t.TailSegments[0].ProceduralPosition.x * worldTurbScale) / 2f) + (.5f + Mathf.Cos(Time.time * worldTurbSpeed + t.TailSegments[0].ProceduralPosition.z * worldTurbScale) / 2f);
                    lengthRatio += worldPosTurbulence * worldTurb * t.WindWorldNoisePower;
                }

                lengthRatio *= t.WindEffectPower;

                if (t.WindTurbulencePower > 0f)
                    t.WindEffect = new Vector3(targetWind.x * lengthRatio + finalAddTurbulence.x * t.WindTurbulencePower, targetWind.y * lengthRatio + finalAddTurbulence.y * t.WindTurbulencePower, targetWind.z * lengthRatio + finalAddTurbulence.z * t.WindTurbulencePower);
                else
                    t.WindEffect = new Vector3(targetWind.x * lengthRatio, targetWind.y * lengthRatio, targetWind.z * lengthRatio);

            }
        }


        private void Start()
        {
            int numCount = 10;
            randNumbers = new float[numCount];
            randTimes = new float[numCount];
            randSpeeds = new float[numCount];

            for (int i = 0; i < 10; i++)
            {
                randNumbers[i] = Random.Range(-1000f, 1000f);
                randTimes[i] = Random.Range(-1000f, 1000f);
                randSpeeds[i] = Random.Range(0.18f, 0.7f);
            }
        }


        void ComputeWind()
        {

            Vector3 newWind;
            if (overrideWind != Vector3.zero) newWind = overrideWind;
            else // Procedural wind
            {
                for (int i = 0; i < 4; i++)
                    randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed;

                Quaternion windDir = windOrientation;

                float x = -1f + Mathf.PerlinNoise(randTimes[0], 256f + randTimes[1]) * 2f;
                float y = -1f + Mathf.PerlinNoise(-randTimes[1], 55f + randTimes[2]) * 2f;
                float z = -1f + Mathf.PerlinNoise(-randTimes[3], 55f + randTimes[0]) * 2f;
                windDir *= Quaternion.Euler(new Vector3(0, y, 0) * changesPower);
                windDir = Quaternion.Euler(x * (changesPower / 6f), windDir.eulerAngles.y, z * (changesPower / 6f));

                smoothWindOrient = FEngineering.SmoothDampRotation(smoothWindOrient, windDir, ref smoothWindOrientHelper, 1f - rapidness, Time.deltaTime);

                transform.rotation = smoothWindOrient;
                newWind = smoothWindOrient * Vector3.forward;
            }

            // Additional turbulence
            smoothAddTurbulence = Vector3.SmoothDamp(smoothAddTurbulence, GetAddTurbulence() * additionalTurbulence, ref addTurbHelper, 0.05f, Mathf.Infinity, Time.deltaTime);

            // Smooth out
            smoothWind = Vector3.SmoothDamp(smoothWind, newWind, ref windVeloHelper, 0.1f, Mathf.Infinity, Time.deltaTime);

            for (int i = 7; i < 10; i++)
                randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed;

            float turbulencedPower = power * 0.015f;
            turbulencedPower *= 0.5f + Mathf.PerlinNoise(randTimes[7] * 2f, 25 + randTimes[8] * 0.5f);

            finalAddTurbulence = smoothAddTurbulence * turbulencedPower;
            targetWind = smoothWind * turbulencedPower;
        }

        Vector3 finalAddTurbulence = Vector3.zero;
        Vector3 addTurbHelper = Vector3.zero;
        private Vector3 GetAddTurbulence()
        {

            for (int i = 4; i < 7; i++)
                randTimes[i] += Time.deltaTime * randSpeeds[i] * additionalTurbSpeed;

            float x = -1f + Mathf.PerlinNoise(randTimes[4] + 7.123f, -2.324f + Time.time * 0.24f) * 2f;
            float y = -1f + Mathf.PerlinNoise(randTimes[5] - 4.7523f, -25.324f + Time.time * 0.54f) * 2f;
            float z = -1f + Mathf.PerlinNoise(randTimes[6] + 1.123f, -63.324f + Time.time * -0.49f) * 2f;
            return new Vector3(x, y, z);
        }

        Vector3 smoothAddTurbulence = Vector3.zero;


        /// <summary>
        /// Collecting tail animator components from scene (WARNING: Don't execute it every frame)
        /// </summary>
        public void GetTailAnimatorsFromScene()
        {
            TailAnimator2[] tails = FindObjectsOfType<TailAnimator2>();
            for (int i = 0; i < tails.Length; i++)
            {
                if (!WindAffected.Contains(tails[i])) WindAffected.Add(tails[i]);
            }
        }
    }
}