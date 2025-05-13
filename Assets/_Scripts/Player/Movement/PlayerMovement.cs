using _Scripts.Player;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerStats _playerStats;

    private Vector2 _moveInput;
    private Rigidbody _rb;
    public  bool canMove = true;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxVel;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        _playerStats = GetComponent<PlayerStats>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveSpeed *= _playerStats.speedMultiplier;
        maxVel *= _playerStats.speedMultiplier;
        _moveInput = InputHandler.Instance.moveInput;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        Move();
    }


    private void Move()
    {
        Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
        moveDir.Normalize();
        if (moveDir != Vector3.zero)
        {
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
