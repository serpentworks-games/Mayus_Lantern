namespace ML.Helpers
{
    using UnityEngine;
    using System.Collections.Generic;

    public class ObjectPooler<T> where T : UnityEngine.MonoBehaviour, IPooled<T>
    {
        public T[] instances;

        protected Stack<int> m_FreeIndex;

        public void Initialize(int count, T prefab)
        {
            instances = new T[count];
            m_FreeIndex = new Stack<int>(count);

            for (int i = 0; i < count; i++)
            {
                instances[i] = Object.Instantiate(prefab);
                instances[i].gameObject.SetActive(false);
                instances[i].poolID = i;
                instances[i].pool = this;

                m_FreeIndex.Push(i);
            }
        }

        public T GetNew()
        {
            int index = m_FreeIndex.Pop();
            instances[index].gameObject.SetActive(true);

            return instances[index];
        }

        public void Free(T obj)
        {
            m_FreeIndex.Push(obj.poolID);
            instances[obj.poolID].gameObject.SetActive(false);
        }
    }

    public interface IPooled<T> where T : MonoBehaviour, IPooled<T>
    {
        int poolID { get; set; }
        ObjectPooler<T> pool { get; set; }
    }
}