using System;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator _animator;
        private String _currentState = "Idle";

        private void Start()
        {
            _animator = GetComponent<Animator>();
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
}
