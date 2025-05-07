using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 _moveInput;
    private Rigidbody _rb;


    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxVel;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _moveInput = InputHandler.Instance.moveInput;
    }

    private void FixedUpdate()
    {
        Move();
    }


    private void Move()
    {
        Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
        moveDirection.Normalize();
        if (moveDirection != Vector3.zero)
        {
            _rb.AddForce(moveDirection * moveSpeed, ForceMode.VelocityChange);
            if (_rb.linearVelocity.magnitude > maxVel)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxVel;
            }
        }
    }
}
