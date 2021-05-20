namespace ML.Enemies {
    using UnityEngine;
    using ML.Core;

    [DefaultExecutionOrder(100)]
    public class EnemyBehavior : MonoBehaviour, IMessageReceiver {
        public TargetScanner playerScanner;

        public PlayerController target { get { return m_Target; } }
        public EnemyController controller { get { return m_Controller; } }

        PlayerController m_Target = null;
        EnemyController m_Controller;

        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {
            Debug.Log("Message Received!");
        }

        private void OnDrawGizmos() {
            playerScanner.EditorGizmo(transform);
        }
    }
}