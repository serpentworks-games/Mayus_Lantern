using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_NEW : MonoBehaviour
{

    [Header("State Behavior Variables")]
    public bool canAttack = false;
    public bool isSneaking = false;
    public bool hasControl = true;

    [Header("Public References")]
    public CameraSettings_NEW cameraSettings = null;
    public Transform objectHoldPoint;

    PlayerMovement playerMovement;

    Transform currentCheckPoint;
    bool isRespawning;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        if (!hasControl) return;

        if (canAttack && Input.GetButtonDown("Fire2"))
        {
            Attack();
        }
        else
        {
            Movement();
        }
    }

    public Transform GetObjectHoldPoint()
    {
        return objectHoldPoint;
    }

    void Movement()
    {
    }

    void Attack()
    {
        Debug.Log("WHACK!");
    }


}
