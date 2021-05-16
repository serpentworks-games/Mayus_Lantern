using UnityEngine;
using UnityEngine.Events;

namespace ML.InteractionSystem
{    
    [RequireComponent(typeof(Collider))]
    public class InteractOnCollision : MonoBehaviour {
        public LayerMask layerMask;
        public UnityEvent OnCollision;

        private void Reset() {
            layerMask = LayerMask.NameToLayer("Everything");
        }

        private void OnCollisionEnter(Collision other) {
            if(0 != (layerMask.value & 1 << other.transform.gameObject.layer)){
                OnCollision.Invoke();
            }
        }

        private void OnDrawGizmos() {
            Gizmos.DrawIcon(transform.position, "Interaction Trigger", false);
        }

        private void OnDrawGizmosSelected() {
            
        }
    }
}