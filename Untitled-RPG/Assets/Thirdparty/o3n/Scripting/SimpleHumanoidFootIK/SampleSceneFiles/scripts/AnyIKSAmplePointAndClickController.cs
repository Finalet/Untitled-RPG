using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnyIKSAmplePointAndClickController : MonoBehaviour {

	public GameObject			particle;
    protected UnityEngine.AI.NavMeshAgent		agent;
	protected Animator			animator;
    protected AnyIKSampleLocomotion locomotion;
	protected Object particleClone;

    private int destroyCountDown = 25;

    private bool setDestinationInNextFrame = false;

    [SerializeField]
    float walkingSpeed = 1f;

    // Use this for initialization
    void Start () {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		agent.updateRotation = false;

		animator = GetComponent<Animator>();
		locomotion = new AnyIKSampleLocomotion(animator);

        particleClone = null;
    }

	protected void SetDestination()
	{
        destroyCountDown = 25;

        // Construct a ray from the current mouse coordinates
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit))
		{
            if (particleClone != null)
            {
                GameObject.Destroy(particleClone);
                particleClone = null;
            }
            // Create a particle if hit
            Quaternion q = new Quaternion();
            q.SetLookRotation(hit.normal * -1f, Vector3.forward);
            Vector3 markerPosition = hit.point + 1f * Vector3.up;
            particleClone = Instantiate(particle, markerPosition, q);
            agent.destination = hit.point;
        }
	}

    protected void SetupAgentLocomotion()
	{
        if (AgentDone() && locomotion != null)
		{
			locomotion.Do(0, 0);
            if (particleClone != null && destroyCountDown == 0)
            {
                GameObject.Destroy(particleClone);
                particleClone = null;
            }
        }
		else if (locomotion != null)
		{
            float speed = agent.desiredVelocity.magnitude;

			Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;

			float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

			locomotion.Do(speed, angle);
		}
	}

    void OnAnimatorMove()
    {
        if (Time.deltaTime != 0)
            agent.velocity = animator.deltaPosition / Time.deltaTime;
        else
            agent.velocity = Vector3.zero;

		transform.rotation = animator.rootRotation;
    }

	protected bool AgentDone()
	{
        return !agent.pathPending && AgentStopping();
	}

	protected bool AgentStopping()
	{
        return agent.remainingDistance <= agent.stoppingDistance;
	}

	// Update is called once per frame
	void Update () 
	{
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            locomotion = new AnyIKSampleLocomotion(animator);
        }
            

        if (destroyCountDown != 0)
            destroyCountDown--;

        if (setDestinationInNextFrame)
        {
            SetDestination();
            setDestinationInNextFrame = false;
        }

        if (Input.GetButtonDown ("Fire1"))
        {
            agent.speed = walkingSpeed;
            setDestinationInNextFrame = true;
        }

        SetupAgentLocomotion();
	}

}
