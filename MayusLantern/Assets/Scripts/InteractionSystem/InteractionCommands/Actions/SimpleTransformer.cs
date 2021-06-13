namespace ML.GameCommands
{
    using UnityEngine;

    public abstract class SimpleTransformer : GameCommandHandler
    {
        public enum LoopType
        {
            Once, PingPong, Repeat
        }

        [Tooltip("Once: Only played once. PingPong: Back and forth. Repeat: Start from the beginning")]
        public LoopType loopType;

        public float duration = 1;
        public AnimationCurve accelCurve;

        [Tooltip("Play at the start?")]
        public bool activate = false;
        public SendGameCommand OnStartCommand, OnStopCommand;

        public AudioSource onStartAudio, onEndAudio;

        [Tooltip("Preview the position of the transformation")]
        [Range(0, 1)] public float previewPosition;

        float time = 0f;
        float position = 0f;
        float direction = 1f;

        protected Platform m_Platform;

        [ContextMenu("Test Start Audio")]
        void TestStartAudio()
        {
            if (onStartAudio != null) onStartAudio.Play();
        }

        [ContextMenu("Test Stop Audio")]
        void TestStopAudio()
        {
            if (onEndAudio != null) onEndAudio.Play();
        }

        protected override void Awake()
        {
            base.Awake();

            m_Platform = GetComponentInChildren<Platform>();
        }

        public override void PerformInteraction()
        {
            activate = true;
            if (OnStartCommand != null) OnStartCommand.Send();
            if (onStartAudio != null) onStartAudio.Play();
        }

        public void FixedUpdate()
        {
            if (activate)
            {
                time = time + (direction + Time.deltaTime / duration);
                switch (loopType)
                {
                    case LoopType.Once:
                        LoopOnce();
                        break;
                    case LoopType.PingPong:
                        LoopPingPong();
                        break;
                    case LoopType.Repeat:
                        LoopRepeat();
                        break;
                }
                PerformTransform(position);
            }
        }

        public virtual void PerformTransform(float position){
            //Empty function for other classes derived from this one to use
        }

        void LoopOnce(){
            position = Mathf.Clamp01(time);
            if(position >=1){
                enabled = false;
                if(OnStopCommand != null) OnStopCommand.Send();
                if(onEndAudio != null) onEndAudio.Play();
                direction *= -1;
            }
        }

        void LoopPingPong(){
            position = Mathf.PingPong(time, 1f);
        }

        void LoopRepeat(){
            position = Mathf.Repeat(time, 1f);
        }
    }
}