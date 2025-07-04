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

        [SerializeField] private float walkAnimationSpeedBase = 1.5f;
        [SerializeField] private float rotationSpeed = 700f;

        // NetworkVariables for animation sync
        public NetworkVariable<float> WalkSpeed = new NetworkVariable<float>(
            writePerm: NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> IsWalking = new NetworkVariable<bool>(
            writePerm: NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _aniManager = GetComponent<AniManager>();
            _enemyAttack = GetComponentInChildren<EnemyAttack>();
        }

        private void Start()
        {
            if (IsServer)
            {
                StartCoroutine(UpdateTarget());
            }

            // Always sync animations, even on clients
            WalkSpeed.OnValueChanged += (oldValue, newValue) =>
            {
                _aniManager.ChangeFloat("WalkSpeed", newValue);
            };

            IsWalking.OnValueChanged += (oldValue, newValue) =>
            {
                if (newValue)
                    _aniManager.ChangeAnimation("Walk", 0.2f);
                else
                    _aniManager.ChangeAnimation("Idle", 0.2f);
            };
        }

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
            if (!_navMeshAgent.enabled || !IsServer)
                return;

            if (_navMeshAgent.remainingDistance >= 1.5f)
            {
                Vector3 direction = _navMeshAgent.steeringTarget - transform.position;
                direction.y = 0f;

                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(_target.position - transform.position);
                Vector3 eulerAngles = transform.eulerAngles;
                eulerAngles.x = 0f;
                eulerAngles.z = 0f;
                transform.eulerAngles = eulerAngles;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer || GameManager.Instance.players.Count == 0)
                return;

            float speed = _navMeshAgent.velocity.magnitude;

            if (speed > 0.1f)
            {
                WalkSpeed.Value = speed + walkAnimationSpeedBase;
                IsWalking.Value = true;
            }
            else
            {
                IsWalking.Value = false;
            }
        }

        private IEnumerator UpdateTarget()
        {
            yield return new WaitUntil(() => _navMeshAgent.enabled);

            while (true)
            {
                if (_breaker)
                    yield break;

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
