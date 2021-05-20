namespace ML.Player
{
    using ML.Core;
    using System.Collections;
    using ML.DamageSystem;
    using ML.Effects;
    using ML.Helpers;
    using ML.MessageSystem;
    using UnityEngine;

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
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
        float m_AngleDiff;
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
            playerAnimHash = GetComponent<PlayerAnimHash>();

            //TODO: set weapon

            s_Instance = this;
        }

        private void OnEnable()
        {
            SceneLinkedSMB<PlayerController>.Initialise(m_Animator, this);

            m_Damageable = GetComponent<Damageable>();
            m_Damageable.onDamageMessageReceivers.Add(this);
            m_Damageable.isInvulnerable = true; //So Mayu doesn't take damage while the script is being enabled

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
            inputBlocked |= m_NextStateInfo.tagHash == playerAnimHash.m_HashBlockInput;
            m_Input.playerControllerInputBlocked = inputBlocked;
        }

        /// <summary>
        /// Handles movement on the XZ plane
        /// Calculated every physics step
        /// </summary>
        private void CalculateForwardMovement()
        {
            //Cache move input and cap its magnitude at 1
            Vector2 moveInput = m_Input.MoveInput;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            //Calculate the intented speed by input
            m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

            //Determine if the speed needs to change based on whether there's any move input at all
            float accel = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            //Adjust forward speed towards the desired speed
            m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, m_DesiredForwardSpeed, accel * Time.deltaTime);

            //Set the Animator float for speed
            m_Animator.SetFloat(playerAnimHash.m_HashForwardSpeed, m_ForwardSpeed);
        }

        /// <summary>
        /// Handles movement in the Y axis for jumps and falling
        /// Calculated every physics step
        /// </summary>
        private void CalculateVerticleMovement()
        {
            //If jump is not currently held and Mayu is on the ground, then she's ready to jump
            if (!m_Input.JumpInput && m_IsGrounded) m_ReadyToJump = true;

            //If Mayu is grounded she can jump
            if (m_IsGrounded)
            {
                //When Mayu is grounded, apply a slight negative speed so she 'sticks'
                m_VerticalSpeed = -gravity * k_StickingGravityProportion;

                //If jump is pressed, Mayu is ready to jump, and she's not attacking or interacting
                if (m_Input.JumpInput && m_ReadyToJump && !m_InAttack)
                {
                    //Set her vertical speed to the jumpSpeed and keep her from jumping again (prevents double jumps)
                    m_VerticalSpeed = jumpSpeed;
                    m_IsGrounded = false;
                    m_ReadyToJump = false;
                }
            }

            else
            {
                //If Mayu is already in the air, Jump is not held/pressed
                //and Mayu is traveling upwards
                if (!m_Input.JumpInput && m_VerticalSpeed > 0.0f)
                {
                    //Decrease Mayu's upward speed
                    //This is how jumps are longer when jump is held vs tapped
                    m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                //If the jump is approx at the peak, make its speed 0f
                if (Mathf.Approximately(m_VerticalSpeed, 0f))
                {
                    m_VerticalSpeed = 0f;
                }

                //If Mayu is airborne, apply gravity
                m_VerticalSpeed -= gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Sets the rotation Mayu is aming to have
        /// Called every physics step
        /// </summary>
        private void SetTargetRotation()
        {
            //Grab local input to player
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            //Flatten the camera forward direction
            Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            //Local target rotation
            Quaternion targetRotation;

            //If local movement direction is the opposite of the forward direction, 
            //then the target rotation should be towards the camera
            if (Mathf.Approximately(Vector3.Dot(localMoveDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                //Otherwise the rotation should be offset from the input from the camera's forward
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMoveDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            //The desired forward direction
            Vector3 resultingForward = targetRotation * Vector3.forward;

            //TODO: combat things

            //Get the difference between the current rotation and desired rotation of the player in radians
            float currentAngle = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            m_TargetRotation = targetRotation;
        }

        /// <summary>
        /// Determines whether Mayu can turn under player input
        /// Called each physics step
        /// </summary>
        private bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == playerAnimHash.m_HashLocomotion || m_NextStateInfo.shortNameHash == playerAnimHash.m_HashLocomotion;
            bool updateOrientationForAirborne = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == playerAnimHash.m_HashAirborne || m_NextStateInfo.shortNameHash == playerAnimHash.m_HashAirborne;
            bool updateOrientationForLanding = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == playerAnimHash.m_HashLanding || m_NextStateInfo.shortNameHash == playerAnimHash.m_HashLanding;

            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || !m_InAttack;
        }

        /// <summary>
        /// Updates Mayu's orientation if there is move input and she's in the correct animator state
        /// Called every physics step after SetTargetRotation
        /// </summary>
        private void UpdateOrientation()
        {
            m_Animator.SetFloat(playerAnimHash.m_HashAngleDeltaRad, m_AngleDiff * Mathf.Deg2Rad);

            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);

            float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);

            float actualTurnSpeed = m_IsGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;

            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, actualTurnSpeed * Time.deltaTime);

            transform.rotation = m_TargetRotation;
        }

        /// <summary>
        /// Checks what audio should be played based on footsteps or physics events (such as hurt or landing)
        /// </summary>
        private void PlayAudio()
        {
            //TODO: audio
        }

        /// <summary>
        /// Counts up to the point where Mayu will begin a random idle animation
        /// Called each physics step
        /// </summary>
        private void TimeOutToIdle()
        {
            bool inputDetected = IsMoveInput || m_Input.AttackInput || m_Input.JumpInput;
            if (m_IsGrounded && !inputDetected)
            {
                m_Idletimer += Time.deltaTime;
                if (m_Idletimer >= idleTimeout)
                {
                    m_Idletimer = 0f;
                    m_Animator.SetTrigger(playerAnimHash.m_HashTimeoutToIdle);
                }
            }
            else
            {
                m_Idletimer = 0f;
                m_Animator.ResetTrigger(playerAnimHash.m_HashTimeoutToIdle);
            }

            m_Animator.SetBool(playerAnimHash.m_HashInputDetected, inputDetected);
        }

        /// <summary>
        /// Overrides root motion data to the Animator component after FixedUpdate, provided it is set to Animate Physics
        /// </summary>
        private void OnAnimatorMove()
        {
            Vector3 movement;

            //If Mayu is grounded
            if (m_IsGrounded)
            {
                //Raycast into the ground
                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    //Rotate the root motion movement along the plane of the ground (no floating on slopes)
                    movement = Vector3.ProjectOnPlane(m_Animator.deltaPosition, hit.normal);

                    //Store the current walking surface so audio is played properly
                    Renderer groundRenderer = hit.collider.GetComponentInChildren<Renderer>();
                    m_CurrentWalkingSurface = groundRenderer ? groundRenderer.sharedMaterial : null;
                }
                else
                {
                    //If no ground is hit, just get the movement as root motion
                    //This should RARELY happen as the ray should always hit when Mayu is grounded
                    movement = m_Animator.deltaPosition;
                    m_CurrentWalkingSurface = null;
                    Debug.Log("No ground was hit by the ray! No walking surface can be found. Ray at: " + ray.origin);
                }
            }
            else
            {
                //If Mayu is not grounded, the movement is just the forward direction
                movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
            }

            //Rotate the character controller by the animation's root rotation
            m_CharCtrl.transform.rotation *= m_Animator.deltaRotation;

            //Add the movement with the vertical speed
            movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

            //Move the character controller
            m_CharCtrl.Move(movement);

            //After movement, store whether or not the controller is grounded
            m_IsGrounded = m_CharCtrl.isGrounded;

            //If Mayu is not on the ground, send the vertical speed to the animator
            //This is so the speed is kept when landing so the landing animation is correct
            if (!m_IsGrounded)
                m_Animator.SetFloat(playerAnimHash.m_HashAirborneVerticalSpeed, m_VerticalSpeed);

            //Pass whether Mayu is grounded or not to the animator
            m_Animator.SetBool(playerAnimHash.m_HashGrounded, m_IsGrounded);
        }

        //TODO: Attack anim things

        /// <summary>
        /// Called by Checkpoints to make sure Mayu respawns correctly
        /// </summary>
        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint != null) m_CurrentCheckpoint = checkpoint;
        }

        /// <summary>
        /// Usually called by a state machine behavior on the anim controller but can be called anywhere
        /// </summary>
        public void Respawn()
        {
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            //Wait for the animator to be transitioning from the Death State
            while (m_CurrentStateInfo.shortNameHash != playerAnimHash.m_HashMayuDeath || !m_IsAnimatorTransitioning)
            {
                yield return null;
            }

            //Wait for the screen to fade out
            //TODO: ScreenFader.cs

            //Enable spawning
            PlayerSpawnFX spawn = GetComponentInChildren<PlayerSpawnFX>();
            spawn.enabled = true;

            //If there is a checkpoint saved, move Mayu to it
            if (m_CurrentCheckpoint != null)
            {
                transform.position = m_CurrentCheckpoint.transform.position;
                transform.rotation = m_CurrentCheckpoint.transform.rotation;
            }
            else
            {
                Debug.LogError("There is no checkpoint set. There should always be a checkpoint set. Did you add one for the spawn location?");
            }

            //Set Respawn trigger
            m_Animator.SetTrigger(playerAnimHash.m_HashRespawn);

            //Start the respawn effect
            spawn.StartEffect();

            //Wait for screen to fade back in
            //TODO: ScreenFader.cs

            m_Damageable.ResetDamage();
        }

        /// <summary>
        /// Called by the SMB once respawning is finished
        /// </summary>
        public void RespawnFinished()
        {
            m_Respawning = false;

            m_Damageable.isInvulnerable = false;
        }

        /// <summary>
        /// Called when Mayu is hurt by the Damageable component
        /// </summary>
        public void OnReceiveMessage(MessageType type, object sender, object data)
        {
            Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;
            switch (type)
            {
                case MessageType.DAMAGED:
                    Damaged(damageData);
                    break;
                case MessageType.DEAD:
                    Die(damageData);
                    break;
            }
        }

        /// <summary>
        /// Called by OnReceiveMessage when Mayu is damaged
        /// </summary>
        void Damaged(Damageable.DamageMessage damageMessage)
        {
            //Set the Hurt param of the animator
            m_Animator.SetTrigger(playerAnimHash.m_HashHurt);

            //Find the direction the damage came from
            Vector3 forward = damageMessage.damageSource - transform.position;
            forward.y = 0f;

            Vector3 localHurt = transform.InverseTransformDirection(forward);

            //Set the HurtFromX and HurtFromY params based on the direction of the damage
            m_Animator.SetFloat(playerAnimHash.m_HashHurtFromX, localHurt.x);
            m_Animator.SetFloat(playerAnimHash.m_HashHurtFromY, localHurt.z);

            //TODO: Camera shake

            //TODO: Audio
        }

        /// <summary>
        /// Called by OnReceiveMessage when Mayu is killed, and by
        /// DeathVolumes
        /// </summary>
        public void Die(Damageable.DamageMessage damageMessage)
        {
            m_Animator.SetTrigger(playerAnimHash.m_HashDeath);
            m_ForwardSpeed = 0f;
            m_VerticalSpeed = 0f;
            m_Respawning = true;
            m_Damageable.isInvulnerable = true;
        }
    }
}