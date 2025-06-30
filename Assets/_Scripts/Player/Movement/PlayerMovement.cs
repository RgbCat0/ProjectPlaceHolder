using System;
using Player.Attack;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Player.Movement
{
    public class PlayerMovement : NetworkBehaviour
    {
        private PlayerStats _playerStats;
        private AniManager _playerAnimator;
        private AttackManager _attackManager;
        private Vector2 _moveInput;
        private Rigidbody _rb;
        private CinemachineCamera _playerCam;
        public NetworkVariable<bool> canMove;

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float rotationSpeed;

        [SerializeField]
        private float maxVel;
        
        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _playerCam = FindFirstObjectByType<CinemachineCamera>();
            _playerStats = GetComponent<PlayerStats>();
            _playerAnimator = GetComponent<AniManager>();
            _attackManager = GetComponent<AttackManager>();
            _rb = GetComponent<Rigidbody>();

            _playerCam.Target.TrackingTarget = gameObject.transform;
            _playerCam.Target.LookAtTarget = gameObject.transform;
            _playerStats.OnSpeedChanged += f => moveSpeed *= f;
        }

        private void OnEnable()
            => RealRpc();


        [Rpc(SendTo.Server)]
        private void RealRpc()
        {
            canMove.Initialize(this);   
            canMove.Value = true; // reset on respawn
        }


        private void Update()
        {
            maxVel = moveSpeed;
            _moveInput = InputHandler.Instance.moveInput;
            if (_rb.linearVelocity.magnitude < 0.1f && !_attackManager.cd)
            {
                _playerAnimator.ChangeAnimation("Idle", 0.2f);
            }
        }

        private void FixedUpdate()
        {
            if (!canMove.Value) return;
            Move();
        }


        private void Move()
        {
            Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
            moveDir.Normalize();
            if (moveDir != Vector3.zero)
            {
                _playerAnimator.ChangeAnimation("Walking", layer: 1);
                Rotation(moveDir);
                _rb.AddForce(moveDir * moveSpeed, ForceMode.VelocityChange);
                if (_rb.linearVelocity.magnitude > maxVel)
                {
                    _rb.linearVelocity = _rb.linearVelocity.normalized * maxVel;
                }
            }
        }

        private void Rotation(Vector3 moveDir)
        {
           Quaternion targetRotation = Quaternion.LookRotation(moveDir);
           transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}