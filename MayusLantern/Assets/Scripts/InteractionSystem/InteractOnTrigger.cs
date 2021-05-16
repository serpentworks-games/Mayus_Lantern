namespace ML.InteractionSystem
{
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(Collider))]
    public class InteractOnTrigger : MonoBehaviour {
        public LayerMask layerMask;
        public UnityEvent OnEnter, OnExit;
        new Collider collider;
        public InventoryController.InventoryChecker[] inventoryChecks;

        private void Reset() {
            layerMask = LayerMask.NameToLayer("Everything");
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) {
            if(0 != (layerMask.value & 1 << other.gameObject.layer)){
                ExecuteOnEnter(other);
            }
        }

        protected virtual void ExecuteOnEnter(Collider other){
            OnEnter.Invoke();
            for (int i = 0; i < inventoryChecks.Length; i++)
            {
                inventoryChecks[i].CheckInventory(other.GetComponent<InventoryController>());
            }
        }

        private void OnTriggerExit(Collider other) {
            if(0 != (layerMask.value & 1 << other.gameObject.layer)){
                ExecuteOnExit(other);
            }
        }

        protected virtual void ExecuteOnExit(Collider other){
            OnExit.Invoke();
        }

        private void OnDrawGizmos() {
            Gizmos.DrawIcon(transform.position, "Interaction Trigger", false);

        }
    }

}