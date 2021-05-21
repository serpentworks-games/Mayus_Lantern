namespace ML.SceneManagement
{
    using System;
    using System.Collections;
    using ML.Player;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// This class is used to transition between scenes. This includes
    /// triggering all things needed to happen on transition such as data persistence
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = FindObjectOfType<SceneController>();

                if (instance != null) return instance;

                Create();

                return instance;
            }
        }

        public static bool Transitioning
        {
            get { return Instance.m_Transitioning; }
        }

        static SceneController instance;

        public static SceneController Create()
        {
            GameObject sceneControllerGameObject = new GameObject("SceneController");
            instance = sceneControllerGameObject.AddComponent<SceneController>();

            return instance;
        }

        public SceneTransitionDestination initialSceneTransitionDestination;

        Scene m_CurrentZoneScene;
        SceneTransitionDestination.DestinationTag m_ZoneRestartDestinationTag;
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

            if (initialSceneTransitionDestination != null)
            {
                SetEnteringGameObjectLocation(initialSceneTransitionDestination);
                //ScreenFader.SetAlpha(1f);
                //StartCoroutine(ScreenFader.FadeSceneIn());
                initialSceneTransitionDestination.OnReachDestination.Invoke();
            }
            else
            {
                m_CurrentZoneScene = SceneManager.GetActiveScene();
                m_ZoneRestartDestinationTag = SceneTransitionDestination.DestinationTag.A;
            }
        }

        public static void RestartZone(bool resetHealth = true)
        {
            //TODO: Reset health on zone restart/load

            Instance.StartCoroutine(Instance.Transition(Instance.m_CurrentZoneScene.name, Instance.m_ZoneRestartDestinationTag));
        }

        public static void RestartZoneWithDelay(float delay, bool resetHealth = true)
        {
            Instance.StartCoroutine(CallWithDelay(delay, RestartZone, resetHealth));
        }

        public static void TransitionToScene(TransitionPoint transitionPoint)
        {
            Instance.StartCoroutine(Instance.Transition(transitionPoint.newSceneName, transitionPoint.transitionDestinationTag, transitionPoint.transitionType));
        }

        public static SceneTransitionDestination GetDestinationFromTag(SceneTransitionDestination.DestinationTag destinationTag)
        {
            return Instance.GetDestination(destinationTag);
        }

        IEnumerator Transition(string newSceneName, SceneTransitionDestination.DestinationTag destinationTag, TransitionPoint.TransitionType transitionType = TransitionPoint.TransitionType.DifferentZone)
        {
            m_Transitioning = true;

            PersistentDataManager.SaveAllData();

            if (m_PlayerInput == null) m_PlayerInput = FindObjectOfType<PlayerInput>();

            if (m_PlayerInput) m_PlayerInput.ReleaseControl();

            //yield return StartCoroutine(ScreenFader.FadeSceneOut(ScreenFader.FadeType.Loading));

            PersistentDataManager.ClearPersisters();

            yield return SceneManager.LoadSceneAsync(newSceneName);

            m_PlayerInput = FindObjectOfType<PlayerInput>();

            if (m_PlayerInput) m_PlayerInput.ReleaseControl();

            PersistentDataManager.LoadAllData();

            SceneTransitionDestination enterance = GetDestination(destinationTag);
            SetEnteringGameObjectLocation(enterance);
            SetupNewScene(transitionType, enterance);

            if (enterance != null) enterance.OnReachDestination.Invoke();

            //yield return StartCoroutine(ScreenFader.FadeSceneIn());

            if (m_PlayerInput) m_PlayerInput.GainControl();

            m_Transitioning = false;
        }

        SceneTransitionDestination GetDestination(SceneTransitionDestination.DestinationTag destinationTag)
        {
            SceneTransitionDestination[] enterances = FindObjectsOfType<SceneTransitionDestination>();

            for (int i = 0; i < enterances.Length; i++)
            {
                if (enterances[i].destinationTag == destinationTag) return enterances[i];
            }

            Debug.LogWarning("No enterance was found with the " + destinationTag + " tag.");
            return null;
        }

        void SetEnteringGameObjectLocation(SceneTransitionDestination enterance)
        {
            if (enterance == null)
            {
                Debug.LogWarning("Entering object's location has not been set.");
                return;
            }

            Transform enteranceLocation = enterance.transform;
            Transform enteringObject = enterance.transitioningGameObject.transform;

            enteringObject.position = enteranceLocation.position;
            enteringObject.rotation = enteranceLocation.rotation;
        }

        void SetupNewScene(TransitionPoint.TransitionType transitionType, SceneTransitionDestination enterance)
        {
            if (enterance == null)
            {
                Debug.LogWarning("Restart info has not been set.");
                return;
            }

            if (transitionType == TransitionPoint.TransitionType.DifferentZone) SetZoneStart(enterance);
        }

        void SetZoneStart(SceneTransitionDestination enterance)
        {
            m_CurrentZoneScene = enterance.gameObject.scene;
            m_ZoneRestartDestinationTag = enterance.destinationTag;
        }

        static IEnumerator CallWithDelay<T>(float delay, Action<T> call, T param)
        {
            yield return new WaitForSeconds(delay);
            call(param);
        }
    }
}