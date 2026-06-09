using System;

// Fíjate que ya NO hereda de MonoBehaviour
public class PlayerViewModel
{
    private PlayerModel data;

    public event Action<int> OnPowerChanged; 
    public event Action<int> OnHealthChanged;
    
    // NUEVO: Evento para avisarle a la vista que grite de dolor
    public event Action OnTakeDamageFlinch;

    public float WalkSpeed => data.walkSpeed;
    public float SprintSpeed => data.sprintSpeed;
    public float RotationSpeed => data.rotationSpeed;
    public float JumpForce => data.jumpForce;
    public float Gravity => data.gravity;

    // NUEVO: Propiedad para saber si Aman fue derrotada
    public bool IsDefeated => data.currentHealth <= 0; 

    // Ahora recibe los datos a través del constructor
    public PlayerViewModel(PlayerModel model)
    {
        data = model;
        data.currentPower = 15; 
        data.currentHealth = data.maxHealth;
    }

    public void AddPower(int amount)
    {
        data.currentPower += amount;
        OnPowerChanged?.Invoke(data.currentPower);
    }

    // NUEVO: Restar cristales al curarse
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

        // NUEVO: Le avisamos a la Vista que ponga la animación de recibir daño
        OnTakeDamageFlinch?.Invoke();
    }

    // NUEVO: Sumar vida sin pasarse de la vida máxima
    public void Heal(int amount)
    {
        data.currentHealth += amount;
        if (data.currentHealth > data.maxHealth) data.currentHealth = data.maxHealth;
        
        OnHealthChanged?.Invoke(data.currentHealth);
    }
}