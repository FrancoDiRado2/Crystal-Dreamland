using UnityEngine;
using System;

public class PlayerViewModel : MonoBehaviour
{
    [SerializeField] private PlayerModel data;

    // Eventos para la UI y otros sistemas lógicos
    public event Action<int> OnPowerChanged; 

    // Propiedades expuestas para que el View las lea en tiempo real
    public float WalkSpeed => data.walkSpeed;
    public float SprintSpeed => data.sprintSpeed;
    public float RotationSpeed => data.rotationSpeed;
    public float JumpForce => data.jumpForce;
    public float Gravity => data.gravity;

    private void Awake()
    {
        // Se asegura que el poder arranque en 10 cada vez que ejecutás la escena
        data.currentPower = 15; 
    }

    // Método para que llamen los cristales
    public void AddPower(int amount)
    {
        data.currentPower += amount;
        Debug.Log($"Poder total: {data.currentPower}");
        OnPowerChanged?.Invoke(data.currentPower);
    }

    public int GetCurrentPower() 
    { 
        return data.currentPower; 
    }
}