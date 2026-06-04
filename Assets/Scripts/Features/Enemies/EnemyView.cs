using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public Animator animator;

    private EnemyViewModel _viewModel;
    private PlayerViewModel _playerViewModel; 

    // Se inyecta desde el Bootstrapper
    public void Initialize(EnemyViewModel viewModel, PlayerViewModel playerViewModel)
    {
        _viewModel = viewModel;
        _playerViewModel = playerViewModel;

        _viewModel.OnDefeated += HandleDefeated;
        _viewModel.OnCombatStarted += HandleCombatStart;
        _viewModel.OnNotEnoughPower += HandleNotEnoughPower;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Leemos el poder de Aman en tiempo real
            int currentPower = _playerViewModel.GetCurrentPower(); 
            _viewModel.TryStartCombat(currentPower);
        }
    }

    private void HandleCombatStart()
    {
        Debug.Log("¡CÁMARA DE COMBATE: INICIA EL DUELO!");
        // En el próximo paso acá frenaremos el movimiento de Aman y cambiaremos la cámara.
    }

    private void HandleNotEnoughPower()
    {
        Debug.Log("¡Te faltan cristales para pelear con este jefe!");
    }

    private void HandleDefeated()
    {
        if (animator != null) //animator.SetTrigger("Die");
        GetComponent<Collider>().enabled = false; // Apagamos el trigger para que no moleste más
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnDefeated -= HandleDefeated;
            _viewModel.OnCombatStarted -= HandleCombatStart;
            _viewModel.OnNotEnoughPower -= HandleNotEnoughPower;
        }
    }
}