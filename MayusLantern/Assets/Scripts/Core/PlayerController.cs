namespace ML.Core
{
    using System;
    using ML.DamageSystem;
    using ML.Helpers;
    using UnityEngine;

    public class PlayerController : MonoBehaviour, IMessageReceiver
    {
        //Singleton instance
        static PlayerController s_Instance;
        public static PlayerController instance { get { return s_Instance; } }

        //Movement Variables
        public bool respawning { get { return m_Respawning; } }
        public float maxForwardSpeed = 8f;      //How fast Mayu moves
        public float gravity = 20f;             //How fast Mayu downwards if airborne
        public float jumpSpeed = 10f;           //How fast Mayu takes off when jumping
        public float minTurnSpeed = 400f;       //How fast Mayu can turn when at max speed
        public float maxTurnSpeed = 1200f;      //How fast Mayu turns when stationary
        public float idleTimeout = 5f;          //How long before Mayu starts doing random idles
        public bool canAttack;                  //Can Mayu swing the Lantern and attack ('stun') enemies?

        //References
        public CameraSettings cameraSettings;   //Used to determine camera's direction
        //TODO: Reference to the Lantern

        //Audio Clips
        //TODO: Add refeerences

        //Private Variables
        PlayerInput m_Input;
        CharacterController m_CharCtrl;
        Material m_CurrentWalkingSurface;       //For footstep sounds
        Quaternion m_TargetRotation;
        Collider[] m_OverlapResult = new Collider[8]; //Checks for colliders near Mayu
        Damageable m_Damageable;                //Used to set health, and invulnerability
        Renderer[] m_Renderers;                 //Used to make sure the renderers are reset properly
        Checkpoint m_CurrentCheckpoint;         //Used to reset Mayu's position on respawn/reload

        //Animator Variables
        Animator m_Animator;
        AnimatorStateInfo m_CurrentStateInfo;
        AnimatorStateInfo m_NextStateInfo;
        bool m_IsAnimatorTransitioning;
        AnimatorStateInfo m_PreviousCurrentStateInfo;
        AnimatorStateInfo m_PreviousNextStateInfo;
        bool m_PreviousIsAnimatorTransitioning;

        //Jumping Variables
        bool m_IsGrounded = true;
        bool m_PreviouslyGrounded = true;
        bool m_ReadyToJump;

        //Movement Variables
        float m_DesiredForwardSpeed;
        float m_ForwardSpeed;
        float m_VerticalSpeed;

        //States Variables
        bool m_Respawning;
        bool m_InAttack;
        float m_Idletimer;

        //Constants
        const float k_AirborneTurnSpeedProportion = 5.4f;
        const float k_GroundedRayDistance = 1f;
        const float k_JumpAbortSpeed = 10f;
        const float k_MinEnemyDotCoeff = 0.2f;
        const float k_InverseOneEighty = 1f / 180f;
        const float k_StickingGravityProportion = 0.3f;
        const float k_GroundAcceleration = 20f;
        const float k_GroundDeceleration = 25f;

        PlayerAnimHash playerAnimHash;

        /// <summary>
        /// Determines if there has been any move input from the player, so long as the value is higher than 0f
        /// </summary>
        bool IsMoveInput
        {
            get { return !Mathf.Approximately(m_Input.MoveInput.sqrMagnitude, 0f); }
        }

        /// <summary>
        /// Determines if Mayu can 'attack'
        /// </summary>
        /// <param name="canAttack">Can Mayu Attack?</param>
        public void SetCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }

        private void Reset()
        {

            //TODO: Reset weapon, reset audio sources

            cameraSettings = FindObjectOfType<CameraSettings>();
            if (cameraSettings != null)
            {
                if (cameraSettings.follow == null) cameraSettings.follow = transform;
                if (cameraSettings.lookAt == null) cameraSettings.lookAt = transform.Find("HeadTarget");
            }
        }

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_Animator = GetComponent<Animator>();
            m_CharCtrl = GetComponent<CharacterController>();

            //TODO: set weapon

            s_Instance = this;
        }

        private void OnEnable()
        {
            m_Damageable = GetComponent<Damageable>();
            m_Damageable.onDamageMessageReceivers.Add(this);
            m_Damageable.isInvulnerable = true; //So Mayu does take damage while spawning

            m_Renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnDisable()
        {
            m_Damageable.onDamageMessageReceivers.Remove(this);

            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_Renderers[i].enabled = true;
            }
        }

        private void FixedUpdate()
        {
            CacheAnimatorState();

            UpdateInputBlocking();

            //Equip Weapons

            m_Animator.SetFloat(playerAnimHash.m_HashStateTime, Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            m_Animator.ResetTrigger(playerAnimHash.m_HashMeleeAttack);

            if (m_Input.AttackInput && canAttack)
                m_Animator.SetTrigger(playerAnimHash.m_HashMeleeAttack);

            CalculateForwardMovement();
            CalculateVerticleMovement();

            SetTargetRotation();

            if (IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();

            PlayAudio();

            TimeOutToIdle();

            m_PreviouslyGrounded = m_IsGrounded;
        }

        /// <summary>
        /// Called at the start of LateUpdate to record the current state of the base layer of the animator
        /// </summary>
        private void CacheAnimatorState()
        {
            m_PreviousCurrentStateInfo = m_CurrentStateInfo;
            m_PreviousNextStateInfo = m_NextStateInfo;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = m_Animator.IsInTransition(0);
        }

        /// <summary>
        /// Called after animator state is cached to determine if we should block player input
        /// </summary>
        private void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash ==
                playerAnimHash.m_HashBlockInput && !m_IsAnimatorTransitioning;
        }

        private void TimeOutToIdle()
        {

        }

        private void PlayAudio()
        {

        }

        private void UpdateOrientation()
        {

        }

        private bool IsOrientationUpdated()
        {
            return true;
        }

        private void SetTargetRotation()
        {

        }

        private void CalculateVerticleMovement()
        {

        }

        private void CalculateForwardMovement()
        {

        }

        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {

        }

        internal void Die(Damageable.DamageMessage damageMessage)
        {

        }
    }
}