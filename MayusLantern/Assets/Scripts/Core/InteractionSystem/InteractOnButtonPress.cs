namespace ML.InteractionSystem
{
    using UnityEngine;
    using UnityEngine.Events;

    public class InteractOnButtonPress : InteractOnTrigger {
        public string buttonName = "E";
        public UnityEvent OnButtonPress;

        bool canExecuteButtons = false;

        protected override void ExecuteOnEnter(Collider other)
        {
            canExecuteButtons = true;
        }

        protected override void ExecuteOnExit(Collider other)
        {
           canExecuteButtons = false;
        }

        private void Update() {
            if(canExecuteButtons && Input.GetButtonDown(buttonName)){
                OnButtonPress.Invoke();
            }
        }
    }
}