using UnityEngine;

public class PlayerInput_NEW : MonoBehaviour
{
    public bool playerInputBlocked;
    Vector2 movement;
    bool jump, attack, pause, externalInputBlock;

    public Vector2 MoveInput
    {
        get
        {
            if (playerInputBlocked || externalInputBlock)
            {
                return Vector2.zero;
            }
            return movement;
        }
    }

    public bool JumpInput
    {
        get { return jump && !playerInputBlocked && !externalInputBlock; }
    }

    public bool AttackInput
    {
        get { return attack && !playerInputBlocked && !externalInputBlock; }
    }

    public bool Pause
    {
        get { return pause; }
    }

    private void Update()
    {
        movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        attack = Input.GetButtonDown("Fire1");
        jump = Input.GetButton("Jump");
        pause = Input.GetButtonDown("Pause");
    }

    public bool HaveControl()
    {
        return !externalInputBlock;
    }

    public void ReleaseControl()
    {
        externalInputBlock = true;
    }

    public void GainControl()
    {
        externalInputBlock = false;
    }
}