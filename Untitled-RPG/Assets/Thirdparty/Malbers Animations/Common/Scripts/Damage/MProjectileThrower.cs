using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Damage/Projectile Thrower")]

    public class MProjectileThrower : MonoBehaviour, IThrower
    {
        /// <summary> Is Used to calculate the Trajectory and Display it as a LineRenderer </summary>
        public System.Action<bool> Predict { get; set; }

        public bool FireOnStart;
        [SerializeField] private Transform m_Target;
        [SerializeField] private GameObject m_Owner;
        [SerializeField] private GameObjectReference m_Projectile = new GameObjectReference();
        public  Aim Aimer;

        [SerializeField, Tooltip("Launch force for the Projectile")]
        private float m_power = 50f;
        [Range(0, 90)]
        [SerializeField] private float m_angle = 45f;
        [SerializeField] private LayerReference hitLayer = new LayerReference(-1);


        public Vector3Reference gravity = new Vector3Reference(Physics.gravity);
        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }
        public LayerMask Layer { get => hitLayer.Value; set => hitLayer.Value = value; }

        public Vector3 AimPos => transform.position;


        public Transform Target { get => m_Target; set => m_Target = value; }

        /// <summary> Owner of the  </summary>
        public GameObject Owner { get => m_Owner; set => m_Owner = value; }
        public GameObject Projectile { get => m_Projectile.Value; set => m_Projectile.Value = value; }

        /// <summary> Projectile Launch Velocity(v3) </summary>
        public Vector3 Velocity { get; set; }

        public float Power { get => m_power; set => m_power = value; }


        private void Start()
        {
            if (Owner == null) Owner = transform.root.gameObject;

            if (FireOnStart) Fire();
        }

        /// <summary> Fire Proyectile </summary>
        public virtual void Fire()
        {
            if (Projectile == null) return;

            CalculateVelocity();
            var Inst_Projectile = Instantiate(Projectile, transform.position, Quaternion.identity);

            Prepare_Projectile(Inst_Projectile);

            Predict?.Invoke(false);
        }
            

        void Prepare_Projectile(GameObject p)
        {
            var projectile = p.GetComponent<MProjectile>();

            if (projectile != null) //Means its a Malbers Projectile ^^
            {
                projectile.Prepare(Owner, Gravity, Velocity,  Layer,  QueryTriggerInteraction.Ignore);
                projectile.Fire();
            }
            else //Fire without the Projectile Component
            {
                var rb = p.GetComponent<Rigidbody>();
                rb?.AddForce(Velocity, ForceMode.VelocityChange);
            }
        }


        public virtual void CalculateVelocity()
        {
            if (Target)
            {
                var target_Direction = (Target.position - transform.position).normalized;

                float TargetAngle = 90 - Vector3.Angle(target_Direction, -Gravity) + 0.1f; //in case the angle is smaller than the target height

                if (TargetAngle < m_angle)
                    TargetAngle = m_angle;

                Power = MTools.PowerFromAngle(AimPos, Target.position, TargetAngle);
                Velocity = MTools.VelocityFromPower(AimPos, Power, TargetAngle, Target.position);
            }
            else if (Aimer)
            {
                Velocity = Aimer.AimDirection.normalized * Power;
            }
            else
            {
                Velocity = transform.forward.normalized * Power;
            }

            Predict?.Invoke(true);
        }
    }
}