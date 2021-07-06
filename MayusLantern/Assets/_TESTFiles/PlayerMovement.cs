using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    public float floorOffsetY;
    public float floorRaycastLength = 1.6f;
    public float moveSpeed = 6f;
    public float rotateSpeed = 10f;
    public float slopeLimit = 45f;
    public float slopInfluence = 5f;
    public float jumpPower = 10f;

    Rigidbody rb;
    Animator anim;
    PlayerInput_NEW playerInput;
    float v, h, inputAmount;
    Vector3 moveDirection, raycastFloorPos, floorMovement, gravity, CombinedRaycast;

    float jumpFalloff = 2f;
    bool jumpPressed;
    float slopeAmount;
    Vector3 floorNormal;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput_NEW>();

    }

    public void HandleMovement()
    {
        //reset movement at the start
        moveDirection = Vector3.zero;

        //Get input
        v = playerInput.MoveInput.x;
        h = playerInput.MoveInput.y;

        jumpPressed = playerInput.JumpInput;

        //Add camera position to movement
        Vector3 correctedVert = v * Camera.main.transform.forward;
        Vector3 correctedHoriz = h * Camera.main.transform.right;

        //Combine the input 
        Vector3 combinedInput = correctedHoriz + correctedVert;

        //In order to keep diagonal movement to the speed, normalize the input
        //Also clear the Y so the character doesn't walk into the floor/sky depending on camera angle
        moveDirection = new Vector3(combinedInput.normalized.x, 0, combinedInput.normalized.z);

        //Lock the input so it's never negative or above 1
        float inputMag = Mathf.Abs(h) + Mathf.Abs(v);
        inputAmount = Mathf.Clamp01(inputMag);

        //Rotate the player to the move direction
        if (moveDirection.sqrMagnitude > 0.00001)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * inputAmount * rotateSpeed);
            transform.rotation = targetRot;
        }

        if (jumpPressed) Jump();

        //Blend tree things here
    }

    private void FixedUpdate()
    {
        //if not grounded, increase force down
        //if going down a slope, also apply to prevent bouncing down the slope
        if (!IsGrounded() || slopeAmount >= 0.1f)
        {
            gravity += Vector3.up * Physics.gravity.y * jumpFalloff * Time.fixedDeltaTime;
        }

        //Actual movement of the rb + down force
        rb.velocity = (moveDirection * GetMoveSpeed() * inputAmount) * gravity;

        //Find the Y position via raycasting
        floorMovement = new Vector3(rb.position.x, FindFloor().y + floorOffsetY, rb.position.z);

        //Only stick to the floor when grounded
        if (floorMovement != rb.position && IsGrounded() && rb.velocity.y <= 0)
        {
            //Move the rb to the floor
            rb.MovePosition(floorMovement);
            gravity.y = 0;
        }
    }

    /// <summary>
    /// Raycasts from the center of the character to check the floor position, 
    /// based on the average of 5 raycasts
    /// </summary>
    Vector3 FindFloor()
    {
        //Width of raycasts around the center of the character
        float raycastWidth = 0.25f;
        //Check floor on 5 raycasts, get the average when not Vector3.zero
        int floorAverage = 1;

        CombinedRaycast = FloorRaycasts(0, 0, floorRaycastLength);
        floorAverage += (GetFloorAverage(raycastWidth, 0) + GetFloorAverage(-raycastWidth, 0) + GetFloorAverage(0, raycastWidth) + GetFloorAverage(0, -raycastWidth));

        return CombinedRaycast / floorAverage;
    }

    /// <summary>
    /// As long as the average is not Vector3.zero, add to the average floor position
    /// </summary>
    int GetFloorAverage(float offsetX, float offsetZ, float raycastLength)
    {
        if (FloorRaycasts(offsetX, offsetZ, 1.6f) != Vector3.zero)
        {
            CombinedRaycast += FloorRaycasts(offsetX, offsetZ, floorRaycastLength);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    bool IsGrounded()
    {
        if (FloorRaycasts(0, 0, 0.6f) != Vector3.zero)
        {
            slopeAmount = Vector3.Dot(transform.forward, floorNormal);
            return true;
        }
        else
        {
            return false;
        }
    }




}