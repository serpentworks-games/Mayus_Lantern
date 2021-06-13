namespace ML.SceneManagement
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(TransitionPoint))]
    public class TransitionStartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}