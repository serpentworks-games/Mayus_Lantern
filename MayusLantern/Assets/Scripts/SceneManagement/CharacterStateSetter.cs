namespace ML.SceneManagement
{
    using System;
    using System.Collections;
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

            int hash;

            public void Awake()
            {
                hash = Animator.StringToHash(paramName);
            }

            public void SetParam(Animator animator)
            {
                switch (paramType)
                {
                    case ParamType.Bool:
                        animator.SetBool(hash, boolValue);
                        break;
                    case ParamType.Float:
                        animator.SetFloat(hash, floatValue);
                        break;
                    case ParamType.Int:
                        animator.SetInteger(hash, intValue);
                        break;
                    case ParamType.Trigger:
                        animator.SetTrigger(hash);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
            }
        }

        public bool setCharacterVelocity;
        public Vector2 characterVelocity;

        public bool setCharacterFacing;
        public bool faceLeft;

        public Animator animator;

        public bool setState;
        public string animatorStateName;

        public bool setParams;
        public ParamSetter[] paramSetters;

        int hashStateName;
        Coroutine setCharacterStateCoroutine;

        private void Awake()
        {
            hashStateName = Animator.StringToHash(animatorStateName);

            for (int i = 0; i < paramSetters.Length; i++)
            {
                paramSetters[i].Awake();
            }
        }
        public void SetCharacterState()
        {
            if (setCharacterStateCoroutine != null) StopCoroutine(setCharacterStateCoroutine);

            if (setState) animator.Play(hashStateName);

            if (setParams)
            {
                for (int i = 0; i < paramSetters.Length; i++)
                {
                    paramSetters[i].SetParam(animator);
                }
            }
        }

        public void SetCharacterState(float delay)
        {
            if (setCharacterStateCoroutine != null) StopCoroutine(setCharacterStateCoroutine);

            setCharacterStateCoroutine = StartCoroutine(CallWithDelay(delay, SetCharacterState));
        }

        IEnumerator CallWithDelay(float delay, Action call)
        {
            yield return new WaitForSeconds(delay);
            call();
        }

    }


}