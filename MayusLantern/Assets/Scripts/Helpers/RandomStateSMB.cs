namespace ML.Helpers
{
    using UnityEngine;

    public class RandomStateSMB : StateMachineBehaviour
    {
        [Tooltip("How many idle states that can be randomized betweent")]
        public int numberOfStates = 3;
        [Tooltip("Minimum amount of normalized time for the state")]
        public float minNormTime = 0f;
        [Tooltip("Maximum amount of normalized time for the idle state")]
        public float maxNormTime = 5f;

        protected float m_RandomNormTime;

        readonly int m_HashRandomIdle = Animator.StringToHash("RandomIdle");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Randomly decide a time at which to transition
            m_RandomNormTime = Random.Range(minNormTime, maxNormTime);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {

            //If transitioning away from this state, reset the random idle param to -1
            if (animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animatorStateInfo.fullPathHash)
            {
                animator.SetInteger(m_HashRandomIdle, -1);
            }

            //IF the state is beyond the randomly decided normalized time and not yet
            //transitioning then set a random idle
            if (animatorStateInfo.normalizedTime > m_RandomNormTime && !animator.IsInTransition(0))
            {
                animator.SetInteger(m_HashRandomIdle, Random.Range(0, numberOfStates));
            }
        }
    }
}