using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public float speed = 1f;
    public float forwardRay = 2f;
    public float backwardRay = 3f;

    Rigidbody body;
    Transform rayOrigin;
    ContactPoint[] cPoints;
    Vector3 groundNormal;
    Vector3 currentGravity;
    float reNormalTime = 5f;

    void OnCollisionStay(Collision ourCollision)
    {
        CheckGrounded(ourCollision);
    }

    void CheckGrounded(Collision newCol)
    {
        cPoints = new ContactPoint[newCol.contactCount];
        newCol.GetContacts(cPoints);
        foreach (ContactPoint cP in cPoints)
        {
            SetGroundNormal(cP.normal);
        }
    }

    void CheckCorners()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, -transform.up + (transform.forward / forwardRay), out hit))
        {
            // Handle Inner Corners
            if (hit.distance < 1.5)
            {
                // Debug.DrawLine(rayOrigin.position, hit.point, Color.yellow);
                SetGroundNormal(hit.normal);
            }
        }
        if (Physics.Raycast(rayOrigin.position, -transform.up - body.transform.forward / backwardRay, out hit))
        {
            // Handle Outer Corners
            if (hit.distance < 3)
            {
                // Debug.DrawLine(rayOrigin.position, hit.point, Color.red);
                SetGroundNormal(hit.normal);
            }

        }
    }

    void DoGravity()
    {
        body.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Force);
    }

    void ExecuteMove()
    {
        // Movement will instead be a "MoveTo" point
        if (body.velocity.magnitude < speed)
        {
            float value = Input.GetAxis("Vertical");
            if (value != 0)
            {
                body.transform.Translate(Vector3.forward * value * Time.fixedDeltaTime * 2.5f);
            }
        }

        // Y Rotation
        body.transform.Rotate(new Vector3(0f, Input.GetAxis("Horizontal") * 150f * Time.fixedDeltaTime, 0f));
    }

    void SetGroundNormal(Vector3 normal)
    {
        groundNormal = normal;
        Quaternion tilt = Quaternion.FromToRotation(body.transform.up, groundNormal);
        body.transform.rotation = Quaternion.Lerp(body.transform.rotation, tilt * body.transform.rotation, reNormalTime * Time.deltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        rayOrigin = gameObject.transform.Find("RayOrigin");
        body.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Handle custom gravity (sticking to walls)
        DoGravity();

        ExecuteMove();

        CheckCorners();
    }
}
