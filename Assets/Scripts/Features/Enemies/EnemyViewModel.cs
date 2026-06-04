using System;
using UnityEngine; 

public class EnemyViewModel
{
    private EnemyModel _model;

    // --- EVENTOS ---
    public event Action<int> OnHealthChanged;
    public event Action OnDefeated;
    public event Action OnNotEnoughPower; 
    public event Action OnCombatStarted; 

    public bool IsDefeated => _model.currentHealth <= 0;

    public EnemyViewModel(EnemyModel model)
    {
        _model = model;
        _model.currentHealth = _model.maxHealth;
    }

    // --- MÉTODOS PARA COMUNICARSE CON LA VISTA ---

    // ¡ACÁ ESTÁ EL MÉTODO QUE FALTABA!
    // Evalúa si Aman tiene el poder necesario y dispara el evento correspondiente
    public void TryStartCombat(int playerPower)
    {
        if (playerPower >= 25)
        {
            OnCombatStarted?.Invoke();
        }
        else
        {
            OnNotEnoughPower?.Invoke();
        }
    }

    public void NotifyNotEnoughPower()
    {
        OnNotEnoughPower?.Invoke();
    }

    public void StartCombat()
    {
        OnCombatStarted?.Invoke();
    }

    // --- LÓGICA DE DAÑO ---

    public void TakeDamage(int damage)
    {
        if (IsDefeated) return;

        _model.currentHealth -= damage;
        if (_model.currentHealth < 0) _model.currentHealth = 0;

        OnHealthChanged?.Invoke(_model.currentHealth);

        if (_model.currentHealth == 0)
        {
            OnDefeated?.Invoke();
        }
    }

    public (int damage, bool isCritical) GetAttackDamage()
    {
        int finalDamage = _model.baseDamage;
        bool isCritical = UnityEngine.Random.Range(0, 100) < _model.criticalChance;

        if (isCritical)
        {
            finalDamage += _model.criticalBonus;
        }

        return (finalDamage, isCritical); 
    }
}