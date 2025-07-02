using System;
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
        private bool _breaker;


        [SerializeField]
        private float walkAnimationSpeedBase = 1.5f;

        [SerializeField]
        private float rotationSpeed = 700f;


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
            _navMeshAgent.stoppingDistance = 1.5f;
            _navMeshAgent.speed = speed;
            _navMeshAgent.updateRotation = false;
        }

        private void Update()
        {
            if(!_navMeshAgent.enabled)
                return;
            if (_navMeshAgent.remainingDistance >= 1.5f)
            {
                Vector3 direction = _navMeshAgent.steeringTarget - transform.position;
                direction.y = 0f; // Optional: keep rotation flat on Y-axis

                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                // if close enough lock on to the target
                transform.rotation = Quaternion.LookRotation(_target.position - transform.position);
                // ensure enemy is always upright
                Vector3 eulerAngles = transform.eulerAngles;
                eulerAngles.x = 0f;
                eulerAngles.z = 0f;
                transform.eulerAngles = eulerAngles;
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.players.Count == 0)
                return;
            // set animation based on speed
            if (_navMeshAgent.velocity.magnitude > 0.1f)
            {
                _aniManager.ChangeFloat("WalkSpeed",
                    _navMeshAgent.velocity.magnitude + walkAnimationSpeedBase); // base + speed
                _aniManager.ChangeAnimation("Walk", 0.2f);
            }
            else
            {
                _aniManager.ChangeAnimation("Idle", 0.2f);
            }
        }

        private IEnumerator UpdateTarget()
        {
            yield return
                new WaitUntil(() => _navMeshAgent.enabled); // still spawning, wait for NavMeshAgent to be enabled
            while (true)
            {
                if (_breaker)
                {
                    // _navMeshAgent.ResetPath();
                    yield break; // exit the coroutine if breaker is true
                }
                _target = GetTarget();
                if (_target != null)
                    _navMeshAgent.SetDestination(_target.position);
                yield return new WaitForFixedUpdate();
            }
        }

        public void StopTarget()
        {
            _breaker = true;
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