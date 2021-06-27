namespace ML.Characters.Player
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}