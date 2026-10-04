using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public CharacterController controller;
    public GameObject cam;
    public Transform groundCheckPosition;
    public Transform topCheckPosition;
    public Animator anim;

    public GameObject head;
    public MeshRenderer headRender;

    [Header("Movement Magnitudes")]

    public float movementSpeed = 5f;

    public float lookSpeedH = 2.0f;
    public float lookSpeedV = 2.0f;

    public float jumpVelocity = 10f;
    public float gravity = 10f;
    //if less than one, gravity will be lower if the player holds down jump, to offer more vertical control while jumping, like in Mario
    public float holdJumpGravityMod = 0.75f;
    public float fixVelocity = 5f;
    public float bumpVelocity = 0f;

    public float groundCheckRadius = 0.45f;
    public float topCheckRadius = 0.45f;
    public LayerMask groundLayers;
    public bool grounded = true;

    public Vector2 verticalLookClamp;

    public bool onlyRawInputs = false;
    public bool onlySmoothInputs = false;

    [Header("Input Variables")]

    public float verticalVelocity = 0f;
    public Vector2 moveInputs;
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public Vector3 firstPersonCameraPos;
    public Vector3 thirdPersonCameraPos;
    public bool firstPerson = false;

    public bool canMove = true;
    public bool canGravity = true;
    public bool canLook = true;
    bool lockCursor = true;

    void Start()
    {
        if (!IsOwner) return;

        //Application.targetFrameRate = 10;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Camera.main.transform.parent = cam.transform;

        if (firstPerson)
        {
            Camera.main.transform.localPosition = firstPersonCameraPos;
            if (headRender != null)
            {
                headRender.enabled = false;
            }
        }
        else
        {
            Camera.main.transform.localPosition = thirdPersonCameraPos;
            if (headRender != null)
            {
                headRender.enabled = true;
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        //set first or third person perspective

        if (Input.GetKeyDown(KeyCode.P)) firstPerson = !firstPerson;

        if (firstPerson)
        {
            Camera.main.transform.localPosition = firstPersonCameraPos;
            if (headRender != null)
            {
                headRender.enabled = false;
            }
        }
        else
        {
            Camera.main.transform.localPosition = thirdPersonCameraPos;
            if (headRender != null)
            {
                headRender.enabled = true;
            }
        }

        // Debug stuff for showing and hiding cursor

        if (Input.GetKeyDown(KeyCode.Tab)) lockCursor = !lockCursor;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //Jumping, Gravity, and other Vertical Shenanigans
        grounded = Physics.CheckSphere(groundCheckPosition.position, groundCheckRadius, groundLayers);
        if (grounded && canMove)
        {
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpVelocity;
            }
            else if(verticalVelocity <= 0)
            {
                verticalVelocity = fixVelocity * -1f;
            }
        }
        else if(canGravity)
        {
            //adds gravity
            if (canMove && Input.GetButton("Jump") && verticalVelocity > 0)
            {
                //add gravity modifier when holding down space (BUT ONLY WHEN MOVING UPWARDS)
                verticalVelocity -= gravity * holdJumpGravityMod * Time.deltaTime;
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }
        }

        //falling animations
        if (grounded)
        {
            anim.SetBool("Grounded", true);
        }
        else
        {
            anim.SetBool("Grounded", false);
        }

        //for hitting ceilings
        if(Physics.CheckSphere(topCheckPosition.position, topCheckRadius, groundLayers) && verticalVelocity > 0)
        {
            verticalVelocity = bumpVelocity;
        }

        //WASD Movement
        if (canMove)
        {
            if(!onlySmoothInputs && (grounded || onlyRawInputs))
            {
                moveInputs.x = Input.GetAxisRaw("Horizontal");
                moveInputs.y = Input.GetAxisRaw("Vertical");
                moveInputs.Normalize();
            }
            else
            {
                moveInputs.x = Input.GetAxis("Horizontal");
                moveInputs.y = Input.GetAxis("Vertical");
                if(moveInputs.magnitude > 1)
                {
                    moveInputs.Normalize();
                }
            }

            //movement animations
            if(moveInputs.x != 0 || moveInputs.y != 0)
            {
                anim.SetBool("Moving", true);
            }
            else
            {
                anim.SetBool("Moving", false);
            }

            controller.Move(((((transform.right * moveInputs.x) + (transform.forward * moveInputs.y)) * movementSpeed + (transform.up * verticalVelocity)) * Time.deltaTime));
        }
        else{
            moveInputs = Vector2.zero;
            anim.SetBool("Moving", false);
        }

        //Camera, Mouse Controls that make me want to fucking kill myself
        if (canLook)
        {
            yaw += lookSpeedH * Input.GetAxisRaw("Mouse X");
            pitch -= lookSpeedV * Input.GetAxisRaw("Mouse Y");

            pitch = Mathf.Clamp(pitch, verticalLookClamp.x, verticalLookClamp.y);

            transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);
            cam.transform.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);
        }

        if(head != null)
        {
            head.transform.eulerAngles = cam.transform.eulerAngles;
        }
    }
}
