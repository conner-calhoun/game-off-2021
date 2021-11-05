using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public float speed = 1f;
    private Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (body.velocity.magnitude < speed)
        {
            float value = Input.GetAxis("Vertical");
            if (value != 0)
                body.AddForce(0, 0, value * Time.fixedDeltaTime * 1000f);
            value = Input.GetAxis("Horizontal");
            if (value != 0)
                body.AddForce(value * Time.fixedDeltaTime * 1000f, 0f, 0f);
        }
    }
}
