using UnityEngine;

namespace FIMSpace
{
    /// <summary> V1.3.5
    /// FM: Base class to hold calculations on colliders for fimpossible packages
    /// </summary>
    public abstract class FImp_ColliderData_Base
    {
        public Transform Transform { get; protected set; }
        public Collider Collider { get; protected set; }
        public Collider2D Collider2D { get; protected set; }
        public bool Is2D = false;

        public bool IsStatic { get; private set; }
        public enum EFColliderType { Box, Sphere, Capsule, Mesh, Terrain }
        public EFColliderType ColliderType { get; protected set; }

        protected Vector3 previousPosition = Vector3.zero;
        protected Quaternion previousRotation = Quaternion.identity;
        protected Vector3 previousScale = Vector3.one;

        /// <summary>
        /// Generating class for given collider
        /// </summary>
        public static FImp_ColliderData_Base GetColliderDataFor(Collider collider)
        {
            SphereCollider s = collider as SphereCollider;

            if (s)
                return new FImp_ColliderData_Sphere(s);
            else
            {
                CapsuleCollider c = collider as CapsuleCollider;
                if (c)
                    return new FImp_ColliderData_Capsule(c);
                else
                {
                    BoxCollider b = collider as BoxCollider;
                    if (b)
                        return new FImp_ColliderData_Box(b);
                    else
                    {
                        MeshCollider m = collider as MeshCollider;
                        if (m)
                            return new FImp_ColliderData_Mesh(m);
                        else
                        {
                            TerrainCollider t = collider as TerrainCollider;
                            if (t)
                                return new FImp_ColliderData_Terrain(t);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Generating class for given collider
        /// </summary>
        public static FImp_ColliderData_Base GetColliderDataFor(Collider2D collider)
        {
            CircleCollider2D s = collider as CircleCollider2D;

            if (s)
                return new FImp_ColliderData_Sphere(s);
            else
            {
                CapsuleCollider2D c = collider as CapsuleCollider2D;
                if (c)
                    return new FImp_ColliderData_Capsule(c);
                else
                {
                    BoxCollider2D b = collider as BoxCollider2D;
                    if (b)
                        return new FImp_ColliderData_Box(b);
                    else
                    {
                        PolygonCollider2D m = collider as PolygonCollider2D;
                        if (m)
                            return new FImp_ColliderData_Mesh(m);
                        //else
                        //{
                        //    EdgeCollider2D e = collider as EdgeCollider2D;
                        //    if (e)
                        //        return new FImp_ColliderData_Mesh(e);
                        //    else
                        //    {
                        //        TilemapCollider2D t = collider as TilemapCollider2D;
                        //        if (t)
                        //            return new FImp_ColliderData_Mesh(t);
                        //        else
                        //        {
                        //            CompositeCollider2D cps = collider as CompositeCollider2D;
                        //            if (cps)
                        //                return new FImp_ColliderData_Mesh(cps);
                        //        }
                        //    }
                        //}
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// When collider moves / rotates / scales this method should be called
        /// </summary>
        public virtual void RefreshColliderData()
        {
            if (Transform.gameObject.isStatic) IsStatic = true; else IsStatic = false;
        }


        /// <summary>
        /// Detecting if given point (sphere) is inside collider or colliding with (for mesh collider)
        /// and projecting it onto collider's surface
        /// </summary>
        /// <param name="point"> Position of colliding sphere which will be pushed out </param>
        /// <param name="pointRadius"> Radius of colliding sphere </param>
        /// <param name="pointOffset"> Offset in position of colliding sphere </param>
        /// <returns></returns>
        public virtual bool PushIfInside(ref Vector3 point, float pointRadius, Vector3 pointOffset)
        {
            if ( Collider as SphereCollider )
            Debug.Log("It shouldn't appear");
            return false;
        }

        /// <summary>
        /// If not implemented 3D algorithm will be applied
        /// </summary>
        public virtual bool PushIfInside2D(ref Vector3 point, float pointRadius, Vector3 pointOffset)
        {
            return PushIfInside(ref point, pointRadius, pointOffset);
        }


        public static bool VIsSame(Vector3 vec1, Vector3 vec2)
        {
            if (vec1.x != vec2.x) return false; if (vec1.y != vec2.y) return false; if (vec1.z != vec2.z) return false; return true;
        }
    }
}