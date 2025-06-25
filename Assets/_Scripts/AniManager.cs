using Unity.Netcode;
using UnityEngine;

public class AniManager : NetworkBehaviour
{
    private Animator _animator;
    private string _currentState = "idle"; // Default state, can be changed as needed

    private void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null) // try getting in child
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject or its children.");
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ChangeAnimationRpc(string newState, float crossFade = 0.7f, int layer = 0)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;
        _animator.CrossFade(_currentState, crossFade, layer);
    }

    public void ChangeFloat(string parameterName, float value)
    {
        _animator.SetFloat(parameterName, value);
    }

}