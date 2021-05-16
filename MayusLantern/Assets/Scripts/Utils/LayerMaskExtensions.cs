namespace ML.Utils
{
    using UnityEngine;
    
    public static class LayerMaskExtensions {
        public static bool Contains(this LayerMask layerMask, GameObject gameObject){
            return 0 != (layerMask.value & 1 << gameObject.layer);
        }
    }
}