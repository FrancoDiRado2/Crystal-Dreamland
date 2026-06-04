using System;

public class EnemyViewModel
{
    private EnemyModel _model;
    
    // Eventos para la UI y la vista
    public event Action<int> OnHealthChanged;
    public event Action OnDefeated;
    public event Action OnCombatStarted;
    public event Action OnNotEnoughPower;

    public bool IsDefeated => _model.IsDefeated;

    public EnemyViewModel(EnemyModel model)
    {
        _model = model;
    }

    public void TryStartCombat(int playerPower)
    {
        if (_model.IsDefeated) return;

        if (playerPower >= _model.RequiredPowerToFight)
        {
            OnCombatStarted?.Invoke();
        }
        else
        {
            OnNotEnoughPower?.Invoke();
        }
    }

    public void TakeDamage(int damage)
    {
        if (_model.IsDefeated) return;

        _model.CurrentHealth -= damage;
        OnHealthChanged?.Invoke(_model.CurrentHealth);

        if (_model.CurrentHealth <= 0)
        {
            _model.CurrentHealth = 0;
            _model.IsDefeated = true;
            OnDefeated?.Invoke();
        }
    }
}