using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 

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
    private bool _wasGrounded; 
    
    [Header("Sonidos de Combate")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip healSound;

    [Header("Sonidos de Exploración")]
    public AudioClip stepSound;    
    public AudioClip landSound;    

[Header("Volumen de Exploración")]
    [Range(0f, 1f)] public float stepBaseVolume = 0.05f; // Barrita en el inspector
    [Range(0f, 1f)] public float landBaseVolume = 0.2f;  // Barrita en el inspector

    [Header("Efectos Visuales")]
    public Light hitLight; 

    [Header("Equipamiento")]
    public GameObject torchObject;

    // 🌟 TEMPORIZADOR MATEMÁTICO PARA PASOS (Sin solapamientos)
    private float _footstepTimer = 0f;
    [SerializeField] private float _walkStepInterval = 0.45f;   // Cada cuánto suena al caminar (segundos)
    [SerializeField] private float _sprintStepInterval = 0.3f;  // Cada cuánto suena al correr (segundos)

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

        _wasGrounded = _isGrounded;
        _isGrounded = _controller.isGrounded;

        // Sonido de aterrizaje
        if (_isGrounded && !_wasGrounded && _velocity.y < -3f)
        {
            PlayLandSound();
        }

        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

        bool isSprinting = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        Vector2 inputVector = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputVector.y += 1;
            if (Keyboard.current.sKey.isPressed) inputVector.y -= 1;
            if (Keyboard.current.dKey.isPressed) inputVector.x += 1;
            if (Keyboard.current.aKey.isPressed) inputVector.x -= 1;
            
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                ToggleTorch();
            }
        }

        CalculateMovement(inputVector.normalized, isSprinting);

        // 🌟 LÓGICA DE PASOS SINTÉTICOS (Imposible que se solapen)
        if (_isGrounded && _horizontalMove.magnitude > 0.1f)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                PlayFootstepAudio(); // Dispara el sonido
                // Resetea el tiempo dependiendo si corremos o caminamos
                _footstepTimer = isSprinting ? _sprintStepInterval : _walkStepInterval;
            }
        }
        else
        {
            // Si nos detenemos, el reloj se reinicia y no suena nada
            _footstepTimer = 0f;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_viewModel.JumpForce * -2f * _viewModel.Gravity);
            TriggerJumpAnimation();
        }

        _velocity.y += _viewModel.Gravity * Time.deltaTime;
        Vector3 finalMovement = _horizontalMove + (Vector3.up * _velocity.y);
        _controller.Move(finalMovement * Time.deltaTime);
    }

    private void ToggleTorch()
    {
        if (torchObject != null)
        {
            bool isTorchActive = !torchObject.activeSelf;
            torchObject.SetActive(isTorchActive);

            if (_animator != null)
            {
                _animator.SetBool("HasTorch", isTorchActive);
            }
        }
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
        if (_animator != null) _animator.SetFloat("Speed", 0f);
    }

    public void TeleportTo(Transform targetTransform)
    {
        if (_controller != null) _controller.enabled = false;
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        if (_controller != null) _controller.enabled = true;
    }
    
    public void PlayAttackAnimation()
    {
        if (_animator != null) _animator.SetTrigger("Attack");
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound); 
    }

    private void PlayFlinchAnimation()
    {
        if (_animator != null) _animator.SetTrigger("Flinch");
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound); 
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

// 🌟 REPRODUCTOR PRIVADO
    private void PlayFootstepAudio()
    {
        if (audioSource != null && stepSound != null) 
        {
            // Usa el volumen de tu barrita, más un mini "ruido" aleatorio para que suene natural
            float finalVolume = stepBaseVolume + Random.Range(0f, 0.02f);
            audioSource.PlayOneShot(stepSound, finalVolume);
        }
    }

    public void PlayLandSound()
    {
        if (audioSource != null && landSound != null) 
        {
            // Usa directamente el volumen de la barrita
            audioSource.PlayOneShot(landSound, landBaseVolume);
        }
    }

    private IEnumerator FlashLightRoutine()
    {
        if (hitLight == null) yield break;

        hitLight.gameObject.SetActive(true); 
        float startIntensity = hitLight.intensity;
        float t = 0f;
        
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            hitLight.intensity = Mathf.Lerp(startIntensity, 0f, t / 0.15f);
            yield return null;
        }
        
        hitLight.gameObject.SetActive(false); 
        hitLight.intensity = startIntensity; 
    }

    private void OnDestroy()
    {
        if (_viewModel != null) _viewModel.OnTakeDamageFlinch -= PlayFlinchAnimation;
    }
}