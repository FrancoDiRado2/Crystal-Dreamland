using System;
using System.Threading.Tasks;
using UnityEngine; 

public class CombatViewModel
{
    private EnemyViewModel _enemyVM;
    private PlayerViewModel _playerVM;

    public bool IsPlayerTurn { get; private set; } = true;
    public string CurrentTurnMessage { get; private set; }

    public event Action<string> OnTurnMessageChanged;
    public event Action<bool> OnCombatEnded;

    public CombatViewModel(EnemyViewModel enemyVM, PlayerViewModel playerVM)
    {
        _enemyVM = enemyVM;
        _playerVM = playerVM;
        StartPlayerTurn();
    }

    public void ClearTurnMessage()
    {
        CurrentTurnMessage = "";
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
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
        
        int finalDamage = _playerVM.GetCurrentPower();
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
        
        // 1. En este momento exacto, el botón UI ya hizo que Aman empiece su animación
        
        // 2. Esperamos 0.8 segundos (o 1s) para que la espada viaje y "conecte" visualmente
        await Task.Delay(800); 

        // 3. ¡Impacto! Le bajamos la vida a Garmanar. 
        _enemyVM.TakeDamage(finalDamage);

        // 4. Esperamos a que termine de quejarse
        await Task.Delay(1500); 

        if (_enemyVM.IsDefeated)
            EndCombat(true);
        else
            StartEnemyTurn();
    }

    public async void PlayerHeal()
    {
        if (!IsPlayerTurn) return;

        if (_playerVM.GetCurrentPower() - 5 < 25)
        {
            CurrentTurnMessage = "No puedes descender de 25 de poder.";
            OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
            return; 
        }

        IsPlayerTurn = false; 
        
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
        
        await Task.Delay(1000); // Pequeña pausa para que se lea el cartel

        int enemyDamage = 35;
        bool isEnemyCritical = UnityEngine.Random.Range(0, 100) < 30;

        if (isEnemyCritical)
        {
            enemyDamage += 5; 
            CurrentTurnMessage = $"¡GOLPE CRÍTICO! Garmanar quita {enemyDamage} de vida.";
        }
        else
        {
            CurrentTurnMessage = $"¡Garmanar te ataca y quita {enemyDamage} de vida!";
        }
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);

        // 1. Le decimos a Garmanar que tire su animación de ataque
        _enemyVM.TriggerAttackAnimation();
        
        // 2. Esperamos 1 segundo a que su arma nos alcance
        await Task.Delay(1000);

        // 3. ¡Impacto a Aman! Dispara el Flinch automáticamente en PlayerView
        _playerVM.TakeDamage(enemyDamage);
        
        // 4. Esperamos a que Aman termine de retroceder por el golpe
        await Task.Delay(1500);

        if (_playerVM.IsDefeated)
            EndCombat(false); 
        else
            StartPlayerTurn();
    }

    private void EndCombat(bool playerWon)
    {
        if (playerWon)
            CurrentTurnMessage = "¡VICTORIA! Portal desbloqueado.";
        else
            CurrentTurnMessage = "Has sido derrotado..."; 
        
        OnTurnMessageChanged?.Invoke(CurrentTurnMessage);
        OnCombatEnded?.Invoke(playerWon);
    }
}