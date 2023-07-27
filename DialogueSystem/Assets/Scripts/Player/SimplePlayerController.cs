using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float MaxMovementSpeed = 10f;
    [SerializeField] private float MovementSharpness = 15f;

    [Header("Rotation")]
    [SerializeField] private float RotationSharpness = 15f;

    private Rigidbody _rb;
    private CapsuleCollider _collider;

    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private float _colliderHeight;
    private bool _canMove = true;
    private bool _canRotate = true;
    public bool CanMove => _canMove;
    public bool CanRotate => _canRotate;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _collider = GetComponent<CapsuleCollider>();
        _colliderHeight = _collider.height;
    }

    public void SetInputs(CharacterControllerInputs inputs)
    {
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, transform.up).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, transform.up).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, transform.up);

        //Look towards camera.
        //_lookInputVector = cameraPlanarDirection;

        //Look towards movement.
        _moveInputVector = cameraPlanarRotation * moveInputVector;
        _lookInputVector = _moveInputVector.normalized;
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleVelocity();
    }

    private void HandleRotation()
    {
        if (!_canRotate || DialogueManager.instance._dialogueIsPlaying) return;
        if (_lookInputVector.sqrMagnitude > 0f && RotationSharpness > 0f)
        {
            Vector3 smoothLookInputDirection = Vector3.Slerp(transform.forward, _lookInputVector, 1 - Mathf.Exp(-RotationSharpness * Time.fixedDeltaTime)).normalized;
            transform.rotation = Quaternion.LookRotation(smoothLookInputDirection, transform.up);
        }
    }

    private void HandleVelocity()
    {
        if (!_canMove || DialogueManager.instance._dialogueIsPlaying) return;
        Vector3 currentVelocity = _rb.velocity;
        //Vector3 inputRight = Vector3.Cross(_moveInputVector, transform.up);
        Vector3 targetVelocity = _moveInputVector * MaxMovementSpeed;

        _rb.velocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-MovementSharpness * Time.fixedDeltaTime));
    }
}
