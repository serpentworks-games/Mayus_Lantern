namespace ML.Characters.Player
{
    using UnityEngine;

    public class PlayerMovement : MonoBehaviour
    {
        bool sneak;
        [HideInInspector] public bool inputDetected;
        bool externalInputDetected;

        Vector3 movement;

        float desiredForwardSpeed, currentForwardSpeed, verticalSpeed;
        float accel = 20f;
        float gravity = 100f;
        float stickyGravityProportion = 0.3f;

        CharacterController player;
        PlayerInput playerInput;
        Animator anim;

        protected float idleTimer;

        private void Awake()
        {
            player = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();
            playerInput = GetComponent<PlayerInput>();
        }

        /// <summary>
        /// Handles moving the player based on input
        /// </summary>
        public void MovePlayer(float forwardSpeed, float speedDampTime, float turnSpeed, float turnSmoothing)
        {
            if (player.isGrounded)
            {
                float hMovement = playerInput.MoveInput.x;
                float vMovement = playerInput.MoveInput.y;

                Vector3 correctedVertical = vMovement * Camera.main.transform.forward;
                Vector3 correctedHorizontal = hMovement * Camera.main.transform.right;

                Vector3 combinedInput = correctedHorizontal + correctedVertical;

                movement = new Vector3(combinedInput.normalized.x, 0f, combinedInput.normalized.z);

                if (movement != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement),
                        turnSpeed * Time.deltaTime);
                }
                movement *= forwardSpeed;
            }
            movement.y -= gravity * Time.deltaTime;

            player.Move(movement * Time.deltaTime);

            CalculateForwardSpeed(forwardSpeed);
        }

        /// <summary>
        /// Handles triggering random idles when no input is detected
        /// </summary>
        public void TimeOutToIdle(float idleTimeOut)
        {
            //If no input is detected...
            if (!inputDetected)
            {
                idleTimer += Time.deltaTime;

                // if the idleTimer is greater than or equal to the idleTimeOut
                // reset the timer
                if (idleTimer >= idleTimeOut)
                {
                    idleTimer = 0f;
                    anim.SetTrigger("TimeOutToIdle");
                }
            }
            else
            { //otherwise, reset the trigger and timer
                idleTimer = 0f;
                anim.ResetTrigger("TimeOutToIdle");
            }

            anim.SetBool("InputDetected", inputDetected);
        }

        /// <summary>
        /// Updates the animator based on speed
        /// </summary>
        public void UpdateAnimForwardSpeed(float forwardSpeed, float speedDampTime)
        {
            anim.SetFloat("ForwardSpeed", currentForwardSpeed, speedDampTime, Time.deltaTime);
        }

        /// <summary>
        /// Calculates the forward speed for the animator ;
        /// </summary>
        void CalculateForwardSpeed(float forwardSpeed)
        {
            Vector2 moveInput = playerInput.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            desiredForwardSpeed = moveInput.magnitude * forwardSpeed;
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, desiredForwardSpeed, accel * Time.deltaTime);
        }
    }

}