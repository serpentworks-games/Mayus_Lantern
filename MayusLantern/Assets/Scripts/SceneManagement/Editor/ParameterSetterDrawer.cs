namespace ML.SceneManagement
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Animations;
    using System;

    [CustomPropertyDrawer(typeof(CharacterStateSetter.ParamSetter))]
    public class ParameterSetterDrawer : PropertyDrawer
    {

        SerializedProperty animProp;
        SerializedProperty paramNameProp;
        SerializedProperty paramTypeProp;
        SerializedProperty boolValueProp;
        SerializedProperty floatValueProp;
        SerializedProperty intValueProp;

        bool setupCalled;
        string[] paramNames;
        CharacterStateSetter.ParamSetter.ParamType[] paramTypes;
        int paramNameIndex;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (animProp == null) return 0f;

            if (animProp.objectReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 3f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!setupCalled || paramNames == null) ParamSetterSetup(property);

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, animProp);

            if (animProp.objectReferenceValue == null) return;

            position.y += position.height;
            paramNameIndex = EditorGUI.Popup(position, paramNameIndex, paramNames);
            paramNameProp.stringValue = paramNames[paramNameIndex];
            paramTypeProp.enumValueIndex = (int)paramTypes[paramNameIndex];

            position.y += position.height;

            switch ((CharacterStateSetter.ParamSetter.ParamType)paramTypeProp.enumValueIndex)
            {
                case CharacterStateSetter.ParamSetter.ParamType.Bool:
                    EditorGUI.PropertyField(position, boolValueProp);
                    break;
                case CharacterStateSetter.ParamSetter.ParamType.Float:
                    EditorGUI.PropertyField(position, floatValueProp);
                    break;
                case CharacterStateSetter.ParamSetter.ParamType.Int:
                    EditorGUI.PropertyField(position, intValueProp);
                    break;
            }
        }

        void ParamSetterSetup(SerializedProperty property)
        {
            setupCalled = true;

            animProp = property.FindPropertyRelative("animator");
            paramNameProp = property.FindPropertyRelative("paramName");
            paramTypeProp = property.FindPropertyRelative("paramType");
            boolValueProp = property.FindPropertyRelative("boolValue");
            floatValueProp = property.FindPropertyRelative("floatValue");
            intValueProp = property.FindPropertyRelative("intValue");

            if (animProp.objectReferenceValue == null)
            {
                paramNames = null;
                return;
            }

            Animator animator = animProp.objectReferenceValue as Animator;

            if (animator.runtimeAnimatorController == null)
            {
                paramNames = null;
                return;
            }

            AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;

            AnimatorControllerParameter[] parameters = animatorController.parameters;

            paramNames = new string[parameters.Length];
            paramTypes = new CharacterStateSetter.ParamSetter.ParamType[parameters.Length];

            for (int i = 0; i < paramNames.Length; i++)
            {
                paramNames[i] = parameters[i].name;

                switch (parameters[i].type)
                {
                    case AnimatorControllerParameterType.Float:
                        paramTypes[i] = CharacterStateSetter.ParamSetter.ParamType.Float;
                        break;
                    case AnimatorControllerParameterType.Int:
                        paramTypes[i] = CharacterStateSetter.ParamSetter.ParamType.Int;
                        break;
                    case AnimatorControllerParameterType.Bool:
                        paramTypes[i] = CharacterStateSetter.ParamSetter.ParamType.Bool;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        paramTypes[i] = CharacterStateSetter.ParamSetter.ParamType.Trigger;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (int i = 0; i < paramNames.Length; i++)
            {
                if (paramNames[i] == paramNameProp.stringValue)
                {
                    paramNameIndex = i;
                    paramTypeProp.enumValueIndex = (int)paramTypes[i];
                }
            }

        }
    }
}