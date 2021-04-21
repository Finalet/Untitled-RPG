using UnityEngine;

namespace FIMSpace
{
    public class FImp_ColliderData_Mesh : FImp_ColliderData_Base
    {
        public MeshCollider Mesh { get; private set; }
        public PolygonCollider2D Poly2D { get; private set; }
        private ContactFilter2D filter;

        public FImp_ColliderData_Mesh(MeshCollider collider)
        {
            Is2D = false;
            Transform = collider.transform;
            Collider = collider;
            Mesh = collider;
            ColliderType = EFColliderType.Mesh;
        }

        public FImp_ColliderData_Mesh(PolygonCollider2D collider)
        {
            Is2D = true;
            Transform = collider.transform;
            Poly2D = collider;
            Collider2D = collider;
            ColliderType = EFColliderType.Mesh;
            filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.useDepth = false;
            r = new RaycastHit2D[1];
        }

        private RaycastHit2D[] r;
        public override bool PushIfInside(ref Vector3 segmentPosition, float segmentRadius, Vector3 segmentOffset)
        {
            if (Is2D == false)
            {
                if (Mesh.convex)
                {
                    Vector3 closest;
                    Vector3 positionOffsetted = segmentPosition + segmentOffset;
                    float castMul = 1f;

                    closest = Physics.ClosestPoint(positionOffsetted, Mesh, Mesh.transform.position, Mesh.transform.rotation);
                    if (Vector3.Distance(closest, positionOffsetted) > segmentRadius * 1.01f) return false;

                    Vector2 dir = (closest - positionOffsetted);
                    RaycastHit meshHit;
                    Mesh.Raycast(new Ray(positionOffsetted, dir.normalized), out meshHit, segmentRadius * castMul);

                    if (meshHit.transform)
                    {
                        segmentPosition = meshHit.point + meshHit.normal * segmentRadius;
                        return true;
                    }
                }
                else
                {
                    Vector3 closest;
                    float plus = 0f;

                    Vector3 positionOffsetted = segmentPosition + segmentOffset;

                    closest = Mesh.ClosestPointOnBounds(positionOffsetted);
                    plus = (closest - Mesh.transform.position).magnitude;

                    bool inside = false;
                    float insideMul = 1f;

                    if (closest == positionOffsetted)
                    {
                        inside = true;
                        insideMul = 7f;
                        closest = Mesh.transform.position;
                    }

                    Vector3 targeting = closest - positionOffsetted;
                    Vector3 rayDirection = targeting.normalized;
                    Vector3 rayOrigin = positionOffsetted - rayDirection * (segmentRadius * 2f + Mesh.bounds.extents.magnitude);

                    float rayDistance = targeting.magnitude + segmentRadius * 2f + plus + Mesh.bounds.extents.magnitude;

                    if ((positionOffsetted - closest).magnitude < segmentRadius * insideMul)
                    {
                        Ray ray = new Ray(rayOrigin, rayDirection);

                        RaycastHit hit;
                        if (Mesh.Raycast(ray, out hit, rayDistance))
                        {
                            float hitToPointDist = (positionOffsetted - hit.point).magnitude;

                            if (hitToPointDist < segmentRadius * insideMul)
                            {

                                Vector3 toNormal = hit.point - positionOffsetted;
                                Vector3 pushNormal;

                                if (inside) pushNormal = toNormal + toNormal.normalized * segmentRadius; else pushNormal = toNormal - toNormal.normalized * segmentRadius;

                                float dot = Vector3.Dot((hit.point - positionOffsetted).normalized, rayDirection);
                                if (inside && dot > 0f) pushNormal = toNormal - toNormal.normalized * segmentRadius;

                                segmentPosition = segmentPosition + pushNormal;

                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                Vector2 positionOffsetted = segmentPosition + segmentOffset;
                Vector2 closest;

                if (Poly2D.OverlapPoint(positionOffsetted))
                {
                    // Collider inside polygon collider!
                    Vector3 indir = Poly2D.bounds.center - (Vector3)positionOffsetted; indir.z = 0f;
                    Ray r = new Ray(Poly2D.bounds.center - indir * Poly2D.bounds.max.magnitude, indir);
                    float dist = 0f;
                    Poly2D.bounds.IntersectRay(r, out dist); // We've got partially correct point
                    if (dist > 0f)
                        closest = Poly2D.ClosestPoint(r.GetPoint(dist));
                    else
                        closest = Poly2D.ClosestPoint(positionOffsetted);
                }
                else
                    closest = Poly2D.ClosestPoint(positionOffsetted);

                Vector2 dir = (closest - positionOffsetted).normalized;
                int hits = Physics2D.Raycast(positionOffsetted, dir, filter, r, segmentRadius);

                if (hits > 0)
                {
                    if (r[0].transform == Transform)
                    {
                        segmentPosition = closest + r[0].normal * segmentRadius;
                        return true;
                    }
                }
#else
                return false;
#endif
            }

            return false;
        }



        public static void PushOutFromMeshCollider(MeshCollider mesh, Collision collision, float segmentColliderRadius, ref Vector3 pos)
        {
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 pushNormal = collision.contacts[0].normal;

            RaycastHit info;
            // Doing cheap mesh raycast from outside to hit surface
            if (mesh.Raycast(new Ray(pos + pushNormal * segmentColliderRadius * 2f, -pushNormal), out info, segmentColliderRadius * 5))
            {
                pushNormal = info.point - pos;
                float pushMagn = pushNormal.sqrMagnitude;
                if (pushMagn > 0 && pushMagn < segmentColliderRadius * segmentColliderRadius) pos = info.point - pushNormal * (segmentColliderRadius / Mathf.Sqrt(pushMagn)) * 0.9f;
            }
            else
            {
                pushNormal = collisionPoint - pos;
                float pushMagn = pushNormal.sqrMagnitude;
                if (pushMagn > 0 && pushMagn < segmentColliderRadius * segmentColliderRadius) pos = collisionPoint - pushNormal * (segmentColliderRadius / Mathf.Sqrt(pushMagn)) * 0.9f;
            }
        }



        public static void PushOutFromMesh(MeshCollider mesh, Collision collision, float pointRadius, ref Vector3 point)
        {
            Vector3 closest;
            float plus = 0f;

            closest = mesh.ClosestPointOnBounds(point);
            plus = (closest - mesh.transform.position).magnitude;

            bool inside = false;
            float insideMul = 1f;

            if (closest == point)
            {
                inside = true;
                insideMul = 7f;
                closest = mesh.transform.position;
            }

            Vector3 targeting = closest - point;
            Vector3 rayDirection = targeting.normalized;
            Vector3 rayOrigin = point - rayDirection * (pointRadius * 2f + mesh.bounds.extents.magnitude);

            float rayDistance = targeting.magnitude + pointRadius * 2f + plus + mesh.bounds.extents.magnitude;

            if ((point - closest).magnitude < pointRadius * insideMul)
            {
                Vector3 collisionPoint;

                if (!inside)
                    collisionPoint = collision.contacts[0].point;
                else
                {
                    Ray ray = new Ray(rayOrigin, rayDirection);
                    RaycastHit hit;
                    if (mesh.Raycast(ray, out hit, rayDistance)) collisionPoint = hit.point; else collisionPoint = collision.contacts[0].point;
                }

                float hitToPointDist = (point - collisionPoint).magnitude;

                if (hitToPointDist < pointRadius * insideMul)
                {
                    Vector3 toNormal = collisionPoint - point;
                    Vector3 pushNormal;

                    if (inside) pushNormal = toNormal + toNormal.normalized * pointRadius; else pushNormal = toNormal - toNormal.normalized * pointRadius;

                    float dot = Vector3.Dot((collisionPoint - point).normalized, rayDirection);
                    if (inside && dot > 0f) pushNormal = toNormal - toNormal.normalized * pointRadius;

                    point = point + pushNormal;
                }
            }
        }

    }
}
