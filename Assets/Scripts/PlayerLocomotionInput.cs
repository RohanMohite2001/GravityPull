using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocoMotionMapActions
{
    private PlayerControls _playerControls;
    public Vector2 movement;
    public Vector2 look;
    public Vector2 gravity;

    private void OnEnable()
    {
        _playerControls = new PlayerControls();
        _playerControls.PlayerLocoMotionMap.Enable();
        _playerControls.PlayerLocoMotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        _playerControls.PlayerLocoMotionMap.Disable();
        _playerControls.PlayerLocoMotionMap.RemoveCallbacks(this);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        // Debug.Log(movement);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnGravity(InputAction.CallbackContext context)
    {
        gravity = context.ReadValue<Vector2>();
        Debug.Log(gravity);
    }
}
