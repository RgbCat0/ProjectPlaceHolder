using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 _moveInput;
    private Rigidbody _rb;


    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
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
