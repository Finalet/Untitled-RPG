/*
    I TURNED OFF IK PASS ON THE "MAIN" ANIMATION LAYER BECASUE WITH IT ENABLED THE PLAYER SHAKES UP AND DOWN
*/

using System;
using UnityEngine;

public class SimpleHumanoidFootIK : MonoBehaviour {

    public LayerMask layerMask = 1;
    public float groundedControlTolerance = 0.02f;
    public Boolean advancedForwardDetect = true;
    public float advancedForwardDetectionRange = 0.25f;

    public float raycastDistance = 0.5f;
    
    public float fixedVerticalBodyPositionOffset = 0f;
    public float lerpValue = 0.5f;

    private Animator animator;
    private IKTarget[] targets;
    private float bodyOffset = 0f;

    private void Start()
    {
        targets = new IKTarget[2];
        targets[0] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee, Vector3.zero, false, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, new RaycastHit(), 0f, 0f, 0f);
        targets[1] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee, Vector3.zero, false, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, new RaycastHit(), 0f, 0f, 0f);
    }

    void Update()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnAnimatorIK()
    {
        if (animator == null)
            return;
        lerpValue = Mathf.Clamp01(lerpValue);
        bodyOffset = Mathf.Lerp(bodyOffset, CalculateTargets(), lerpValue);
        ApplyBodyPosition(bodyOffset);
        for(int i = 0; i<targets.Length; i++)
        {
            ApplyIK(targets[i]);
        }
    }

    private float CalculateTargets()
    {
        float maxVerticalIkDistance = 0f;
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].boneTargetDistance = i == 0 ? animator.leftFeetBottomHeight : animator.rightFeetBottomHeight;
            targets[i].ikEnabled = false;
            targets[i].rotationEnabled = false;
            targets[i].hintEnabled = false;
            Vector3 oldPosition = targets[i].position;
            
            if (Physics.Raycast(animator.GetBoneTransform(targets[i].ikBone).position + new Vector3(0, raycastDistance, 0), Vector3.down, out targets[i].hit, 2f * raycastDistance, layerMask))
            {
                float originalHitY = targets[i].hit.point.y;
                if (GetRootPosition().y + groundedControlTolerance > (animator.GetBoneTransform(targets[i].ikBone).position.y) - (targets[i].boneTargetDistance))
                {
                    // feet position is lower than tolerated height near floor (root)
                    // Must place the feet on ground
                    targets[i].position = targets[i].hit.point + new Vector3(0, targets[i].boneTargetDistance, 0);
                    targets[i].rotation = Quaternion.FromToRotation(Vector3.up, targets[i].hit.normal) * animator.GetIKRotation(targets[i].ikGoal);


                } else
                {
                    // feet position is higher than tolerated height near floor
                    // Must place on the animation height over the floor without any rotation
                    targets[i].position = targets[i].hit.point + new Vector3(0, animator.GetBoneTransform(targets[i].ikBone).position.y - GetRootPosition().y, 0);
                    targets[i].rotation = animator.GetIKRotation(targets[i].ikGoal);
                }

                if (advancedForwardDetect)
                {
                    Vector3 footVelocity = Vector3.zero;
                    if (oldPosition != Vector3.zero)
                    {
                        footVelocity = targets[i].position - oldPosition;
                    }

                    float angle = 180f;
                    if (footVelocity != Vector3.zero)
                    {
                        angle = Vector3.Angle(footVelocity, animator.velocity);
                    }

                    if (angle < 90f)
                    {
                        if (Physics.Raycast(targets[i].position + (animator.velocity.normalized * advancedForwardDetectionRange) + new Vector3(0, raycastDistance, 0), Vector3.down, out targets[i].hit, 2f * raycastDistance, layerMask))
                        {
                            if (originalHitY < targets[i].hit.point.y && Vector3.Angle(targets[i].hit.normal, Vector3.up) < 10)
                            {
                                targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, (targets[i].hit.point.y - originalHitY), lerpValue);
                                targets[i].advancedForwardDetectionHeight = Mathf.Clamp01(targets[i].advancedForwardDetectionHeight);
                                targets[i].position.y += targets[i].advancedForwardDetectionHeight;
                            } else
                            {
                                targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                            }
                        }
                        else
                        {
                            targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                        }
                    }
                    else
                    {
                        targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
                    }


                }

                targets[i].ikEnabled = true;
                targets[i].rotationEnabled = true;
                if (animator.GetBoneTransform(targets[i].ikBone).position.y - targets[i].position.y > maxVerticalIkDistance)
                    maxVerticalIkDistance = animator.GetBoneTransform(targets[i].ikBone).position.y - targets[i].position.y;

                targets[i].hintOffset = Vector3.Distance(animator.GetBoneTransform(targets[i].ikBone).position, targets[i].position);
                targets[i].hintEnabled = true;
            }
        }
        
        return maxVerticalIkDistance;
    }

    private Vector3 GetRootPosition()
    {
        if (animator != null)
        {
            return animator.rootPosition;
        }
        else
        {
            return transform.position;
        }
    }

    private void ApplyBodyPosition(float maxVerticalIkDistance)
    {
        fixedVerticalBodyPositionOffset = Mathf.Clamp(fixedVerticalBodyPositionOffset, -0.05f, 0.05f);
        Vector3 bodyPositionDelta = new Vector3(0, -maxVerticalIkDistance + fixedVerticalBodyPositionOffset, 0);
        animator.bodyPosition = animator.bodyPosition + bodyPositionDelta;
    }

    private void ApplyIK(IKTarget target)
    {
        if (animator == null)
            return;
        if (!target.ikEnabled)
        {
            animator.SetIKPositionWeight(target.ikGoal, 0.0f);
            animator.SetIKRotationWeight(target.ikGoal, 0.0f);
            animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
            return;
        }
        animator.SetIKPosition(target.ikGoal, target.position);
        animator.SetIKPositionWeight(target.ikGoal, 1.0f);

        if(!target.rotationEnabled)
        {
            animator.SetIKRotationWeight(target.ikGoal, 0.0f);
        } else
        {
            animator.SetIKRotation(target.ikGoal, target.rotation);
            animator.SetIKRotationWeight(target.ikGoal, 1.0f);
        }
        
        if (!target.hintEnabled)
        {
            animator.SetIKHintPositionWeight(target.ikHint, 0.0f);
            return;
        }

        Vector3 localPosition = this.gameObject.transform.InverseTransformPoint(animator.GetIKHintPosition(target.ikHint));
        localPosition += new Vector3(0,target.hintOffset,target.hintOffset);
        Vector3 globalPosition = this.gameObject.transform.TransformPoint(localPosition);
        animator.SetIKHintPosition(target.ikHint, globalPosition);
        animator.SetIKHintPositionWeight(target.ikHint, 1.0f);

    }

    public struct IKTarget
    {
        public Vector3 position;
        public Vector3 hintPosition;
        public Boolean ikEnabled;
        public Quaternion rotation;
        public Boolean rotationEnabled;
        public AvatarIKGoal ikGoal;
        public AvatarIKHint ikHint;
        public Boolean hintEnabled;
        public HumanBodyBones ikBone;
        public HumanBodyBones hintBone;
        public RaycastHit hit;
        public float boneTargetDistance;
        public float hintOffset;
        public float advancedForwardDetectionHeight;

        public IKTarget(Vector3 position,
            Boolean ikEnabled,
            Quaternion rotation,
            Boolean rotationEnabled,
            AvatarIKGoal ikGoal,
            AvatarIKHint ikHint,
            Vector3 hintPosition,
            Boolean hintEnabled,
            HumanBodyBones ikBone,
            HumanBodyBones hintBone,
            RaycastHit hit,
            float boneTargetDistance,
            float hintOffset,
            float advancedForwardDetectionHeight)
        {
            this.position = position;
            this.hintPosition = hintPosition;
            this.ikEnabled = ikEnabled;
            this.rotation = rotation;
            this.rotationEnabled = rotationEnabled;
            this.ikGoal = ikGoal;
            this.ikHint = ikHint;
            this.hintEnabled = hintEnabled;
            this.ikBone = ikBone;
            this.hintBone = hintBone;
            this.hit = hit;
            this.boneTargetDistance = boneTargetDistance;
            this.hintOffset = hintOffset;
            this.advancedForwardDetectionHeight = advancedForwardDetectionHeight;
        }
    }
}
