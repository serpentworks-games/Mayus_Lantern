namespace ML.Core.DamageSystem
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class ContactDamager : MonoBehaviour
    {
        [HelpBox] public string helpString = @"
        Remember:
        - Set the collider on this object or its children to trigger
        - Place the object on a layer that colliders with what you're trying to damage (if the parent object is on a layer that does not collide with the layer in question, add a child on a diffrent layer)
        ";

        public int amount;
        public LayerMask damageLayers;

        private void OnTriggerStay(Collider other)
        {

            //If the collider is not on the damageable layers, don't apply damage
            if ((damageLayers.value & 1 << other.gameObject.layer) == 0) return;

            Damageable d = other.GetComponentInChildren<Damageable>();

            if (d != null && !d.isInvulnerable)
            {
                Damageable.DamageMessage message = new Damageable.DamageMessage
                {
                    damageSource = transform.position,
                    damager = this,
                    amount = amount,
                    direction = (other.transform.position - transform.position).normalized,
                };

                d.ApplyDamage(message);
            }
        }

    }
    public class HelpBoxAttribute : PropertyAttribute
    {

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Max(EditorGUIUtility.singleLineHeight * 2, EditorStyles.helpBox.CalcHeight(new GUIContent(property.stringValue), Screen.width) + EditorGUIUtility.singleLineHeight);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.HelpBox(position, property.stringValue, MessageType.Info);
        }
    }
#endif
}