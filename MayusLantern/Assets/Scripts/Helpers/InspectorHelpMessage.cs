namespace ML.Helpers
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// A small helper class that allows a message to be displayed
    /// in the inspector for documentation on a gameObject
    /// </summary>
    public class InspectorHelpMessage : MonoBehaviour
    {
        public string message;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InspectorHelpMessage))]
    public class InspectorHelpMessageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox((target as InspectorHelpMessage).message, MessageType.Info);

        }
    }
#endif
}