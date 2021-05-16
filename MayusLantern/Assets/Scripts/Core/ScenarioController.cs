namespace ML.Core
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class ScenarioController : MonoBehaviour
    {
        [System.Serializable]
        public class ScenarioObjective
        {
            public string name;
            [Tooltip("Required number of triggers or events needed to complete the scenario.")]
            public int requiredCount;
            public bool completed = false;
            public int currentCount;
            public bool eventFired = false;
            public UnityEvent OnComplete, OnProgress;

            public void Complete()
            {
                currentCount += 1;
                if (currentCount >= requiredCount)
                {
                    completed = true;
                    if (!eventFired) OnComplete.Invoke();
                    eventFired = true;
                }
                else
                {
                    OnProgress.Invoke();
                }
            }
        }

        public UnityEvent OnAllObjectivesComplete;

        [SerializeField]
        List<ScenarioObjective> objectives = new List<ScenarioObjective>();

        public bool AddObjective(string name, int requiredCount)
        {
            for (var i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].name == name) return false;
            }

            objectives.Add(new ScenarioObjective()
            {
                name = name,
                requiredCount = requiredCount
            });

            objectives.Sort((A, B) => A.name.CompareTo(B.name));
            return true;
        }

        public void RemoveObjective(string name)
        {
            for (var i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].name == name)
                {
                    objectives.RemoveAt(i);
                    return;
                }
            }
        }

        public ScenarioObjective[] GetAllObjectives()
        {
            return objectives.ToArray();
        }

        public void CompleteObjective(string name)
        {
            for (var i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].name == name)
                {
                    objectives[i].Complete();
                }
            }
            for (var i = 0; i < objectives.Count; i++)
            {
                if (!objectives[i].completed) return;
            }

            OnAllObjectivesComplete.Invoke();
        }
    }
}