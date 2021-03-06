using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Make sure everything is framerate-independent (I'm looking at you, jet pack code)

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera = null;
    public float mouseSensitivity = 1f;
    [Range(0f, 0.5f)] public float moveSmoothTime = 0.1f;
    [Range(0f, 0.5f)] public float mouseSmoothTime = 0.03f;
    public float walkSpeed = 10f;
    public float gravity = -40f;
    public float jumpHeight = 3f;
    public float jetPackPower = 1f;
    public float jetPackFuelMax = 200f;
    public RectTransform jetPackBar = null;
    public float bulletSpeed = 50f;
    public Rigidbody bullet = null;
    public Transform muzzle = null;
    public float timeBetweenShooting = 0.25f;
    public float spread = 0.1f;

    private float cameraPitch = 0f;
    private float velocityY = 0f;
    private CharacterController controller = null;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 currentDirectionVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    private float jetPackFuel = 0f;
    private bool jetPackRanOut = false;
    private bool readyToShoot = true;
    private bool allowInvoke = true;

    void Start()
    {
        // get the character controller
        controller = GetComponent <CharacterController>();

        // turn off the cursor
        Cursor.lockState = CursorLockMode.Locked;

        jetPackFuel = jetPackFuelMax;
    }

    void Update()
    {
        UpdateMouseLook();

        UpdateMovement();

        UpdateShooting();

        if (Input.GetButtonDown("Cancel"))
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

            if (jetPackFuel < jetPackFuelMax)
            {
                // regenerate jet pack fuel
                jetPackFuel += 2f;
            }
        }

        if (Input.GetKey("left shift") && jetPackFuel <= 1)
        {
            jetPackRanOut = true;
        }
        if (Input.GetKeyUp("left shift"))
        {
            jetPackRanOut = false;
        }

        if (Input.GetKey("left shift") && jetPackFuel > 1 && !jetPackRanOut)
        {
            // add velocity upward for jet pack
            velocityY += jetPackPower;
            // clamp y velocity to reasonable speeds
            velocityY = Mathf.Clamp(velocityY, -30, 10);

            // use up jet pack fuel
            jetPackFuel -= 1f;
        }
        else
        {
            // gravity
            velocityY += gravity * Time.deltaTime;

            if (jetPackFuel < jetPackFuelMax)
            {
                // slowly regenerate fuel
                jetPackFuel += 0.25f;
            }
        }

        // update jet pack fuel indicator
        jetPackBar.sizeDelta = new Vector2(jetPackFuel, jetPackBar.sizeDelta.y);

        // calculate velocity
        Vector3 velocity = (transform.forward * currentDirection.y + transform.right * currentDirection.x) * walkSpeed + Vector3.up * velocityY;
        // move using the character controller
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateShooting()
    {
        if (Input.GetButton("Fire1") && readyToShoot)
        {
            readyToShoot = false;

            //Vector3 bulletPosition = transform.position;
            //bulletPosition.y += 0.5f;
            Vector3 bulletPosition = muzzle.position;

            // calculate direction to shoot bullet in
            Ray ray = playerCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f,  0.5f, 0f));
            RaycastHit hit;
            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(75);
            }
            Vector3 bulletDirection = targetPoint - bulletPosition;

            float spreadX = Random.Range(-spread, spread);
            float spreadY = Random.Range(-spread, spread);
            bulletDirection += new Vector3(spreadX, spreadY, 0f);

            // create bullet and rotate it
            Rigidbody bulletClone = (Rigidbody)Instantiate(bullet, bulletPosition, Quaternion.identity);
            bulletClone.transform.forward = bulletDirection.normalized;

            // add force to bullet
            bulletClone.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * bulletSpeed, ForceMode.Impulse);

            if (allowInvoke)
            {
                // wait until can shoot again
                Invoke("ResetShot", timeBetweenShooting);
                allowInvoke = false;
            }
        }
    }

    void ResetShot()
    {
        // can shoot again
        readyToShoot = true;
        allowInvoke = true;
    }
}
