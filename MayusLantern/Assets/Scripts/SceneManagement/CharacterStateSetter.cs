namespace ML.SceneManagement
{
    using System;
    using UnityEngine;

    /// <summary>
    /// This class is used to put the player into a specific state, usually when entering a scene
    /// </summary>
    public class CharacterStateSetter : MonoBehaviour
    {
        [System.Serializable]
        public class ParamSetter
        {
            public enum ParamType
            {
                Bool, Float, Int, Trigger,
            }

            public string paramName;
            public ParamType paramType;
            public bool boolValue;
            public float floatValue;
            public int intValue;

            int m_Hash;

            private void Awake()
            {
                m_Hash = Animator.StringToHash(paramName);
            }

            public void SetParam(Animator animator)
            {
                switch (paramType)
                {
                    case ParamType.Bool:
                        animator.SetBool(m_Hash, boolValue);
                        break;
                    case ParamType.Float:
                        animator.SetFloat(m_Hash, floatValue);
                        break;
                    case ParamType.Int:
                        animator.SetInteger(m_Hash, intValue);
                        break;
                    case ParamType.Trigger:
                        animator.SetTrigger(m_Hash);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
            }
        }
    }


}