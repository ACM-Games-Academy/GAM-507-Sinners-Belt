using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    //Movement input reader for the player
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    //Action States
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool DashPressed { get; private set; }

    
    private void LateUpdate()
    {
        //Reset single frame inputs
        JumpPressed = false;
        DashPressed = false;
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsSprinting = true;
        }
        else if (context.canceled)
        {
            IsSprinting = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsCrouching = !IsCrouching; //toggle crouch
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpHeld = true;
            JumpPressed = true; //trigger jump 
        }
        else if (context.canceled)
        {
            JumpHeld = false;
        }
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DashPressed = true; //trigger dash
        }
    }
}
