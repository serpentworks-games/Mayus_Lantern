namespace ML.SceneManagement
{
    using System.Collections;
    using UnityEngine;

    public class ScreenFader : MonoBehaviour
    {
        public enum FadeType
        {
            Black, Loading, GameOver,
        }

        public static ScreenFader Instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;

                s_Instance = FindObjectOfType<ScreenFader>();

                if (s_Instance != null) return s_Instance;

                Create();

                return s_Instance;
            }
        }

        public static bool IsFading
        {
            get { return Instance.m_IsFading; }
        }

        protected static ScreenFader s_Instance;

        public static void Create()
        {
            ScreenFader controllerPrefab = Resources.Load<ScreenFader>("ScreenFader");
            s_Instance = Instantiate(controllerPrefab);
        }

        public CanvasGroup faderGroup;
        public CanvasGroup loadingGroup;
        public CanvasGroup gameOverGroup;
        public float fadeDuration = 1f;

        bool m_IsFading;

        const int k_MaxSortingLayer = 32767;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        protected IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
        {
            m_IsFading = true;
            canvasGroup.blocksRaycasts = true;
            float fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / fadeDuration;
            while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            canvasGroup.alpha = finalAlpha;
            m_IsFading = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void SetAlpha(float alpha)
        {
            Instance.faderGroup.alpha = alpha;
        }

        public static IEnumerator FadeSceneIn()
        {
            CanvasGroup canvasGroup;
            if (Instance.faderGroup.alpha > 0.1f) canvasGroup = Instance.faderGroup;
            else if (Instance.gameOverGroup.alpha > 0.1f) canvasGroup = Instance.gameOverGroup;
            else canvasGroup = Instance.loadingGroup;

            yield return Instance.StartCoroutine(Instance.Fade(0f, canvasGroup));

            canvasGroup.gameObject.SetActive(false);
        }

        public static IEnumerator FadeSceneOut(FadeType fadeType = FadeType.Black)
        {
            CanvasGroup canvasGroup;
            switch (fadeType)
            {
                case FadeType.Black:
                    canvasGroup = Instance.faderGroup;
                    break;
                case FadeType.GameOver:
                    canvasGroup = Instance.gameOverGroup;
                    break;
                default:
                    canvasGroup = Instance.loadingGroup;
                    break;
            }

            canvasGroup.gameObject.SetActive(true);

            yield return Instance.StartCoroutine(Instance.Fade(1f, canvasGroup));
        }
    }
}