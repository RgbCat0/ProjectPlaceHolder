using System.Collections;
using JetBrains.Annotations;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : NetworkBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAttack _enemyAttack;
        private AniManager _aniManager;

        [SerializeField]
        private float walkAnimationSpeedBase = 1.5f;


        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _aniManager = GetComponent<AniManager>();
            _enemyAttack = GetComponentInChildren<EnemyAttack>();
        }

        private void Start()
            => StartCoroutine(UpdateTarget());


        // ReSharper disable Unity.PerformanceAnalysis
        public void SetSpeed(float speed)
        {
            if (_navMeshAgent == null)
                _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.stoppingDistance = 1f;
            _navMeshAgent.speed = speed;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.players.Count == 0)
                return;
            // set animation based on speed
            if (_navMeshAgent.velocity.magnitude > 0.1f)
            {
                _aniManager.ChangeFloat("WalkSpeed", _navMeshAgent.velocity.magnitude + walkAnimationSpeedBase); // base + speed
                _aniManager.ChangeAnimation("Walk", 0.2f);
            }
            else
            {
                _aniManager.ChangeAnimation("Idle", 0.2f);
            }
        }

        private IEnumerator UpdateTarget()
        {
            while (true)
            {
                _target = GetTarget();
                if (_target != null)
                    _navMeshAgent.SetDestination(_target.position);
                yield return new WaitForSeconds(0.5f); // Update every 0.5 seconds
            }
        }

        [CanBeNull]
        private Transform GetTarget()
        {
            if (GameManager.Instance.players.Count == 0)
                return null;
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

            return closestPlayer?.transform;
        }
    }
}