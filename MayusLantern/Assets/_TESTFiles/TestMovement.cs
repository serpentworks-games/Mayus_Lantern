using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public float floorOffsetY;
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float slopeLimit = 45f;
    public float slopeInfluence = 5f;
    public float jumpPower = 10f;
    public float raycastLength = 1.6f;

    Rigidbody rb;
    Animator anim;
    float v, h;
    Vector3 moveDirection;
    float inputAmount;

    Vector3 raycastFloorPos;
    Vector3 floorMovement;
    Vector3 gravity;
    Vector3 combinedRaycast;

    float jumpFalloff = 2f;
    bool jumpInputPressed;
    float slopeAmount;
    Vector3 floorNormal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCharacter();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    void MoveCharacter()
    {
        moveDirection = Vector3.zero;

        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");

        jumpInputPressed = Input.GetKeyDown(KeyCode.Space);

        Vector3 correctedVertical = v * Camera.main.transform.forward;
        Vector3 correctedHorizontal = h * Camera.main.transform.right;

        Vector3 combinedInput = correctedHorizontal + correctedVertical;

        moveDirection = new Vector3(combinedInput.normalized.x, 0, combinedInput.normalized.z);

        float inputMag = Mathf.Abs(h) + Mathf.Abs(v);
        inputAmount = Mathf.Clamp01(inputMag);

        Quaternion rot = Quaternion.LookRotation(moveDirection);
        Quaternion targetRot = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * inputAmount * rotationSpeed);
        transform.rotation = targetRot;

        if (jumpInputPressed) Jump();

        anim.SetFloat("ForwardSpeed", inputAmount, 0.2f, Time.deltaTime);
        anim.SetFloat("SlopeNormal", slopeAmount, 0.2f, Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (!IsGrounded() || slopeAmount >= 0.1f)
        {
            gravity += Vector3.up * Physics.gravity.y * jumpFalloff * Time.fixedDeltaTime;
        }

        rb.velocity = (moveDirection * GetMoveSpeed() * inputAmount) + gravity;

        floorMovement = new Vector3(rb.position.x, FindFloor().y + floorOffsetY, rb.position.z);

        if (floorMovement != rb.position && IsGrounded() && rb.velocity.y <= 0)
        {
            rb.MovePosition(floorMovement);
            gravity.y = 0;
        }
    }

    Vector3 FindFloor()
    {
        float raycastWidth = 0.25f;
        int floorAverage = 1;

        combinedRaycast = FloorRaycasts(0, 0, raycastLength);
        floorAverage += (GetFloorAverage(raycastWidth, 0) + GetFloorAverage(-raycastWidth, 0) + GetFloorAverage(0, raycastWidth) + GetFloorAverage(0, -raycastWidth));

        return combinedRaycast / floorAverage;
    }

    int GetFloorAverage(float offsetX, float offsetZ)
    {
        if (FloorRaycasts(offsetX, offsetZ, raycastLength) != Vector3.zero)
        {
            combinedRaycast += FloorRaycasts(offsetX, offsetZ, raycastLength);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    Vector3 FloorRaycasts(float offsetX, float offsetZ, float _raycastLength)
    {
        RaycastHit hit;

        raycastFloorPos = transform.TransformPoint(0 + offsetX, 0 + 0.5f, 0 + offsetZ);

        Debug.DrawRay(raycastFloorPos, Vector3.down, Color.magenta);

        if (Physics.Raycast(raycastFloorPos, -Vector3.up, out hit, _raycastLength))
        {
            floorNormal = hit.normal;
            if (Vector3.Angle(floorNormal, Vector3.up) < slopeLimit)
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    bool IsGrounded()
    {
        if (FloorRaycasts(0, 0, raycastLength) != Vector3.zero)
        {
            slopeAmount = Vector3.Dot(transform.forward, floorNormal);
            return true;
        }
        else
        {
            return false;
        }
    }

    float GetMoveSpeed()
    {
        //Can add extras for running, crouching, etc
        float currentSpeed = Mathf.Clamp(moveSpeed + (slopeAmount * slopeInfluence), 0, moveSpeed + 1);
        return currentSpeed;

    }

    void Jump()
    {
        if (IsGrounded())
        {
            gravity.y = jumpPower;
            anim.SetTrigger("Jumping");
        }
    }
}
