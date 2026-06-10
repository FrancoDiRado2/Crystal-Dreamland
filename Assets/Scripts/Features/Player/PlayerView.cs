using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private MainHUDView _hudView;

    // Propiedad pública para que los cristales puedan interactuar con el cerebro
    public PlayerViewModel ViewModel => _viewModel; 
    private PlayerViewModel _viewModel;

    private Animator _animator;
    private CharacterController _controller;

    private Vector3 _velocity;
    private Vector3 _horizontalMove;
    private bool _isGrounded;
    
    [Header("Sonidos de Aman")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip healSound;

    // El Bootstrapper le da el cerebro cuando arranca el juego
    public void Initialize(PlayerViewModel viewModel)
    {
        _viewModel = viewModel;

        // NUEVO: Suscribimos la Vista al evento de dolor del ViewModel
        _viewModel.OnTakeDamageFlinch += PlayFlinchAnimation;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Freno de seguridad: esperamos a que el Bootstrapper nos dé el ViewModel
        if (_viewModel == null) return; 

        // Bloqueo por nombre
        if (_hudView != null && !_hudView.HasSetPlayerName)
        {
            UpdateMoveAnimation(0f);
            return; 
        }

        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

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
            
            float targetSpeed = isSprinting ? _viewModel.SprintSpeed : _viewModel.WalkSpeed;
            _horizontalMove = moveDir * targetSpeed;

            float animMultiplier = targetSpeed / _viewModel.WalkSpeed; 
            UpdateMoveAnimation(input.magnitude * animMultiplier);

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

    private void OnDisable()
    {
        // Salvavidas para que no haga Moonwalk en el menú de pausa o combate
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
        }
    }

    // Función para teletransportar a Aman a la marca de combate
    public void TeleportTo(Transform targetTransform)
    {
        if (_controller != null) _controller.enabled = false;
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        
        if (_controller != null) _controller.enabled = true;
    }
    
    // --- FUNCIONES DE COMBATE ---
    public void PlayAttackAnimation()
    {
        if (_animator != null) _animator.SetTrigger("Attack");
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound); // 🎵 SONIDO ATAQUE
    }

    private void PlayFlinchAnimation()
    {
        if (_animator != null) _animator.SetTrigger("Flinch");
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound); // 🎵 SONIDO DAÑO
    }

    public void PlayDeathAnimation()
    {
        if (audioSource != null && deathSound != null) audioSource.PlayOneShot(deathSound); // 🎵 SONIDO MUERTE
    }

    public void PlayHealSound()
    {
        if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound); // 🎵 SONIDO CURAR
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnTakeDamageFlinch -= PlayFlinchAnimation;
        }
    }
}