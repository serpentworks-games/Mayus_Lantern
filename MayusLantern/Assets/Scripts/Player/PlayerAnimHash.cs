using UnityEngine;

namespace ML.Player
{
    public class PlayerAnimHash : MonoBehaviour
    {
        public readonly int m_HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        public readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        public readonly int m_HashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
        public readonly int m_HashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
        public readonly int m_HashGrounded = Animator.StringToHash("Grounded");
        public readonly int m_HashInputDetected = Animator.StringToHash("InputDetected");
        public readonly int m_HashMeleeAttack = Animator.StringToHash("MeleeAttack");
        public readonly int m_HashHurt = Animator.StringToHash("Hurt");
        public readonly int m_HashDeath = Animator.StringToHash("Death");
        public readonly int m_HashRespawn = Animator.StringToHash("Respawn");
        public readonly int m_HashHurtFromX = Animator.StringToHash("HurtFromX");
        public readonly int m_HashHurtFromY = Animator.StringToHash("HurtFromY");
        public readonly int m_HashStateTime = Animator.StringToHash("StateTime");
        public readonly int m_HashFootFall = Animator.StringToHash("FootFall");

        //States
        public readonly int m_HashLocomotion = Animator.StringToHash("Locomotion");
        public readonly int m_HashAirborne = Animator.StringToHash("Airbourne");
        public readonly int m_HashLanding = Animator.StringToHash("Landing"); //Also a paramn
        public readonly int m_HashMayuCombo1 = Animator.StringToHash("MayuCombo1");
        public readonly int m_HashMayuDeath = Animator.StringToHash("MayuDeath");

        //Tags
        public readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");
    }
}