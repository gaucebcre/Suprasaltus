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
    public static bool menuPressed;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction menuAction;

    void Awake()
    {
        playerInputComponent = GetComponent<PlayerInput>();

        moveAction = playerInputComponent.actions["Move"];
        jumpAction = playerInputComponent.actions["Jump"];
        menuAction = playerInputComponent.actions["Menu"];
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
        menuPressed = menuAction.WasPressedThisFrame();
    }
}
