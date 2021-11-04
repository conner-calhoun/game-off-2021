using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 1.5f;

    private GameObject player;
    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        player = this.transform.parent.gameObject;

        // turn off the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // get mouse input
        yaw += Input.GetAxis("Mouse X") * 100 * sensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * 100 * sensitivity * Time.deltaTime;

        // clamp the pitch to straight up and down
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // rotate the camera
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        // rotate the player
        player.transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        if (Input.GetKeyDown("escape"))
        {
            // turn on the cursor
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
