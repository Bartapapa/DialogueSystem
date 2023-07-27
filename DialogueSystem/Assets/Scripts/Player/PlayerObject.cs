using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct CharacterControllerInputs
{
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
}

public class PlayerObject : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] private SimplePlayerController CharacterController;
    [SerializeField] private Camera CharacterCamera;
    [SerializeField] private InteractibleHandler InteractibleHandler;

    private Vector2 _movement;

    private void Update()
    {
        if (CharacterController) HandleMovementInput();
    }

    #region MOVEMENT
    public void OnMovementInput(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //Request jump from character controller.
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractibleHandler.InteractWithCurrentInteractible();
        }
    }

    #endregion
    #region UI
    #endregion

    private void HandleMovementInput()
    {
        CharacterControllerInputs characterInputs = new CharacterControllerInputs();

        characterInputs.CameraRotation = CharacterCamera.transform.rotation;
        characterInputs.MoveAxisForward = _movement.y;
        characterInputs.MoveAxisRight = _movement.x;

        CharacterController.SetInputs(characterInputs);
    }
}
