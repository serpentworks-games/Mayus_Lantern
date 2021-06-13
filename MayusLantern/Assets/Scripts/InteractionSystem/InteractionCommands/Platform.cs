namespace ML.GameCommands
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class Platform : MonoBehaviour
    {
        [Tooltip("Layer(s) to evaluate collider info with")]
        public LayerMask layerMask;
        new Collider collider;

        protected CharacterController m_CharacterController;

        const float k_SqrMaxCharacterMovement = 1f;

        private void Reset()
        {
            layerMask = LayerMask.NameToLayer("Everything");
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                CharacterController character = other.GetComponent<CharacterController>();

                if (character != null)
                {
                    m_CharacterController = character;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                if (m_CharacterController != null && other.gameObject == m_CharacterController.gameObject)
                {
                    m_CharacterController = null;
                }
            }
        }

        public void MoveCharacterController(Vector3 deltaPosition)
        {
            if (m_CharacterController != null && deltaPosition.sqrMagnitude < k_SqrMaxCharacterMovement)
            {
                m_CharacterController.Move(deltaPosition);
            }
        }
    }
}