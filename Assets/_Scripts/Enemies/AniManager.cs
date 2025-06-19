using System;
using UnityEngine;


public class AniManager : MonoBehaviour
{
    private Animator _animator;
    private String _currentState = "idle"; // Default state, can be changed as needed

    private void Start()
    {
        _animator = GetComponent<Animator>();
        if(_animator == null) // try getting in child
        {
            _animator = GetComponentInChildren<Animator>();
        }
        if(_animator == null) 
        {
            Debug.LogError("Animator component not found on the GameObject or its children.");
        }
    }


    public void ChangeAnimation(string newState, float crossFade = 0.7f)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
            _animator.CrossFade(_currentState, crossFade);
        }
    }
}