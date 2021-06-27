namespace ML.Characters.Player
{
    using ML.Core;
    using System.Collections;
    using ML.Core.DamageSystem;
    using ML.Effects;
    using ML.Core.MessageSystem;
    using UnityEngine;
    using ML.SceneManagement;

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IMessageReceiver
    {
        //Singleton instance
        static PlayerController s_Instance;
        public static PlayerController instance { get { return s_Instance; } }

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
        public CameraSettings cameraSettings = null;
        public Transform objectHoldPoint;

        PlayerInput playerInput;
        PlayerMovement playerMovement;
        PlayerAnimHash playerAnimHash;
        Animator anim;
        Damageable damageSystem;

        Transform currentCheckPoint;
        bool isRespawning;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            playerMovement = GetComponent<PlayerMovement>();
            playerInput = GetComponent<PlayerInput>();
            damageSystem = GetComponent<Damageable>();
        }

        private void Update()
        {
            if (anim.GetFloat("ForwardSpeed") <= 0.001)
            {
                playerMovement.inputDetected = false;
            }
            else if (anim.GetFloat("ForwardSpeed") > 0.001)
            {
                playerMovement.inputDetected = true;
            }

            playerMovement.TimeOutToIdle(idleTimeOut);
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


        /// <summary>
        /// Called by Checkpoints to make sure Mayu respawns correctly
        /// </summary>
        public void SetCheckpoint(Transform _checkpoint)
        {
            if (_checkpoint != null) currentCheckPoint = _checkpoint;
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
            while (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != playerAnimHash.m_HashMayuDeath || !anim.IsInTransition(0))
            {
                yield return null;
            }

            //Wait for the screen to fade out
            yield return StartCoroutine(ScreenFader.FadeSceneOut());
            while (ScreenFader.IsFading) yield return null;

            //Enable spawning
            PlayerSpawnFX spawn = GetComponentInChildren<PlayerSpawnFX>();
            spawn.enabled = true;

            //If there is a checkpoint saved, move Mayu to it
            if (currentCheckPoint != null)
            {
                transform.position = currentCheckPoint.transform.position;
                transform.rotation = currentCheckPoint.transform.rotation;
            }
            else
            {
                Debug.LogError("There is no checkpoint set. There should always be a checkpoint set. Did you add one for the spawn location?");
            }

            //Set Respawn trigger
            anim.SetTrigger(playerAnimHash.m_HashRespawn);

            //Start the respawn effect
            spawn.StartEffect();

            //Wait for screen to fade back in
            yield return StartCoroutine(ScreenFader.FadeSceneIn());

            damageSystem.ResetDamage();
        }

        /// <summary>
        /// Called by the SMB once respawning is finished
        /// </summary>
        public void RespawnFinished()
        {
            isRespawning = false;

            damageSystem.isInvulnerable = false;
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
            anim.SetTrigger(playerAnimHash.m_HashHurt);

            //Find the direction the damage came from
            Vector3 forward = damageMessage.damageSource - transform.position;
            forward.y = 0f;

            Vector3 localHurt = transform.InverseTransformDirection(forward);

            //Set the HurtFromX and HurtFromY params based on the direction of the damage
            anim.SetFloat(playerAnimHash.m_HashHurtFromX, localHurt.x);
            anim.SetFloat(playerAnimHash.m_HashHurtFromY, localHurt.z);

            //TODO: Camera shake

            //TODO: Audio
        }

        /// <summary>
        /// Called by OnReceiveMessage when Mayu is killed, and by
        /// DeathVolumes
        /// </summary>
        public void Die(Damageable.DamageMessage damageMessage)
        {
            anim.SetTrigger(playerAnimHash.m_HashDeath);
            forwardSpeed = 0f;
            isRespawning = true;
            damageSystem.isInvulnerable = true;
        }
    }
}