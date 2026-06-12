using System;

public class PlayerViewModel
{
    private PlayerModel data;

    public event Action<int> OnPowerChanged; 
    public event Action<int> OnHealthChanged;
    public event Action OnTakeDamageFlinch;
    
    // 🌟 NUEVO: Evento para los cristales
    public event Action<int> OnCrystalsChanged;

    public float WalkSpeed => data.walkSpeed;
    public float SprintSpeed => data.sprintSpeed;
    public float RotationSpeed => data.rotationSpeed;
    public float JumpForce => data.jumpForce;
    public float Gravity => data.gravity;

    public bool IsDefeated => data.currentHealth <= 0; 

    // 🌟 NUEVO: Propiedad para leer los cristales
    public int CrystalsCollected => data.crystalsCollected; 

    public PlayerViewModel(PlayerModel model)
    {
        data = model;
        data.currentPower = 15; 
        data.currentHealth = data.maxHealth;
        data.crystalsCollected = 0; // Setear en 0 al iniciar
    }

    // 🌟 NUEVO: Función de cristales
    public void AddCrystal()
    {
        data.crystalsCollected++;
        OnCrystalsChanged?.Invoke(data.crystalsCollected);
    }

    // TODO ESTO QUEDA INTACTO PARA NO ROMPER AL JEFE
    public void AddPower(int amount)
    {
        data.currentPower += amount;
        OnPowerChanged?.Invoke(data.currentPower);
    }

    public void ConsumePower(int amount)
    {
        data.currentPower -= amount;
        OnPowerChanged?.Invoke(data.currentPower);
    }

    public int GetCurrentPower() 
    { 
        return data.currentPower; 
    }

    public void TakeDamage(int damage)
    {
        data.currentHealth -= damage;
        if (data.currentHealth < 0) data.currentHealth = 0;
        
        OnHealthChanged?.Invoke(data.currentHealth);
        OnTakeDamageFlinch?.Invoke();
    }

    public void Heal(int amount)
    {
        data.currentHealth += amount;
        if (data.currentHealth > data.maxHealth) data.currentHealth = data.maxHealth;
        
        OnHealthChanged?.Invoke(data.currentHealth);
    }
}