using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetSpeed(float speed)
    {
        _animator.SetFloat(Speed, speed);
    }
    
    public void SetFalling(bool isFalling)
    {
        _animator.SetBool(IsFalling, isFalling);
    }
}

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling
}
