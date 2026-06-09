using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public Animator animator;

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

    private void HandleCombatStart() { Debug.Log("¡CÁMARA DE COMBATE!"); }
    private void HandleNotEnoughPower() { Debug.Log("Faltan cristales"); }

    // Lógica de Flinch para Garmanar
    private void HandleHealthChanged(int newHealth)
    {
        if (animator == null) 
        {
            Debug.LogWarning("¡Atención! Garmanar recibió daño pero su casillero 'Animator' está vacío en el Inspector.");
            return;
        }

        if (newHealth < _lastHealth)
        {
            Debug.Log("¡Garmanar recibió daño! Ejecutando Flinch..."); 
            animator.SetTrigger("Flinch");
        }
        _lastHealth = newHealth;
    }

    // Lógica de Ataque para Garmanar
    private void PlayAttackAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("¡Atención! El árbitro mandó a atacar pero el casillero 'Animator' de Garmanar está vacío.");
            return;
        }

        Debug.Log("¡El árbitro le dijo a Garmanar que ataque!"); 
        animator.SetTrigger("Attack");
    }

    private void HandleDefeated()
    {
        if (animator != null) animator.SetTrigger("Flinch"); 
        GetComponent<Collider>().enabled = false; 
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