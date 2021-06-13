namespace ML.GameCommands
{
    using UnityEngine;

    public class SetAnimatorTrigger : GameCommandHandler
    {

        public Animator animator;
        public string triggerName;

        private void Reset()
        {
            animator = GetComponent<Animator>();
        }

        public override void PerformInteraction()
        {
            if (animator) animator.SetTrigger(triggerName);
        }
    }
}