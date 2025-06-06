using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using _Scripts.Managers;

namespace _Scripts.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : NetworkBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private Transform _target;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void SetSpeed(float speed)
        {
            if (_navMeshAgent == null)
                _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.stoppingDistance = 2f;
            _navMeshAgent.speed = speed;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.players.Count == 0)
                return;

            NetworkObject closestPlayer = null;
            var closestDistanceSqr = float.MaxValue;
            Vector3 currentPosition = transform.position;

            foreach (NetworkObject player in GameManager.Instance.players)
            {
                if (player == null || !player.IsSpawned || player.transform == null || !player.isActiveAndEnabled)
                    continue;
                float distanceSqr = (player.transform.position - currentPosition).sqrMagnitude;
                if (distanceSqr > closestDistanceSqr)
                    continue;

                closestDistanceSqr = distanceSqr;
                closestPlayer = player;
            }

            if (closestPlayer != null)
                _navMeshAgent.SetDestination(closestPlayer.transform.position);
        }
    }
}