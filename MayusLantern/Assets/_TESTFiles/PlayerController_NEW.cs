using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_NEW : MonoBehaviour
{
    [Header("Movement Variables")]
    public float forwardSpeed = 5f;
    public float turnSpeed = 15f;
    public float speedDampTime = 1f;
    public float turnSmoothing = 15f;
    public float idleTimeOut = 2f;

    [Header("State Behavior Variables")]
    public bool canAttack = false;
    public bool isSneaking = false;

    [Header("Public References")]
    public CameraSettings_NEW cameraSettings = null;
    public Transform objectHoldPoint;

    PlayerMovement playerMovement;
    PlayerInput_NEW playerInput;
    Animator anim;

    Transform currentCheckPoint;
    bool isRespawning;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput_NEW>();
    }

    private void FixedUpdate()
    {
        if (!playerInput.HaveControl()) return;

        if (canAttack)
        {
            Attack();
        }
        else
        {
            Movement();
        }

        UpdateAnimator();
    }

    public Transform GetObjectHoldPoint()
    {
        return objectHoldPoint;
    }

    public void TakePlayerControl()
    {
        anim.SetFloat("ForwardSpeed", 0);
        playerInput.ReleaseControl();
    }

    public void GivePlayerControl()
    {
        playerInput.GainControl();
    }

    void Movement()
    {
        playerMovement.MovePlayer(forwardSpeed, speedDampTime, turnSpeed, turnSmoothing);
    }

    void Attack()
    {
        Debug.Log("WHACK!");
    }

    void UpdateAnimator()
    {

        playerMovement.UpdateAnimForwardSpeed(forwardSpeed, speedDampTime);
    }


}
