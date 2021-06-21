namespace ML.Effects
{
    using UnityEngine;

    /// <summary>
    /// Modulates the color of a light between two colors - A & B - per the
    /// modulation type - Sine, Triangle, Perlin, or Random - 
    /// over X time based on the type and frequency
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class ModulatedLight : MonoBehaviour
    {
        public enum ModulationType
        {
            Sine, Triangle, Perlin, Random
        }
        public ModulationType type = ModulationType.Sine;
        [Tooltip("Higher numbers modulate the color faster.")]
        public float frequency = 1f;
        public Color colorA = Color.red;
        public Color colorB = Color.blue;

        public new Light light;

        float TriangleWave(float x)
        {
            var frac = x - (int)x;
            var a = frac * 2.0f - 1.0f;
            return a > 0 ? a : -a;
        }

        private void Reset()
        {
            light = GetComponent<Light>();
            if (light != null) colorA = light.color;
        }

        private void Update()
        {
            if (light == null) light = GetComponent<Light>();

            var time = 0f;

            switch (type)
            {
                case ModulationType.Sine:
                    time = Mathf.Sin(Time.time * frequency);
                    break;
                case ModulationType.Triangle:
                    time = TriangleWave(Time.time * frequency);
                    break;
                case ModulationType.Perlin:
                    time = Mathf.PerlinNoise(Time.time * frequency, 0.5f);
                    break;
                case ModulationType.Random:
                    time = Random.value;
                    break;
            }
            light.color = Color.Lerp(colorA, colorB, time);
        }

    }
}