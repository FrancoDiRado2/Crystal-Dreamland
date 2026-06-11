using UnityEngine;
using System.Collections; // Necesario para las Corrutinas

public class EnemyView : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public Animator animator;

    [Header("Sonidos de Garmanar")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound; 
    public AudioClip deathSound;

    [Header("Efectos Visuales")]
    public Light hitLight; // 🌟 EL DESTELLO

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

    private void HandleHealthChanged(int newHealth)
    {
        if (animator == null) return;

        if (newHealth < _lastHealth)
        {
            animator.SetTrigger("Flinch");
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
            
            // 🌟 Disparamos el destello de luz
            StartCoroutine(FlashLightRoutine());
        }
        _lastHealth = newHealth;
    }

    private void PlayAttackAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger("Attack");
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);
    }

    private void HandleDefeated()
    {
        if (animator != null) animator.SetTrigger("Flinch"); 
        GetComponent<Collider>().enabled = false; 
    }

    // 🌟 LA MAGIA DEL DESTELLO
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