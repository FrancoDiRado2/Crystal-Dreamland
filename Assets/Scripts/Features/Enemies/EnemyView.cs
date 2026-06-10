using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public Animator animator;

    [Header("Sonidos de Garmanar")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound; // 🎵 Agregado para cuando recibe daño

    private EnemyViewModel _viewModel;
    private PlayerViewModel _playerViewModel; 

    private int _lastHealth = 100;

    public void Initialize(EnemyViewModel viewModel, PlayerViewModel playerViewModel)
    {
        _viewModel = viewModel;
        _playerViewModel = playerViewModel;

        _viewModel.OnDefeated += HandleDefeated;
        _viewModel.OnCombatStarted += HandleCombatStart;
        _viewModel.OnNotEnoughPower += HandleNotEnoughPower;
        
        // Escuchamos animaciones
        _viewModel.OnHealthChanged += HandleHealthChanged;
        _viewModel.OnAttackAnim += PlayAttackAnimation;

        _lastHealth = 100; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int currentPower = _playerViewModel.GetCurrentPower(); 
            _viewModel.TryStartCombat(currentPower);
        }
    }

    private void HandleCombatStart() { }
    private void HandleNotEnoughPower() { }

    // Lógica de Flinch para Garmanar
    private void HandleHealthChanged(int newHealth)
    {
        if (animator == null) return;

        if (newHealth < _lastHealth)
        {
            animator.SetTrigger("Flinch");
            // 🎵 SUENA EL GOLPE
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }
        _lastHealth = newHealth;
    }

    // Lógica de Ataque para Garmanar
    private void PlayAttackAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Attack");
        // 🎵 SUENA EL ATAQUE
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);
    }

    private void HandleDefeated()
    {
        if (animator != null) animator.SetTrigger("Flinch"); 
        GetComponent<Collider>().enabled = false; 
        // El sonido de muerte ya NO está acá, se mudó al Árbitro para que no se corte
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnDefeated -= HandleDefeated;
            _viewModel.OnCombatStarted -= HandleCombatStart;
            _viewModel.OnNotEnoughPower -= HandleNotEnoughPower;
            _viewModel.OnHealthChanged -= HandleHealthChanged;
            _viewModel.OnAttackAnim -= PlayAttackAnimation;
        }
    }
}