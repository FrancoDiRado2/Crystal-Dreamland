using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private MainHUDView _hudView;

    private Animator _animator;
    private CharacterController _controller;
    private PlayerViewModel _viewModel;

    private Vector3 _velocity;
    private Vector3 _horizontalMove;
    private bool _isGrounded;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _viewModel = GetComponent<PlayerViewModel>(); 
    }

    private void Update()
    {
        // Bloqueo por nombre
        if (_hudView != null && !_hudView.HasSetPlayerName)
        {
            UpdateMoveAnimation(0f);
            return; 
        }

        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

        // Detectar si se presiona Shift para correr
        bool isSprinting = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        Vector2 inputVector = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputVector.y += 1;
            if (Keyboard.current.sKey.isPressed) inputVector.y -= 1;
            if (Keyboard.current.dKey.isPressed) inputVector.x += 1;
            if (Keyboard.current.aKey.isPressed) inputVector.x -= 1;
        }

        CalculateMovement(inputVector.normalized, isSprinting);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_viewModel.JumpForce * -2f * _viewModel.Gravity);
            TriggerJumpAnimation();
        }

        _velocity.y += _viewModel.Gravity * Time.deltaTime;
        Vector3 finalMovement = _horizontalMove + (Vector3.up * _velocity.y);
        _controller.Move(finalMovement * Time.deltaTime);
    }

    private void CalculateMovement(Vector2 input, bool isSprinting)
    {
        _horizontalMove = Vector3.zero;

        if (input.sqrMagnitude >= 0.01f)
        {
            Transform camTransform = Camera.main.transform;
            Vector3 forward = camTransform.forward;
            Vector3 right = camTransform.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();

            Vector3 moveDir = forward * input.y + right * input.x;
            
            // 1. Aplicamos la velocidad física
            float targetSpeed = isSprinting ? _viewModel.SprintSpeed : _viewModel.WalkSpeed;
            _horizontalMove = moveDir * targetSpeed;

            // 2. Calculamos el valor para el Animator (Speed)
            float animMultiplier = targetSpeed / _viewModel.WalkSpeed; 
            UpdateMoveAnimation(input.magnitude * animMultiplier);

            // --- ARREGLO: La rotación automática solo ocurre en 3ra persona ---
            // Asegurate de que tu cámara de primera persona tenga el nombre "FirstPersonCamera"
            if (Camera.main.name != "FirstPersonCamera")
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _viewModel.RotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            UpdateMoveAnimation(0f);
        }
    }

    private void UpdateMoveAnimation(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    private void TriggerJumpAnimation()
    {
        _animator.SetTrigger("Jump");
    }
}