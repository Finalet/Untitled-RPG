using UnityEngine;

namespace FIMSpace
{
    public class FImp_ColliderData_Capsule : FImp_ColliderData_Base
    {
        public CapsuleCollider Capsule { get; private set; }
        public CapsuleCollider2D Capsule2D { get; private set; }
        private Vector3 Top;
        private Vector3 Bottom;
        private Vector3 Direction;
        private float radius;
        private float scaleFactor;
        private float preRadius;

        public FImp_ColliderData_Capsule(CapsuleCollider collider)
        {
            Is2D = false;
            Transform = collider.transform;
            Collider = collider;
            Transform = collider.transform;
            Capsule = collider;
            ColliderType = EFColliderType.Capsule;
            CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
            RefreshColliderData();
        }

        public FImp_ColliderData_Capsule(CapsuleCollider2D collider)
        {
            Is2D = true;
            Transform = collider.transform;
            Collider2D = collider;
            Transform = collider.transform;
            Capsule2D = collider;
            ColliderType = EFColliderType.Capsule;
            CalculateCapsuleParameters(Capsule2D, ref Direction, ref radius, ref scaleFactor);
            RefreshColliderData();
        }

        public override void RefreshColliderData()
        {
            if (IsStatic) return; // No need to refresh collider data if it is static

            bool diff = false;

            if (!FEngineering.VIsSame(previousPosition, Transform.position)) diff = true;
            else
                if (!FEngineering.QIsSame(Transform.rotation, previousRotation)) diff = true;
            else
            {
                if (Is2D == false)
                {
                    if (preRadius != Capsule.radius || !FEngineering.VIsSame(previousScale, Transform.lossyScale))
                        CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
                }
                else
                {
                    if (preRadius != GetCapsule2DRadius(Capsule2D) || !FEngineering.VIsSame(previousScale, Transform.lossyScale))
                        CalculateCapsuleParameters(Capsule2D, ref Direction, ref radius, ref scaleFactor);
                }
            }

            if (diff)
            {
                if (Is2D == false)
                    GetCapsuleHeadsPositions(Capsule, ref Top, ref Bottom, Direction, radius, scaleFactor);
                else
                    GetCapsuleHeadsPositions(Capsule2D, ref Top, ref Bottom, Direction, radius, scaleFactor);
            }

            base.RefreshColliderData();

            previousPosition = Transform.position;
            previousRotation = Transform.rotation;
            previousScale = Transform.lossyScale;

            if (Is2D == false) preRadius = Capsule.radius; else preRadius = GetCapsule2DRadius(Capsule2D);
        }

        public override bool PushIfInside(ref Vector3 point, float pointRadius, Vector3 pointOffset)
        {
            return PushOutFromCapsuleCollider(pointRadius, ref point, Top, Bottom, radius, pointOffset, Is2D);
        }


        public static bool PushOutFromCapsuleCollider(CapsuleCollider capsule, float segmentColliderRadius, ref Vector3 pos, Vector3 segmentOffset)
        {
            Vector3 direction = Vector3.zero; float capsuleRadius = capsule.radius, scalerFactor = 1f;
            CalculateCapsuleParameters(capsule, ref direction, ref capsuleRadius, ref scalerFactor);
            Vector3 up = Vector3.zero, bottom = Vector3.zero;
            GetCapsuleHeadsPositions(capsule, ref up, ref bottom, direction, capsuleRadius, scalerFactor);
            return PushOutFromCapsuleCollider(segmentColliderRadius, ref pos, up, bottom, capsuleRadius, segmentOffset);
        }

        public static bool PushOutFromCapsuleCollider(float segmentColliderRadius, ref Vector3 segmentPos, Vector3 capSphereCenter1, Vector3 capSphereCenter2, float capsuleRadius, Vector3 segmentOffset, bool is2D = false)
        {
            float radius = capsuleRadius + segmentColliderRadius;
            Vector3 capsuleUp = capSphereCenter2 - capSphereCenter1;
            Vector3 fromCenter = (segmentPos + segmentOffset) - capSphereCenter1;

            if (is2D)
            {
                capsuleUp.z = 0;
                fromCenter.z = 0;
            }

            float orientationDot = Vector3.Dot(fromCenter, capsuleUp);

            if (orientationDot <= 0) // Main Sphere Cap
            {
                float sphereRefDistMagn = fromCenter.sqrMagnitude;

                if (sphereRefDistMagn > 0 && sphereRefDistMagn < radius * radius)
                {
                    segmentPos = capSphereCenter1 - segmentOffset + fromCenter * (radius / Mathf.Sqrt(sphereRefDistMagn));
                    return true;
                }
            }
            else
            {
                float upRefMagn = capsuleUp.sqrMagnitude;
                if (orientationDot >= upRefMagn) // Counter Sphere Cap
                {
                    fromCenter = (segmentPos + segmentOffset) - capSphereCenter2;
                    float sphereRefDistMagn = fromCenter.sqrMagnitude;

                    if (sphereRefDistMagn > 0 && sphereRefDistMagn < radius * radius)
                    {
                        segmentPos = capSphereCenter2 - segmentOffset + fromCenter * (radius / Mathf.Sqrt(sphereRefDistMagn));
                        return true;
                    }
                }
                else if (upRefMagn > 0) // Cylinder Volume
                {
                    fromCenter -= capsuleUp * (orientationDot / upRefMagn);
                    float sphericalRefDistMagn = fromCenter.sqrMagnitude;

                    if (sphericalRefDistMagn > 0 && sphericalRefDistMagn < radius * radius)
                    {
                        float projectedDistance = Mathf.Sqrt(sphericalRefDistMagn);
                        segmentPos += fromCenter * ((radius - projectedDistance) / projectedDistance);
                        return true;
                    }
                }
            }

            return false;
        }



        #region Capsule Calculations Helpers

        /// <summary>
        /// Calculating capsule's centers of up and down sphere which are fitting unity capsule collider with all collider's transformations
        /// </summary>
        protected static void CalculateCapsuleParameters(CapsuleCollider capsule, ref Vector3 direction, ref float trueRadius, ref float scalerFactor)
        {
            Transform cTransform = capsule.transform;

            float radiusScaler;

            if (capsule.direction == 1)
            { /* Y */
                direction = Vector3.up; scalerFactor = cTransform.lossyScale.y;
                radiusScaler = cTransform.lossyScale.x > cTransform.lossyScale.z ? cTransform.lossyScale.x : cTransform.lossyScale.z;
            }
            else if (capsule.direction == 0)
            { /* X */
                direction = Vector3.right; scalerFactor = cTransform.lossyScale.x;
                radiusScaler = cTransform.lossyScale.y > cTransform.lossyScale.z ? cTransform.lossyScale.y : cTransform.lossyScale.z;
            }
            else
            { /* Z */
                direction = Vector3.forward; scalerFactor = cTransform.lossyScale.z;
                radiusScaler = cTransform.lossyScale.y > cTransform.lossyScale.x ? cTransform.lossyScale.y : cTransform.lossyScale.x;
            }

            trueRadius = capsule.radius * radiusScaler;
        }

        private static float GetCapsule2DRadius(CapsuleCollider2D capsule)
        {
            if (capsule.direction == CapsuleDirection2D.Vertical)
                return capsule.size.x / 2f;
            else
                return capsule.size.y / 2f;
        }

        private static float GetCapsule2DHeight(CapsuleCollider2D capsule)
        {
            if (capsule.direction == CapsuleDirection2D.Vertical)
                return capsule.size.y / 2f;
            else
                return capsule.size.x / 2f;
        }

        protected static void CalculateCapsuleParameters(CapsuleCollider2D capsule, ref Vector3 direction, ref float trueRadius, ref float scalerFactor)
        {
            Transform cTransform = capsule.transform;

            float radiusScaler;

            if (capsule.direction == CapsuleDirection2D.Vertical)
            { /* Y */
                direction = Vector3.up; scalerFactor = cTransform.lossyScale.y;
                radiusScaler = cTransform.lossyScale.x > cTransform.lossyScale.z ? cTransform.lossyScale.x : cTransform.lossyScale.z;
                trueRadius = (capsule.size.x / 2f) * radiusScaler;
            }
            else if (capsule.direction == CapsuleDirection2D.Horizontal)
            { /* X */
                direction = Vector3.right; scalerFactor = cTransform.lossyScale.x;
                radiusScaler = cTransform.lossyScale.y > cTransform.lossyScale.z ? cTransform.lossyScale.y : cTransform.lossyScale.z;
                trueRadius = (capsule.size.y / 2f) * radiusScaler;
            }
        }

        protected static void GetCapsuleHeadsPositions(CapsuleCollider capsule, ref Vector3 upper, ref Vector3 bottom, Vector3 direction, float radius, float scalerFactor)
        {
            Vector3 upCapCenter = direction * ((capsule.height / 2) * scalerFactor - radius); // Local Space Position
            upper = capsule.transform.position + capsule.transform.TransformDirection(upCapCenter) + capsule.transform.TransformVector(capsule.center); // World Space

            Vector3 downCapCenter = -direction * ((capsule.height / 2) * scalerFactor - radius);
            bottom = capsule.transform.position + capsule.transform.TransformDirection(downCapCenter) + capsule.transform.TransformVector(capsule.center);
        }

        protected static void GetCapsuleHeadsPositions(CapsuleCollider2D capsule, ref Vector3 upper, ref Vector3 bottom, Vector3 direction, float radius, float scalerFactor)
        {
            Vector3 upCapCenter = direction * (GetCapsule2DHeight(capsule)  * scalerFactor - radius); // Local Space Position
            upper = capsule.transform.position + capsule.transform.TransformDirection(upCapCenter) + capsule.transform.TransformVector(capsule.offset); // World Space
            upper.z = 0f;

            Vector3 downCapCenter = -direction * (GetCapsule2DHeight(capsule)  * scalerFactor - radius);
            bottom = capsule.transform.position + capsule.transform.TransformDirection(downCapCenter) + capsule.transform.TransformVector(capsule.offset);
            bottom.z = 0f;
        }

        #endregion

    }
}
