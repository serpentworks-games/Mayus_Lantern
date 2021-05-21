namespace ML.SceneManagement
{
    using ML.InteractionSystem;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class TransitionPoint : MonoBehaviour
    {
        public enum TransitionType
        {
            DifferentZone, DifferentNonGameplayScene, SameScene,
        }

        public enum TransitionWhen
        {
            ExternalCall, OnTriggerEnter,
        }

        public GameObject transitioningGameObject;
        public TransitionType transitionType;
        [SceneName] public string newSceneName;
        public SceneTransitionDestination.DestinationTag transitionDestinationTag;
        public TransitionPoint destinationTransform;
        public TransitionWhen transitionWhen;
        public bool requiresInventoryCheck;
        public InventoryController inventoryController;
        public InventoryController.InventoryChecker inventoryCheck;

        bool m_TransitioningGameObjectPresent;
    }
}