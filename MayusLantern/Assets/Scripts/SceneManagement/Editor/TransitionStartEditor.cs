namespace ML.SceneManagement
{
    using UnityEngine;
    using UnityEditor;
    using ML.InteractionSystem;

    [CustomEditor(typeof(TransitionPoint))]
    public class TransitionStartEditor : Editor
    {
        SerializedProperty transitioningGameObjectProp;
        SerializedProperty transitionTypeProp;
        SerializedProperty newSceneNameProp;
        SerializedProperty transitionDestinationTagProp;
        SerializedProperty destinationTransformProp;
        SerializedProperty transitionWhenProp;
        SerializedProperty requiresInventoryCheckProp;
        SerializedProperty inventoryControllerProp;
        SerializedProperty inventoryCheckProp;
        SerializedProperty inventoryItemsProp;
        SerializedProperty onHasItemProp;
        SerializedProperty onDoesNotHaveItemProp;

        GUIContent[] inventoryControllerItems = new GUIContent[0];

        private void OnEnable()
        {
            transitioningGameObjectProp = serializedObject.FindProperty("transitioningGameObject");
            transitionTypeProp = serializedObject.FindProperty("transitionType");
            newSceneNameProp = serializedObject.FindProperty("newSceneName");
            transitionDestinationTagProp = serializedObject.FindProperty("transitionDestinationTag");
            destinationTransformProp = serializedObject.FindProperty("destinationTransform");
            transitionWhenProp = serializedObject.FindProperty("transitionWhen");
            requiresInventoryCheckProp = serializedObject.FindProperty("requiresInventoryCheck");
            inventoryControllerProp = serializedObject.FindProperty("inventoryController");
            inventoryCheckProp = serializedObject.FindProperty("inventoryCheck");
            inventoryItemsProp = inventoryCheckProp.FindPropertyRelative("inventoryItems");
            onHasItemProp = inventoryCheckProp.FindPropertyRelative("OnHasItem");
            onDoesNotHaveItemProp = inventoryCheckProp.FindPropertyRelative("OnDoesNotHaveItem");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(transitioningGameObjectProp);

            EditorGUILayout.PropertyField(transitionTypeProp);

            EditorGUI.indentLevel++;

            if ((TransitionPoint.TransitionType)transitionTypeProp.enumValueIndex == TransitionPoint.TransitionType.SameScene)
            {
                EditorGUILayout.PropertyField(destinationTransformProp);
            }
            else
            {
                EditorGUILayout.PropertyField(newSceneNameProp);
                EditorGUILayout.PropertyField(transitionDestinationTagProp);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(transitionWhenProp);

            EditorGUILayout.PropertyField(requiresInventoryCheckProp);
            if (requiresInventoryCheckProp.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(inventoryControllerProp);
                if (EditorGUI.EndChangeCheck() || (inventoryControllerProp.objectReferenceValue != null && inventoryControllerItems.Length == 0))
                {
                    SetupInventoryItemGUI();
                }

                if (inventoryControllerProp.objectReferenceValue != null)
                {
                    InventoryController controller = inventoryControllerProp.objectReferenceValue as InventoryController;
                    inventoryItemsProp.arraySize = EditorGUILayout.IntField("Inventory Items", inventoryItemsProp.arraySize);
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < inventoryItemsProp.arraySize; i++)
                    {
                        SerializedProperty elementProp = inventoryItemsProp.GetArrayElementAtIndex(i);

                        int itemIndex = ItemIndexFromController(controller, elementProp.stringValue);
                        if (itemIndex == -1)
                        {
                            EditorGUILayout.LabelField("No items found in the controller!");
                        }
                        else if (itemIndex == -2)
                        {
                            elementProp.stringValue = inventoryControllerItems[0].text;
                        }
                        else if (itemIndex == -3)
                        {
                            Debug.LogWarning("Previously listed item to check not found, resetting to item index");
                            elementProp.stringValue = inventoryControllerItems[0].text;
                        }
                        else
                        {
                            itemIndex = EditorGUILayout.Popup(new GUIContent("Item " + i), itemIndex, inventoryControllerItems);
                            elementProp.stringValue = inventoryControllerItems[itemIndex].text;
                        }
                    }

                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(onHasItemProp);
                    EditorGUILayout.PropertyField(onDoesNotHaveItemProp);
                }
                else
                {
                    for (int i = 0; i < inventoryItemsProp.arraySize; i++)
                    {
                        SerializedProperty elementProp = inventoryItemsProp.GetArrayElementAtIndex(i);
                        elementProp.stringValue = "";
                    }
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SetupInventoryItemGUI()
        {
            if (inventoryControllerProp.objectReferenceValue == null) return;

            InventoryController inventoryController = inventoryControllerProp.objectReferenceValue as InventoryController;

            inventoryControllerItems = new GUIContent[inventoryController.inventoryEvents.Length];
            for (int i = 0; i < inventoryController.inventoryEvents.Length; i++)
            {
                inventoryControllerItems[i] = new GUIContent(inventoryController.inventoryEvents[i].key);
            }
        }

        int ItemIndexFromController(InventoryController controller, string itemName)
        {

            if (controller.inventoryEvents.Length == 0) return -1;

            if (string.IsNullOrEmpty(itemName)) return -2;

            for (int i = 0; i < controller.inventoryEvents.Length; i++)
            {
                if (controller.inventoryEvents[i].key == itemName) return i;
            }

            return -3;
        }
    }
}