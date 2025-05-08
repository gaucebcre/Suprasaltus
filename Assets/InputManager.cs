using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInputComponent;

    public static Vector2 MovementDirection;
    public static bool JumpPressed;
    public static bool JumpHeld;
    public static bool JumpReleased;

    InputAction moveAction;
    InputAction jumpAction;

    void Awake()
    {
        PlayerInputComponent = GetComponent<PlayerInput>();

        moveAction = PlayerInputComponent.actions["Move"];
        jumpAction = PlayerInputComponent.actions["Jump"];

    }

    void Start()
    {

    }

    void Update()
    {
        MovementDirection = moveAction.ReadValue<Vector2>();

        JumpPressed = jumpAction.WasPressedThisFrame();
        JumpHeld = jumpAction.IsPressed();
        JumpReleased = jumpAction.WasReleasedThisFrame();
    }
}
