namespace ML.SceneManagement
{
    using UnityEngine;
    using ML.Characters.Player;
    using System.Collections;

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

        protected IEnumerator Transition(GameObject transitionGameObject, bool releaseControl, Vector3 destination, bool fade)
        {
            m_Transitioning = true;

            if (releaseControl)
            {
                if (m_PlayerInput == null) m_PlayerInput = FindObjectOfType<PlayerInput>();
                m_PlayerInput.ReleaseControl();
            }

            if (fade) yield return StartCoroutine(ScreenFader.FadeSceneOut());

            transitionGameObject.transform.position = destination;

            if (fade) yield return StartCoroutine(ScreenFader.FadeSceneIn());

            if (releaseControl) m_PlayerInput.GainControl();

            m_Transitioning = false;
        }

        SceneTransitionDestination GetDestination(SceneTransitionDestination.DestinationTag destinationTag)
        {
            SceneTransitionDestination[] entrances = FindObjectsOfType<SceneTransitionDestination>();
            for (int i = 0; i < entrances.Length; i++)
            {
                if (entrances[i].destinationTag == destinationTag) return entrances[i];

            }

            Debug.LogWarning("No entrance was found with the " + destinationTag + " tag.");
            return null;
        }
    }
}