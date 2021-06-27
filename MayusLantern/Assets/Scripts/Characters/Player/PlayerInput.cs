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

        public bool playerControllerInputBlocked;

        Vector2 m_Movement;
        Vector2 m_Camera;
        bool m_Jump, m_Attack, m_Pause, m_ExternalInputBlocked;

        public Vector2 MoveInput
        {
            get
            {
                if (playerControllerInputBlocked || m_ExternalInputBlocked)
                    return Vector2.zero;
                return m_Movement;
            }
        }

        public Vector2 CameraInput
        {
            get
            {
                if (playerControllerInputBlocked || m_ExternalInputBlocked)
                    return Vector2.zero;
                return m_Camera;
            }
        }

        public bool JumpInput
        {
            get { return m_Jump && !playerControllerInputBlocked && !m_ExternalInputBlocked; }
        }

        public bool AttackInput
        {
            get { return m_Attack && !playerControllerInputBlocked && !m_ExternalInputBlocked; }
        }

        public bool Pause
        {
            get { return m_Pause; }
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
            m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            m_Camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            m_Jump = Input.GetButton("Jump");

            if (Input.GetButtonDown("Fire1"))
            {
                if (m_AttackWaitCoroutine != null)
                    StopCoroutine(m_AttackWaitCoroutine);

                m_AttackWaitCoroutine = StartCoroutine(AttackWait());
            }

            m_Pause = Input.GetButtonDown("Pause");
        }

        IEnumerator AttackWait()
        {
            m_Attack = true;
            yield return m_AttackInputWait;
            m_Attack = false;
        }

        public bool HaveControl()
        {
            return !m_ExternalInputBlocked;
        }

        public void ReleaseControl()
        {
            m_ExternalInputBlocked = true;
        }

        public void GainControl()
        {
            m_ExternalInputBlocked = false;
        }

    }
}