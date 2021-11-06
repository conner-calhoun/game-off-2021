using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera = null;
    public float mouseSensitivity = 1f;
    public float walkSpeed = 10f;
    public float gravity = -35f;
    public float jumpHeight = 3f;
    [Range(0f, 0.5f)] public float moveSmoothTime = 0.1f;
    [Range(0f, 0.5f)] public float mouseSmoothTime = 0.03f;

    private float cameraPitch = 0f;
    private float velocityY = 0f;
    private CharacterController controller = null;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 currentDirectionVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    void Start()
    {
        controller = GetComponent <CharacterController>();

        // turn off the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        UpdateMouseLook();

        UpdateMovement();

        if (Input.GetKeyDown("escape"))
        {
            // turn on the cursor
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void UpdateMouseLook()
    {
        // get the mouse input
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        // calculate pitch and clamp to straight up and down
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        // rotate camera up and down
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        // rotate player left and right
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }

    void UpdateMovement()
    {
        // get keyboard input, normalize, and smooth out
        Vector2 targetDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDirection.Normalize();
        currentDirection = Vector2.SmoothDamp(currentDirection, targetDirection, ref currentDirectionVelocity, moveSmoothTime);

        if(controller.isGrounded)
        {
            // reset y velocity
            velocityY = 0f;
            
            if (Input.GetButtonDown("Jump"))
            {
                // add velocity upward for jump
                velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // gravity
        velocityY += gravity * Time.deltaTime;

        // calculate velocity
        Vector3 velocity = (transform.forward * currentDirection.y + transform.right * currentDirection.x) * walkSpeed + Vector3.up * velocityY;
        // move using the character controller
        controller.Move(velocity * Time.deltaTime);
    }
}
