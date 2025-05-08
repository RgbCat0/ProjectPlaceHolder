using System;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private Transform _target;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void SetSpeed(float speed)
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = speed;
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                _target = GameObject.FindGameObjectWithTag("Player").transform;
                return;
            }
            _navMeshAgent.SetDestination(_target.position);
        }
    }
}
