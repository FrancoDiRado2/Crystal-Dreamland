using System;
using System.Threading.Tasks;
using UnityEngine; // Necesario para Random

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
        
        // El daño base es el poder total de Aman
        int finalDamage = _playerVM.GetCurrentPower();
        
        // Chance de Crítico (30% de probabilidad)
        bool isCritical = UnityEngine.Random.Range(0, 100) < 30;

        if (isCritical)
        {
            finalDamage += 5; 
            CurrentTurnMessage = $"¡GOLPE CRÍTICO! Sacás {finalDamage} de daño.";
        }
        else
        {
            CurrentTurnMessage = $"¡Atacás con {finalDamage} de daño!";
        }

        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        _enemyVM.TakeDamage(finalDamage);

        await Task.Delay(2000); 

        if (_enemyVM.IsDefeated)
            EndCombat(true);
        else
            StartEnemyTurn();
    }

    public void PlayerHeal()
    {
        // Por ahora mantenemos el botón sin lógica extra, solo pasa el turno
        CurrentTurnMessage = "¡Turno pasado!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        StartEnemyTurn(); 
    }

    private async void StartEnemyTurn()
    {
        CurrentTurnMessage = "Turno de Garmanar...";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        await Task.Delay(1500);

        int enemyDamage = 15;
        CurrentTurnMessage = $"¡Garmanar te ataca y quita {enemyDamage}!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        _playerVM.TakeDamage(enemyDamage);
        
        await Task.Delay(1500);
        StartPlayerTurn();
    }

    private void EndCombat(bool playerWon)
    {
        CurrentTurnMessage = playerWon ? "¡VICTORIA! Portal desbloqueado." : "Has sido derrotado...";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        OnCombatEnded?.Invoke();
    }
}