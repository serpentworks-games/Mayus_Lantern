namespace ML.Characters.Player
{
    using UnityEngine;
    using System.Collections;

    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance
        {
            get { return s_Instance; }
        }

        protected static PlayerInput s_Instance;

        public bool playerInputBlocked;

        Vector2 movement;
        bool jump, attack, pause, interact, externalInputBlocked;

        public Vector2 MoveInput
        {
            get
            {
                if (playerInputBlocked || externalInputBlocked)
                    return Vector2.zero;
                return movement;
            }
        }

        public bool JumpInput
        {
            get { return jump && !playerInputBlocked && !externalInputBlocked; }
        }

        public bool AttackInput
        {
            get { return attack && !playerInputBlocked && !externalInputBlocked; }
        }

        public bool InteractInput
        {
            get { return interact && !playerInputBlocked && !externalInputBlocked; }
        }

        public bool Pause
        {
            get { return pause; }
        }

        WaitForSeconds m_AttackInputWait;
        Coroutine m_AttackWaitCoroutine;

        const float k_AttackInputDuration = 0.03f;

        private void Awake()
        {
            m_AttackInputWait = new WaitForSeconds(k_AttackInputDuration);

            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != null)
                throw new UnityException("There cannot be more than one PlayerInput script! The instances are " + s_Instance.name + " and " + name);
        }

        private void Update()
        {
            movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            jump = Input.GetButton("Jump");
            interact = Input.GetButtonDown("Interact");

            if (Input.GetButtonDown("Fire1"))
            {
                if (m_AttackWaitCoroutine != null)
                    StopCoroutine(m_AttackWaitCoroutine);

                m_AttackWaitCoroutine = StartCoroutine(AttackWait());
            }

            pause = Input.GetButtonDown("Pause");
        }

        IEnumerator AttackWait()
        {
            attack = true;
            yield return m_AttackInputWait;
            attack = false;
        }

        public bool HaveControl()
        {
            return !externalInputBlocked;
        }

        public void ReleaseControl()
        {
            externalInputBlocked = true;
        }

        public void GainControl()
        {
            externalInputBlocked = false;
        }

    }
}