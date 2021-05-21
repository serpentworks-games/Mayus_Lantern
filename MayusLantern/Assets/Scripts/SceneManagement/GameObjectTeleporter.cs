namespace ML.SceneManagement
{
    using UnityEngine;
    using ML.Player;

    /// <summary>
    ///  This class is used to move gameObjects from one position to another in the scene
    /// </summary>
    public class GameObjectTeleporter : MonoBehaviour
    {
        public static GameObjectTeleporter Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = FindObjectOfType<GameObjectTeleporter>();

                if (instance != null) return instance;

                GameObject gameObjectTeleporter = new GameObject("GameObjectTeleporter");
                instance = gameObjectTeleporter.AddComponent<GameObjectTeleporter>();

                return instance;
            }
        }

        public static bool Transitioning
        {
            get { return Instance.m_Transitioning; }
        }

        static GameObjectTeleporter instance;
        PlayerInput m_PlayerInput;
        bool m_Transitioning;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            m_PlayerInput = FindObjectOfType<PlayerInput>();
        }

        public static void Teleport(TransitionPoint transitionPoint)
        {
            Transform destinationTransform = Instance.GetDestination(transitionPoint.transitionDestinationTag).transform;

            Instance.StartCoroutine(Instance.Transition(transitionPoint.transitioningGameObject, true, destinationTransform.position, true));
        }
    }
}