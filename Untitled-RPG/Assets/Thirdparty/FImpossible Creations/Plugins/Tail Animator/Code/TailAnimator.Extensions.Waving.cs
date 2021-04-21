using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {

        #region Inspector Variables

        [Tooltip("Using auto waving option to give floating effect")]
        public bool UseWaving = true;

        [Tooltip("Adding some variation to waving animation")]
        public bool CosinusAdd = false;

        [Tooltip("If you want few tails to wave in the same way you can set this sinus period cycle value")]
        public float FixedCycle = 0f;

        [Tooltip("How frequent swings should be")]
        public float WavingSpeed = 3f;
        [Tooltip("How big swings should be")]
        public float WavingRange = 0.8f;

        [Tooltip("What rotation axis should be used in auto waving")]
        public Vector3 WavingAxis = new Vector3(1.0f, 1.0f, 1.0f);

        public Quaternion WavingRotationOffset { get; private set; }

        [Tooltip("Type of waving animation algorithm, it can be simple trigonometric wave or animation based on noises (advanced)")]
        public FEWavingType WavingType = FEWavingType.Advanced;
        public enum FEWavingType { Simple, Advanced }

        [Tooltip("Offsetting perlin noise to generate different variation of tail rotations")]
        public float AlternateWave = 1f;

        #endregion



        // ------------------------ Auto Waving Section ------------------------ \\

        /// <summary> Trigonometric function time variable </summary>
        float _waving_waveTime;
        float _waving_cosTime;


        /// <summary>
        /// Defining time variables for trigonometrics
        /// </summary>
        void Waving_Initialize()
        {
            if (FixedCycle == 0f)
            {
                _waving_waveTime = Random.Range(-Mathf.PI, Mathf.PI) * 100f;
                _waving_cosTime = Random.Range(-Mathf.PI, Mathf.PI) * 50f;
            }
            else
            {
                _waving_waveTime = FixedCycle;
                _waving_cosTime = FixedCycle;
            }

            _waving_sustain = Vector3.zero;
            //_waving_sustainVelo = Vector3.zero;
        }


        /// <summary>
        /// Calculating trigonometric waving feature
        /// </summary>
        void Waving_AutoSwingUpdate()
        {
            // Defining base variables
            _waving_waveTime += justDelta * (2 * WavingSpeed);

            // Simple trigonometrical waving
            if (WavingType == FEWavingType.Simple)
            {
                float sinVal = Mathf.Sin(_waving_waveTime) * (30f * WavingRange);

                if (CosinusAdd)
                {
                    _waving_cosTime += justDelta * (2.535f * WavingSpeed);
                    sinVal += Mathf.Cos(_waving_cosTime) * (27f * WavingRange);
                }

                WavingRotationOffset = Quaternion.Euler(sinVal * WavingAxis * TailSegments[0].BlendValue);
            }
            else
            // Advanced waving based on perlin noise
            {
                float perTime = _waving_waveTime * 0.23f;

                float altX = AlternateWave * -5f;
                float altY = AlternateWave * 100f;
                float altZ = AlternateWave * 20f;

                float x = Mathf.PerlinNoise(perTime, altX) * 2.0f - 1.0f;
                float y = Mathf.PerlinNoise(altY + perTime, perTime + altY) * 2.0f - 1.0f;
                float z = Mathf.PerlinNoise(altZ, perTime) * 2.0f - 1.0f;

                WavingRotationOffset = Quaternion.Euler(Vector3.Scale(WavingAxis * WavingRange * 35f * TailSegments[0].BlendValue, new Vector3(x, y, z)));
            }
        }




        // ------------------------ Sustain Section ------------------------ \\


        /// <summary> Sustain smooth damp helper variable </summary>
        //Vector3 _waving_sustainVelo = Vector3.zero;
        /// <summary> Sustain position offset for previous procedural position</summary>
        Vector3 _waving_sustain = Vector3.zero;

        /// <summary>
        /// Calculating sustain feature offset
        /// </summary>
        void Waving_SustainUpdate()
        {
            TailSegment firstB = TailSegments[0];

            // When tail is short and have few segments we have to ampify sustain effect behaviour and vice versa
            float tailRatio = (_TC_TailLength / (float)TailSegments.Count);
            tailRatio = Mathf.Pow(tailRatio, 1.65f);
            tailRatio = (_sg_curly / tailRatio) / 6f;

            // Clamp extreme values
            if (tailRatio < 0.1f) tailRatio = 0.1f; else if (tailRatio > 1f) tailRatio = 1f;

            int mid = (int)Mathf.LerpUnclamped(TailSegments.Count * 0.4f, TailSegments.Count * 0.6f, Sustain);

            float susFact = FEasing.EaseOutExpo(1f, 0.09f, Sustain);

            float mild = 1.5f;
            mild *= (1f - TailSegments[0].Curling / 8f);
            mild *= (1.5f - tailRatio / 1.65f);
            mild *= Mathf.Lerp(0.7f, 1.2f, firstB.Slithery);
            mild *= FEasing.EaseOutExpo(1f, susFact, firstB.Springiness);

            Vector3 velo = TailSegments[mid].PreviousPush;
            if (mid + 1 < TailSegments.Count) velo += TailSegments[mid + 1].PreviousPush;
            if (mid - 1 > TailSegments.Count) velo += TailSegments[mid - 1].PreviousPush;


            _waving_sustain = velo * Sustain * mild * 2f;

            #region Backup
            //_waving_sustain = Vector3.Lerp(_waving_sustain, TailBones[mid].PreviousPush * Sustain * mild, unifiedDelta);
            //Vector3.SmoothDamp
            //(
            //_waving_sustain, 
            //TailBones[mid].PreviousPush * Sustain * mild, 
            //ref _waving_sustainVelo, 
            //Mathf.LerpUnclamped(0.15f + tailRatio / 5f, 0.1f + tailRatio / 5f, Sustain), 
            //Mathf.Infinity, Time.smoothDeltaTime);

            //TailBones[Mathf.Min(TailBones.Count - 1, _tc_startII)].PreviousPosition += _waving_sustain * mild;
            #endregion
        }


    }
}