namespace ML.Core.Weapons
{
    using UnityEngine;
    using System.Collections;

    public class TimeEffect : MonoBehaviour
    {
        public Light weaponLight;

        Animation animation;

        private void Awake()
        {
            animation = GetComponent<Animation>();
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            weaponLight.enabled = true;

            if (animation) animation.Play();

            StartCoroutine(DisableAtEndOfAnimation());
        }

        IEnumerator DisableAtEndOfAnimation()
        {
            yield return new WaitForSeconds(animation.clip.length);

            gameObject.SetActive(false);
            weaponLight.enabled = false;
        }
    }
}