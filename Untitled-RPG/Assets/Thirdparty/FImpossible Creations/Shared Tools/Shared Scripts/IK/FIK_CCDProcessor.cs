using UnityEngine;

namespace FIMSpace.FTools
{

    /// <summary>
    /// FC: Class for processing IK logics for multiple bones inverse kinematics
    /// </summary>
    [System.Serializable]
    public class FIK_CCDProcessor : FIK_ProcessorBase
    {
        #region CCDIK Processor

        public CCDIKBone[] IKBones; // { get; private set; }
        public CCDIKBone StartIKBone { get { return IKBones[0]; } }
        public CCDIKBone EndIKBone { get { return IKBones[IKBones.Length - 1]; } }

        public bool ContinousSolving = true;
        [Range(0f, 1f)]
        public float SyncWithAnimator = 1f;
        [Range(1, 12)]
        public int ReactionQuality = 2;
        [Range(0f, 1f)]
        public float Smoothing = 0f;
        [Range(0f, 1.5f)]
        public float MaxStretching = 0f;
        public AnimationCurve StretchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public bool Use2D = false;

        public float ActiveLength { get; private set; }

        /// <summary> Assigning bones for IK processor with CCD IK logics (unlimited bone count) </summary>
        public FIK_CCDProcessor(Transform[] bonesChain)
        {
            IKBones = new CCDIKBone[bonesChain.Length];
            Bones = new CCDIKBone[IKBones.Length];

            for (int i = 0; i < bonesChain.Length; i++)
            {
                IKBones[i] = new CCDIKBone(bonesChain[i]);
                Bones[i] = IKBones[i];
            }

            IKTargetPosition = EndBone.transform.position; IKTargetRotation = EndBone.transform.rotation;
        }


        public override void Init(Transform root)
        {
            if (Initialized) return;

            fullLength = 0f;
            for (int i = 0; i < Bones.Length; i++)
            {
                CCDIKBone b = IKBones[i];
                CCDIKBone child = null, parent = null;

                if (i > 0)
                    parent = IKBones[i - 1];
                else if (i < Bones.Length - 1)
                    child = IKBones[i + 1];


                if (i < Bones.Length - 1)
                {
                    IKBones[i].Init(child, parent);
                    fullLength += b.BoneLength;
                    b.ForwardOrientation = Quaternion.Inverse(b.transform.rotation) * (IKBones[i + 1].transform.position - b.transform.position);
                }
                else
                {
                    IKBones[i].Init(child, parent);
                    b.ForwardOrientation = Quaternion.Inverse(b.transform.rotation) * (IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position);
                }
            }

            Initialized = true;
        }


        #region Methods


        /// <summary> Updating processor with n-bones oriented inverse kinematics </summary>
        public override void Update()
        {
            if (!Initialized) return;
            if (IKWeight <= 0f) return;
            CCDIKBone wb = IKBones[0];


            // Restoring previous IK progress for continous solving
            if (ContinousSolving)
            {
                while (wb != null)
                {
                    wb.LastKeyLocalRotation = wb.transform.localRotation;
                    wb.transform.localPosition = wb.LastIKLocPosition;
                    wb.transform.localRotation = wb.LastIKLocRotation;
                    wb = wb.IKChild;
                }
            }
            else
            {
                if (SyncWithAnimator > 0f)
                    // Memory for animator syncing
                    while (wb != null)
                    {
                        wb.LastKeyLocalRotation = wb.transform.localRotation;
                        wb = wb.IKChild;
                    }
            }

            if (ReactionQuality < 0) ReactionQuality = 1;

            Vector3 goalPivotOffset = Vector3.zero;
            if (ReactionQuality > 1) goalPivotOffset = GetGoalPivotOffset();

            for (int itr = 0; itr < ReactionQuality; itr++)
            {
                // Restrictions for multiple interations
                if (itr >= 1)
                    if (goalPivotOffset.sqrMagnitude == 0)
                        if (Smoothing > 0)
                            if (GetVelocityDifference() < Smoothing * Smoothing) break;

                LastLocalDirection = RefreshLocalDirection();

                Vector3 ikGoal = IKTargetPosition + goalPivotOffset;

                // Going in iterations in reversed way, from pre end child to root parent
                wb = IKBones[IKBones.Length - 2];

                if (!Use2D) // Full 3D space rotations calculations
                {
                    while (wb != null)
                    {
                        float weight = wb.MotionWeight * IKWeight;

                        if (weight > 0f)
                        {
                            Quaternion targetRotation = Quaternion.FromToRotation(Bones[Bones.Length - 1].transform.position - wb.transform.position /*fromThisToEndChildBone*/, ikGoal - wb.transform.position /*fromThisToIKGoal*/) * wb.transform.rotation;
                            if (weight < 1f) wb.transform.rotation = Quaternion.Lerp(wb.transform.rotation, targetRotation, weight);
                            else wb.transform.rotation = targetRotation;
                        }

                        wb.AngleLimiting();
                        wb = wb.IKParent;
                    }
                }
                else
                {
                    // Going in while() loop is 2x faster than for(i;i;i;) when there is more iterations
                    while (wb != null)
                    {
                        float weight = wb.MotionWeight * IKWeight;

                        if (weight > 0f)
                        {
                            Vector3 fromThisToEndChildBone = Bones[Bones.Length - 1].transform.position - wb.transform.position;
                            Vector3 fromThisToIKGoal = ikGoal - wb.transform.position;
                            wb.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(Mathf.Atan2(fromThisToEndChildBone.x, fromThisToEndChildBone.y) * Mathf.Rad2Deg /* Angle to last bone */, Mathf.Atan2(fromThisToIKGoal.x, fromThisToIKGoal.y) * Mathf.Rad2Deg /* Angle to goal position */) * weight, Vector3.back) * wb.transform.rotation;
                        }

                        wb.AngleLimiting();
                        wb = wb.IKParent;
                    }
                }
            }

            LastLocalDirection = RefreshLocalDirection();

            // Support for stretching
            if (MaxStretching > 0f)
            {
                ActiveLength = Mathf.Epsilon;

                wb = IKBones[0];
                while (wb.IKChild != null)
                {
                    wb.FrameWorldLength = (wb.transform.position - wb.IKChild.transform.position).magnitude;
                    ActiveLength += wb.FrameWorldLength;
                    wb = wb.IKChild;
                }

                Vector3 toGoal = IKTargetPosition - StartBone.transform.position;
                float stretch = toGoal.magnitude / ActiveLength;

                if (stretch > 1f) for (int i = 1; i < IKBones.Length; ++i) IKBones[i].transform.position += toGoal.normalized * (IKBones[i - 1].BoneLength * MaxStretching) * StretchCurve.Evaluate(-(1f - stretch));
            }


            wb = IKBones[0];
            while (wb != null)
            {
                // Storing final rotations for animator offset
                wb.LastIKLocRotation = wb.transform.localRotation;
                wb.LastIKLocPosition = wb.transform.localPosition;

                // Offset based rotation sync with animator
                Quaternion ikDiff = wb.LastIKLocRotation * Quaternion.Inverse(wb.InitialLocalRotation);
                wb.transform.localRotation = Quaternion.Lerp(wb.LastIKLocRotation, ikDiff * wb.LastKeyLocalRotation, SyncWithAnimator);

                if (IKWeight < 1f)
                    wb.transform.localRotation = Quaternion.Lerp(wb.LastKeyLocalRotation, wb.transform.localRotation, IKWeight);

                wb = wb.IKChild;
            }
        }


        protected Vector3 GetGoalPivotOffset()
        {
            if (!GoalPivotOffsetDetected()) return Vector3.zero;

            Vector3 IKDirection = (IKTargetPosition - IKBones[0].transform.position).normalized;
            Vector3 secondaryDirection = new Vector3(IKDirection.y, IKDirection.z, IKDirection.x);

            if (IKBones[IKBones.Length - 2].AngleLimit < 180 || IKBones[IKBones.Length - 2].TwistAngleLimit < 180)
                secondaryDirection = IKBones[IKBones.Length - 2].transform.rotation * IKBones[IKBones.Length - 2].ForwardOrientation;

            return Vector3.Cross(IKDirection, secondaryDirection) * IKBones[IKBones.Length - 2].BoneLength * 0.5f;
        }

        private bool GoalPivotOffsetDetected()
        {
            if (!Initialized) return false;

            Vector3 toLastDirection = Bones[Bones.Length - 1].transform.position - Bones[0].transform.position;
            Vector3 toGoalDirection = IKTargetPosition - Bones[0].transform.position;

            float toLastMagn = toLastDirection.magnitude;
            float toGoalMagn = toGoalDirection.magnitude;

            if (toGoalMagn == 0) return false;
            if (toLastMagn == 0) return false;
            if (toLastMagn < toGoalMagn) return false;
            if (toLastMagn < fullLength - (Bones[Bones.Length - 2].BoneLength * 0.1f)) return false;
            if (toGoalMagn > toLastMagn) return false;

            float dot = Vector3.Dot(toLastDirection / toLastMagn, toGoalDirection / toGoalMagn);
            if (dot < 0.999f) return false;

            return true;
        }

        Vector3 RefreshLocalDirection()
        {
            LocalDirection = Bones[0].transform.InverseTransformDirection(Bones[Bones.Length - 1].transform.position - Bones[0].transform.position);
            return LocalDirection;
        }

        float GetVelocityDifference()
        { return Vector3.SqrMagnitude(LocalDirection - LastLocalDirection); }



        /// <summary> Limiting angle for all IK bones </summary>
        public void AutoLimitAngle(float angleLimit = 60f, float twistAngleLimit = 50f)
        {
            if (IKBones == null) return;

            float step = 1f / (float)IKBones.Length;
            for (int i = 0; i < IKBones.Length; i++)
            {
                IKBones[i].AngleLimit = angleLimit * Mathf.Min(1f, (i + 1) * step * 3f);
                IKBones[i].TwistAngleLimit = twistAngleLimit * Mathf.Min(1f, (i + 1) * step * 4.5f);
            }
        }


        /// <summary> Spreading weight over IK bones automatically </summary>
        public void AutoWeightBones(float baseValue = 1f)
        {
            float step = baseValue / (float)(Bones.Length * 1.3f);
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].MotionWeight = baseValue - step * i;
                //Bones[i].MotionWeight *= Mathf.Min(1f, (i + 1) * step1 * 3f);
            }
        }


        /// <summary> Spreading weight over IK bones with curve (Clamped01) </summary>
        public void AutoWeightBones(AnimationCurve weightCurve)
        {
            for (int i = 0; i < Bones.Length; i++)
                Bones[i].MotionWeight = Mathf.Clamp(weightCurve.Evaluate((float)i / (float)Bones.Length), 0f, 1f);
        }


        #endregion

        #endregion


        [System.Serializable]
        public class CCDIKBone : FIK_IKBoneBase
        {
            public CCDIKBone IKParent { get; private set; }
            public CCDIKBone IKChild { get; private set; }

            [Range(0f, 180f)] public float AngleLimit = 45f;
            [Range(0f, 180f)] public float TwistAngleLimit = 5f;


            /// <summary> Defined at Init() of CCD IK processor </summary>
            public Vector3 ForwardOrientation;
            public float FrameWorldLength = 1f;

            public Vector2 HingeLimits = Vector2.zero;
            public Quaternion PreviousHingeRotation;
            public float PreviousHingeAngle;

            public Vector3 LastIKLocPosition;
            public Quaternion LastIKLocRotation;

            public CCDIKBone(Transform t) : base(t) { }

            public void Init(CCDIKBone child, CCDIKBone parent)
            {
                LastIKLocPosition = transform.localPosition;
                IKParent = parent;
                if (child != null) SetChild(child);
                IKChild = child;
            }

            public override void SetChild(FIK_IKBoneBase child)
            {
                base.SetChild(child);
            }

            #region CCD IK Methods

            public void AngleLimiting()
            {
                Quaternion localRotation = Quaternion.Inverse(LastKeyLocalRotation) * transform.localRotation;
                Quaternion limitedRotation = localRotation;

                if (FEngineering.VIsZero(HingeLimits))
                {
                    if (AngleLimit < 180) limitedRotation = LimitSpherical(limitedRotation);
                    if (TwistAngleLimit < 180) limitedRotation = LimitZ(limitedRotation);
                }
                else limitedRotation = LimitHinge(limitedRotation);

                if (FEngineering.QIsSame(limitedRotation, localRotation)) return;

                transform.localRotation = LastKeyLocalRotation * limitedRotation;
            }

            private Quaternion LimitSpherical(Quaternion rotation)
            {
                if (FEngineering.QIsZero(rotation)) return rotation;
                Vector3 currentForward = rotation * ForwardOrientation;
                Quaternion limitAngle = Quaternion.RotateTowards(Quaternion.identity, Quaternion.FromToRotation(ForwardOrientation, currentForward), AngleLimit);
                return Quaternion.FromToRotation(currentForward, limitAngle * ForwardOrientation) * rotation;
            }

            private Quaternion LimitZ(Quaternion currentRotation)
            {
                Vector3 orthoOrientation = new Vector3(ForwardOrientation.y, ForwardOrientation.z, ForwardOrientation.x);
                Vector3 normal = currentRotation * ForwardOrientation;
                Vector3 tangent = orthoOrientation;
                Vector3.OrthoNormalize(ref normal, ref tangent);

                orthoOrientation = currentRotation * orthoOrientation;
                Vector3.OrthoNormalize(ref normal, ref orthoOrientation);

                Quaternion limitRot = Quaternion.FromToRotation(orthoOrientation, tangent) * currentRotation;
                if (TwistAngleLimit <= 0) return limitRot;

                return Quaternion.RotateTowards(limitRot, currentRotation, TwistAngleLimit);
            }

            private Quaternion LimitHinge(Quaternion rotation)
            {
                Quaternion addRotation = (Quaternion.FromToRotation(rotation * ForwardOrientation, ForwardOrientation) * rotation) * Quaternion.Inverse(PreviousHingeRotation);
                float addAngle = Quaternion.Angle(Quaternion.identity, addRotation);

                Vector3 orthoOrientation = new Vector3(ForwardOrientation.z, ForwardOrientation.x, ForwardOrientation.y);
                Vector3 cross = Vector3.Cross(orthoOrientation, ForwardOrientation);
                if (Vector3.Dot(addRotation * orthoOrientation, cross) > 0f) addAngle = -addAngle;

                PreviousHingeAngle = Mathf.Clamp(PreviousHingeAngle + addAngle, HingeLimits.x, HingeLimits.y);
                PreviousHingeRotation = Quaternion.AngleAxis(PreviousHingeAngle, ForwardOrientation);
                return PreviousHingeRotation;
            }

            #endregion

        }

    }


}
