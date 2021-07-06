namespace ML.Core
{
    using UnityEngine;
    using ML.Characters.Player;

    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Checkpoint");
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController controller = other.GetComponent<PlayerController>();

            if (controller == null) return;

            controller.SetCheckpoint(this.transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue * 0.75f;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}