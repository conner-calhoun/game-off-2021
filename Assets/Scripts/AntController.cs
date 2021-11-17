using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public int health = 100;
    public float speed = 1f;
    public float rotationSpeed = 5f;
    public bool manualControls = false;
    public bool paralyzed = false;

    // External GameObjects
    GameObject player;

    // Children & Components
    Rigidbody body;
    Transform rayOrigin;
    Transform moveToPoint;
    Transform headTarget;
    Transform frontRayTarget;
    Transform backRayTarget;
    // Transform meshCollider;

    ContactPoint[] cPoints;
    Vector3 groundNormal;
    Vector3 currentGravity;
    float reNormalTime = 5f;
    bool isGrounded = false;
    bool isAnAbsoluteUnit = false;
    bool alive = true;

    // Length of front ray to use
    float rayLength = 1.5f; // Turns out this roughly matches the scale pretty closely

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
            Debug.Log("Tag: " + cP.thisCollider.tag);
            SetGroundNormal(cP.normal);
            return true;
        }
        return false;
    }

    void CheckCorners()
    {
        RaycastHit hit;
        Vector3 frontDir = frontRayTarget.position - rayOrigin.position;
        if (Physics.Raycast(rayOrigin.position, frontDir, out hit))
        {
            // Debug.DrawLine(rayOrigin.position, hit.point, Color.yellow);
            // Handle Inner Corners
            if (hit.distance < rayLength)
            {
                SetGroundNormal(hit.normal);
            }
        }
        Vector3 backDir = backRayTarget.position - rayOrigin.position;
        if (Physics.Raycast(rayOrigin.position, backDir, out hit))
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
        if (manualControls)
        {
            ManualInput();
            return;
        }

        // Giant Ants use different AI
        if (isAnAbsoluteUnit)
        {
            // Look at the player
            if (player)
            {
                headTarget.position = player.transform.position;
            }
            transform.position += new Vector3(0f, Mathf.Sin(Time.realtimeSinceStartup) * 0.04f, 0f);
        }
        else
        {
            if (player)
            {
                // Handle AI / Place moveToPoint on player // For now, set the moveToPoint location to the player's location
                moveToPoint.position = player.transform.position;
                headTarget.position = player.transform.position;
            }

            if (!paralyzed)
            {
                float step = speed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, moveToPoint.position, step);
            }

            Vector3 dir = (moveToPoint.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    void SetGroundNormal(Vector3 normal)
    {
        if (!isAnAbsoluteUnit)
        {
            groundNormal = normal;
            Quaternion tilt = Quaternion.FromToRotation(body.transform.up, groundNormal);
            body.transform.rotation = Quaternion.Lerp(body.transform.rotation, tilt * body.transform.rotation, reNormalTime * Time.deltaTime);
        }
    }

    void ManualInput()
    {
        // Movement will instead be a "MoveTo" point
        if (body.velocity.magnitude < speed)
        {
            float value = Input.GetAxis("Vertical");
            if (value > 0)
            {
                body.transform.Translate(Vector3.forward * value * Time.fixedDeltaTime * speed);
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
        frontRayTarget = gameObject.transform.Find("FrontRayTarget");
        backRayTarget = gameObject.transform.Find("BackRayTarget");

        moveToPoint = gameObject.transform.Find("MoveTarget");
        /// we need to detach the MoveTarget from the ant or it will move with the ant.
        moveToPoint.parent = null;

        headTarget = gameObject.transform.Find("BugRig/HeadAim/HeadLookAt");

        body.useGravity = false;

        rayLength = transform.localScale.x; // Set length of forward raycast to the scale of the ant

        player = GameObject.Find("Player");

        // If scale is very large, the ant is HUGE and needs to function differently
        isAnAbsoluteUnit = (transform.localScale.x > 40) ? true : false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (alive)
        {
            CheckCorners();
            DoGravity();
            HandleAI();
        }
        else
        {
            // Ragdoll on death, after a certain amount of time without movement, sleep the rigidbody
        }
    }
}
