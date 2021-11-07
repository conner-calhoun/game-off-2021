using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public float speed = 1f;
    public float rotationSpeed = 5f;



    // External GameObjects
    GameObject player;

    // Children & Components
    Rigidbody body;
    Transform rayOrigin;
    Transform moveToPoint;
    Transform headTarget;

    ContactPoint[] cPoints;
    Vector3 groundNormal;
    Vector3 currentGravity;
    float reNormalTime = 5f;
    bool isGrounded = true;

    // RayCasting angle modifier
    float rayLength = 1.5f; // Turns out this matches the scale pretty closely
    float forwardRay = 2f; // points in front of ant
    float backwardRay = 3f; // points behind ant

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "AntIgnoreCollision")
        {
            Physics.IgnoreCollision(other.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }
    }

    void OnCollisionStay(Collision ourCollision)
    {
        isGrounded = CheckGrounded(ourCollision);
    }

    bool CheckGrounded(Collision newCol)
    {
        cPoints = new ContactPoint[newCol.contactCount];
        newCol.GetContacts(cPoints);
        foreach (ContactPoint cP in cPoints)
        {
            SetGroundNormal(cP.normal);
            return true;
        }
        return false;
    }

    void CheckCorners()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, -transform.up + (transform.forward / forwardRay), out hit))
        {
            // Handle Inner Corners
            if (hit.distance < rayLength)
            {
                // Debug.DrawLine(rayOrigin.position, hit.point, Color.yellow);
                SetGroundNormal(hit.normal);
            }
        }
        if (Physics.Raycast(rayOrigin.position, -transform.up - body.transform.forward / backwardRay, out hit))
        {
            // Handle Outer Corners
            // Debug.DrawLine(rayOrigin.position, hit.point, Color.red);
            SetGroundNormal(hit.normal);
        }
    }

    void DoGravity()
    {
        if (isGrounded)
        {
            body.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Force);
        }
        else
        {
            // If not on the ground, just fall like normal
            body.AddForce(-Vector3.up * Physics.gravity.magnitude, ForceMode.Force);
        }
    }

    void HandleAI()
    {
        if (player)
        {
            // Handle AI / Place moveToPoint on player // For now, set the moveToPoint location to the player's location
            moveToPoint.position = player.transform.position;
            headTarget.position = player.transform.position;
        }

        float step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, moveToPoint.position, step);

        Vector3 dir = (moveToPoint.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
    }

    void SetGroundNormal(Vector3 normal)
    {
        groundNormal = normal;
        Quaternion tilt = Quaternion.FromToRotation(body.transform.up, groundNormal);
        body.transform.rotation = Quaternion.Lerp(body.transform.rotation, tilt * body.transform.rotation, reNormalTime * Time.deltaTime);
    }

    void ManualInput()
    {
        // Movement will instead be a "MoveTo" point
        if (body.velocity.magnitude < speed)
        {
            float value = Input.GetAxis("Vertical");
            if (value > 0)
            {
                body.transform.Translate(Vector3.forward * value * Time.fixedDeltaTime * 2.5f);
            }
        }

        // Y Rotation
        body.transform.Rotate(new Vector3(0f, Input.GetAxis("Horizontal") * 150f * Time.fixedDeltaTime, 0f));
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        rayOrigin = gameObject.transform.Find("RayOrigin");

        moveToPoint = gameObject.transform.Find("MoveTarget");
        /// we need to detach the MoveTarget from the ant or it will move with the ant.
        moveToPoint.parent = null;

        headTarget = gameObject.transform.Find("BugRig/HeadAim/HeadLookAt");

        body.useGravity = false;

        rayLength = transform.localScale.x; // Set length of forward raycast to the scale of the ant

        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckCorners();
        DoGravity();
        HandleAI();
    }
}
