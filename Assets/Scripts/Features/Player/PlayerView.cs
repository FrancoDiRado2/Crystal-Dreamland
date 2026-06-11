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
    public AudioClip jumpSound;    
    public AudioClip landSound;    

    [Header("Efectos Visuales")]
    public Light hitLight; 

    // 🌟 NUEVA REFERENCIA PARA LA ANTORCHA
    [Header("Equipamiento")]
    public GameObject torchObject;

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
            
            // 🌟 LÓGICA DEL TOGGLE CON LA TECLA "T"
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                ToggleTorch();
            }
        }

        CalculateMovement(inputVector.normalized, isSprinting);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_viewModel.JumpForce * -2f * _viewModel.Gravity);
            PlayJumpSound(); 
            TriggerJumpAnimation();
        }

        _velocity.y += _viewModel.Gravity * Time.deltaTime;
        Vector3 finalMovement = _horizontalMove + (Vector3.up * _velocity.y);
        _controller.Move(finalMovement * Time.deltaTime);
    }

    // 🌟 LA FUNCIÓN QUE PRENDE Y APAGA LA ANTORCHA
    private void ToggleTorch()
    {
        if (torchObject != null)
        {
            // Cambia el estado del objeto en la mano
            bool isTorchActive = !torchObject.activeSelf;
            torchObject.SetActive(isTorchActive);

            // Le avisa al Animator que cambie la postura de los brazos
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
    
    // --- FUNCIONES DE COMBATE Y EFECTOS ---
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

    public void PlayStepSound()
    {
        if (audioSource != null && stepSound != null) 
            audioSource.PlayOneShot(stepSound, Random.Range(0.4f, 0.6f));
    }

    public void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    public void PlayLandSound()
    {
        if (audioSource != null && landSound != null) audioSource.PlayOneShot(landSound);
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