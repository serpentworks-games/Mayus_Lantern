namespace ML.Utils
{
    using UnityEngine;
    using System.Collections.Generic;

    public class BatchProcessor : MonoBehaviour
    {
        public delegate void BatchProcessing();

        static protected BatchProcessor s_Instance;
        static protected List<BatchProcessing> s_ProcessList;

        static BatchProcessor()
        {
            s_ProcessList = new List<BatchProcessing>();
        }

        static public void RegisterBatchFunction(BatchProcessing function)
        {
            s_ProcessList.Add(function);
        }

        static public void UnregisterBatchFunction(BatchProcessing function)
        {
            s_ProcessList.Remove(function);
        }

        private void Update()
        {
            for (int i = 0; i < s_ProcessList.Count; i++)
            {
                s_ProcessList[i]();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (s_Instance != null) return;

            GameObject obj = new GameObject("BatchProcessor");
            DontDestroyOnLoad(obj);

            s_Instance = obj.AddComponent<BatchProcessor>();
        }
    }
}