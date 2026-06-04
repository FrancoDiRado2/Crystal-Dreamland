using UnityEngine;
using System;

public class PlayerViewModel : MonoBehaviour
{
    [SerializeField] private PlayerModel data;

    public event Action<int> OnPowerChanged; 
    public event Action<int> OnHealthChanged; // NUEVO: Para que la barra roja se entere

    public float WalkSpeed => data.walkSpeed;
    public float SprintSpeed => data.sprintSpeed;
    public float RotationSpeed => data.rotationSpeed;
    public float JumpForce => data.jumpForce;
    public float Gravity => data.gravity;

    private void Awake()
    {
        data.currentPower = 15; 
        data.currentHealth = data.maxHealth; // Resetea la vida al empezar el juego
    }

    public void AddPower(int amount)
    {
        data.currentPower += amount;
        OnPowerChanged?.Invoke(data.currentPower);
    }

    public int GetCurrentPower() 
    { 
        return data.currentPower; 
    }

    // NUEVO: Método para que el Jefe le pegue a Aman
    public void TakeDamage(int damage)
    {
        data.currentHealth -= damage;
        if (data.currentHealth < 0) data.currentHealth = 0;
        
        OnHealthChanged?.Invoke(data.currentHealth);
    }
}