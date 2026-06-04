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

    // NUEVO: Lógica de curación con validación
    public async void PlayerHeal()
    {
        if (!IsPlayerTurn) return;

        // Comprobamos si tiene suficientes cristales extra (Cuesta 5, mínimo a mantener 25)
        if (_playerVM.GetCurrentPower() - 5 < 25)
        {
            CurrentTurnMessage = "No puedes descender de 25 de poder.";
            OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
            return; // Corta la ejecución, no pasa de turno y te deja elegir Atacar.
        }

        IsPlayerTurn = false; // Solo pasa el turno si la validación es correcta
        
        _playerVM.ConsumePower(5);
        _playerVM.Heal(25);

        CurrentTurnMessage = "Te has curado 25 de vida.";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        await Task.Delay(2000);
        StartEnemyTurn(); 
    }

    private async void StartEnemyTurn()
    {
        CurrentTurnMessage = "Turno de Garmanar...";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        await Task.Delay(1500);

        // NUEVO: Daño base de 25 y 30% de chances de crítico para el Jefe
        int enemyDamage = 40;
        bool isEnemyCritical = UnityEngine.Random.Range(0, 100) < 30;

        if (isEnemyCritical)
        {
            enemyDamage += 10; // Saca 50 en crítico
            CurrentTurnMessage = $"¡GOLPE CRÍTICO! Garmanar quita {enemyDamage} de vida.";
        }
        else
        {
            CurrentTurnMessage = $"¡Garmanar te ataca y quita {enemyDamage} de vida!";
        }
        
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        
        // El jefe le baja la vida real a Aman
        _playerVM.TakeDamage(enemyDamage);
        
        await Task.Delay(1500);

        // NUEVO: Chequeo de Derrota
        if (_playerVM.IsDefeated)
        {
            EndCombat(false); // Falso = Perdió el jugador
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private void EndCombat(bool playerWon)
    {
        if (playerWon)
            CurrentTurnMessage = "¡VICTORIA! Portal desbloqueado.";
        else
            CurrentTurnMessage = "Has sido derrotado..."; // Ya tenías el mensaje preparado
        
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        OnCombatEnded?.Invoke();
    }
}