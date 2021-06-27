namespace ML.Characters.Enemies
{
    using UnityEngine;
    using UnityEngine.AI;


    [DefaultExecutionOrder(-1)] //this ensures this runs before any behaviour or script that depends on it

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {

    }
}