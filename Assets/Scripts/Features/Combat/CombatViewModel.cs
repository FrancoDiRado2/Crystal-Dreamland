using System;
using System.Threading.Tasks;

public class CombatViewModel
{
    private EnemyViewModel _enemyVM;
    private PlayerViewModel _playerVM;

    // Estado del combate
    public bool IsPlayerTurn { get; private set; } = true;
    public string CurrentTurnMessage { get; private set; }

    // Eventos para que la UI se actualice
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
        if (!IsPlayerTurn) return; // Evita doble clic
        
        IsPlayerTurn = false;
        
        // --- LA MATEMÁTICA DEL DAÑO ---
        int baseDamage = 10;
        int currentCrystals = _playerVM.GetCurrentPower();
        int extraCrystals = currentCrystals - 25; // 25 es lo que exige la puerta
        
        // Si juntó más de 25, pega más fuerte. (Por ej: 5 de daño extra por cada cristal)
        int finalDamage = baseDamage;
        if (extraCrystals > 0)
        {
            finalDamage += (extraCrystals * 5);
        }

        CurrentTurnMessage = $"¡Atacás con {finalDamage} de daño!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        _enemyVM.TakeDamage(finalDamage);

        // Esperamos 2 segundos para que el jugador lea el texto y vea la animación
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
        
        // Acá a futuro llamarías a _playerVM.Heal(20);
        // Por ahora pasamos de turno directamente
        StartEnemyTurn(); 
    }

    private async void StartEnemyTurn()
    {
        CurrentTurnMessage = "Turno del Jefe...";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        // El enemigo "piensa" 1.5 segundos
        await Task.Delay(1500);

        int enemyDamage = 15;
        CurrentTurnMessage = $"¡El Jefe te ataca y quita {enemyDamage}!";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        // Acá a futuro llamarías a _playerVM.TakeDamage(enemyDamage);
        
        await Task.Delay(1500);
        StartPlayerTurn();
    }

    private void EndCombat(bool playerWon)
    {
        if (playerWon)
        {
            CurrentTurnMessage = "¡VICTORIA!";
        }
        else
        {
            CurrentTurnMessage = "Has sido derrotado...";
        }
        
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        OnCombatEnded?.Invoke();
    }
}