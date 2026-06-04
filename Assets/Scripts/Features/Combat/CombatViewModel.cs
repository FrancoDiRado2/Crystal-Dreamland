using System;
using System.Threading.Tasks;
using UnityEngine; // Necesario para el Random

public class CombatViewModel
{
    private EnemyViewModel _enemyVM;
    private PlayerViewModel _playerVM;

    public bool IsPlayerTurn { get; private set; } = true;
    public string CurrentTurnMessage { get; private set; }

    public event Action<string> OnTurnMessageChanged;
    public event Action OnCombatEnded;

    public CombatViewModel(EnemyViewModel enemyVM, PlayerViewModel playerVM)
    {
        _enemyVM = enemyVM;
        _playerVM = playerVM;
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        IsPlayerTurn = true;
        CurrentTurnMessage = "¡Tu turno!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
    }

    public async void PlayerAttack()
    {
        if (!IsPlayerTurn) return; 
        IsPlayerTurn = false;
        
        // 1. Tu daño es exactamente tu poder (cristales)
        int finalDamage = _playerVM.GetCurrentPower();
        
        // 2. Calculamos probabilidad de crítico (30% de chances)
        bool isCritical = UnityEngine.Random.Range(0, 100) < 30;

        if (isCritical)
        {
            finalDamage += 5; // Le sumamos 5 extra por crítico
            CurrentTurnMessage = $"¡GOLPE CRÍTICO! Sacás {finalDamage} de daño.";
        }
        else
        {
            CurrentTurnMessage = $"¡Atacás con {finalDamage} de daño!";
        }

        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        // Le pegamos al jefe
        _enemyVM.TakeDamage(finalDamage);

        await Task.Delay(2000); 

        if (_enemyVM.IsDefeated)
        {
            EndCombat(true);
        }
        else
        {
            StartEnemyTurn();
        }
    }

    public void PlayerHeal()
    {
        if (!IsPlayerTurn) return;
        IsPlayerTurn = false;
        
        CurrentTurnMessage = "¡Te curás! (Aún por implementar)";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        StartEnemyTurn(); 
    }

    private async void StartEnemyTurn()
    {
        CurrentTurnMessage = "Turno del Jefe...";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        await Task.Delay(1500);

        int enemyDamage = 15;
        CurrentTurnMessage = $"¡El Jefe te ataca y quita {enemyDamage}!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        // 3. AHORA SÍ: El jefe le baja la vida real a Aman
        _playerVM.TakeDamage(enemyDamage);
        
        await Task.Delay(1500);
        StartPlayerTurn();
    }

    private void EndCombat(bool playerWon)
    {
        if (playerWon)
            CurrentTurnMessage = "¡VICTORIA! Portal desbloqueado.";
        else
            CurrentTurnMessage = "Has sido derrotado...";
        
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        OnCombatEnded?.Invoke();
    }
}