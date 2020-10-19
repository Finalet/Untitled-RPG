using UnityEngine;

public class AnyIKSampleThirdPersonCamera : MonoBehaviour
{
    // Rotation
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private GameObject lookAt;
    [SerializeField]
    float cameraMoveSpeed = 15;
    [SerializeField]
    float minCameraAngle = 10f;
    [SerializeField]
    float maxCameraAngle = 30f;
    [SerializeField]
    private GameObject pivotCamera;


    // Mouse wheel
    [SerializeField]
    float wheelSpeed = 5f;
    [SerializeField]
    float distance = 2.6f;
    [SerializeField]
    float minDistanceLimit = 0.6f;
    [SerializeField]
    float maxDistanceLimit = 10f;

    //offset
    [SerializeField]
    float horizontalOffset = 0f;
    [SerializeField]
    float verticalOffset = 0f;

    // Floor check
    [SerializeField]
    float floorCheckHeight = 0.5f;

    // fields
    private bool rightclicked = true;
    private bool mouseWheeled = true;
    private float x = 0f;
    private float y = 0f;

    void Start(){
        //SetCameraOffset(horizontalOffset, verticalOffset, distance);
        RotateByPositionAndLookAt();
    }

    void Update()
    {
        
    }
	
	void LateUpdate ()
	{
        if (Input.GetMouseButton(1))
            rightclicked = true;
        else
            rightclicked = false;
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            mouseWheeled = true;
        else
            mouseWheeled = false;

        RotateByPositionAndLookAt();
        SetCameraOffset(horizontalOffset, verticalOffset, distance);
    }

    void RotateByPositionAndLookAt()
    {
        if (target)
        {
            if (mouseWheeled)
            {
                distance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
                if (distance < minDistanceLimit)
                    distance = minDistanceLimit;
                if (distance > maxDistanceLimit)
                    distance = maxDistanceLimit;
            }

            bool floorCollisionDetected = false;
            if (distance > minDistanceLimit)
            {
                RaycastHit hit;
                if (Physics.Linecast(transform.position + Vector3.down * floorCheckHeight, target.transform.position, out hit))
                {
                    if (hit.collider.gameObject.layer == 8)
                    {
                        //Debug.Log("Floor collision detected!");
                        floorCollisionDetected = true;
                        /*
                        float diff = distance -hit.distance;
                        Debug.Log("hit.distance: " + hit.distance + " distance: " + distance + " diff: " + diff);
                        if (diff > 0.5)
                        {
                            distance -= hit.distance;
                            Debug.Log("new distance: " + distance);
                        }
                        */
                    }
                }
            }


            if (rightclicked)
            {
                y += Input.GetAxis("Mouse Y") * cameraMoveSpeed;
                x += Input.GetAxis("Mouse X") * cameraMoveSpeed;
                if (x > 180f)
                    x -= 360;
                else if (x <= -180f)
                    x += 360;
                if (y < (-1 * maxCameraAngle))
                    y = -1 * maxCameraAngle;
                else if (y > (-1 * minCameraAngle))
                    y = -1 * minCameraAngle;
            }
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, distance) + target.transform.position;
            if (floorCollisionDetected && transform.position.y > position.y)
            {
                position.y = transform.position.y;
            }
            transform.position = Vector3.Lerp(transform.position, position, cameraMoveSpeed * Time.deltaTime);
            transform.LookAt(lookAt.transform.position);
        }
    }

    void SetCameraOffset(float horizontalOffsetValue, float verticalOffsetValue, float currentDistance)
    {
        if (pivotCamera)
        {
            pivotCamera.transform.localPosition = Vector3.Lerp(pivotCamera.transform.localPosition, new Vector3(horizontalOffsetValue * currentDistance, verticalOffsetValue * currentDistance, 0), cameraMoveSpeed * Time.deltaTime);
        }
    }
}
