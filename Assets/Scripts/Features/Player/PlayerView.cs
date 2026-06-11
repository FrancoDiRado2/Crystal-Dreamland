using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para las Corrutinas

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private MainHUDView _hudView;

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

    [Header("Efectos Visuales")]
    public Light hitLight; // 🌟 EL DESTELLO

    public void Initialize(PlayerViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.OnTakeDamageFlinch += PlayFlinchAnimation;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_viewModel == null) return; 

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
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
        }
    }

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
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound); 
    }

    private void PlayFlinchAnimation()
    {
        if (_animator != null) _animator.SetTrigger("Flinch");
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound); 
        
        // 🌟 Disparamos el destello de luz
        StartCoroutine(FlashLightRoutine());
    }

    public void PlayDeathAnimation()
    {
        if (audioSource != null && deathSound != null) audioSource.PlayOneShot(deathSound); 
    }

    public void PlayHealSound()
    {
        if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound); 
    }

    // 🌟 LA MAGIA DEL DESTELLO
    private IEnumerator FlashLightRoutine()
    {
        if (hitLight == null) yield break;

        hitLight.gameObject.SetActive(true); // Prendemos la luz
        float startIntensity = hitLight.intensity;
        float t = 0f;
        
        // Hacemos que la luz se apague suavemente en 0.15 segundos
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            hitLight.intensity = Mathf.Lerp(startIntensity, 0f, t / 0.15f);
            yield return null;
        }
        
        hitLight.gameObject.SetActive(false); // La apagamos del todo
        hitLight.intensity = startIntensity; // Le devolvemos su fuerza original para el próximo golpe
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnTakeDamageFlinch -= PlayFlinchAnimation;
        }
    }
}