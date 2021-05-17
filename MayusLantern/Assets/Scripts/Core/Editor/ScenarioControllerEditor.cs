namespace ML.Core
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(ScenarioController))]
    public class ScenarioControllerEditor : Editor
    {
        AddObjectiveSubEditor addObjectiveSubEditor = new AddObjectiveSubEditor();

        int showEvent = -1;

        private void OnEnable()
        {
            addObjectiveSubEditor.Init(this);
        }

        public override void OnInspectorGUI()
        {
            var sc = target as ScenarioController;
            var so = new SerializedObject(sc);
            so.Update();

            addObjectiveSubEditor.OnInspectorGUI(sc);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.Label("Objective");
                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Current", GUILayout.Width(64));
                }
                EditorGUILayout.LabelField("Required", GUILayout.Width(64));
                EditorGUILayout.LabelField("", GUILayout.Width(64));
            }

            var property = so.FindProperty("objectives");

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    var o = property.GetArrayElementAtIndex(i);
                    GUI.backgroundColor = showEvent == i ?
                    Color.green : Color.white;
                    using (new EditorGUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button(o.FindPropertyRelative("name").stringValue))
                        {
                            showEvent = i;
                        }

                        if (Application.isPlaying)
                        {
                            EditorGUILayout.PropertyField(o.FindPropertyRelative("currentCount"), GUIContent.none, GUILayout.Width(64));
                        }

                        EditorGUILayout.PropertyField(o.FindPropertyRelative("requiredCount"), GUIContent.none, GUILayout.Width(64));

                        if (GUILayout.Button("Remove", GUILayout.Width(64)))
                        {
                            property.DeleteArrayElementAtIndex(i);
                            so.ApplyModifiedProperties();
                        }
                    }

                    if (showEvent == i)
                    {
                        if (o.FindPropertyRelative("requiredCount").intValue > 1)
                        {
                            EditorGUILayout.PropertyField(o.FindPropertyRelative("OhProgress"));
                        }
                        EditorGUILayout.PropertyField(o.FindPropertyRelative("OnComplete"));
                    }

                    GUI.backgroundColor = Color.white;
                }
            }

            EditorGUILayout.PropertyField(so.FindProperty("OnAllObjectivesComplete"));
            so.ApplyModifiedProperties();

        }
    }
}