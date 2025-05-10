using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInputComponent;

    public static Vector2 movementDirection;
    public static bool jumpPressed;
    public static bool jumpHeld;
    public static bool jumpReleased;

    InputAction moveAction;
    InputAction jumpAction;

    void Awake()
    {
        playerInputComponent = GetComponent<PlayerInput>();

        moveAction = playerInputComponent.actions["Move"];
        jumpAction = playerInputComponent.actions["Jump"];
    }

    void Start()
    {

    }

    void Update()
    {
        movementDirection = moveAction.ReadValue<Vector2>();

        jumpPressed = jumpAction.WasPressedThisFrame();
        jumpHeld = jumpAction.IsPressed();
        jumpReleased = jumpAction.WasReleasedThisFrame();
    }
}
