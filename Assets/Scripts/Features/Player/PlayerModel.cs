using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerModel", menuName = "Game/Player Model")]
public class PlayerModel : ScriptableObject
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Combat Stats")]
    public int currentPower = 15;
    
    // NUEVO: Agregamos la vida de Aman
    public int maxHealth = 100; 
    public int currentHealth = 100; 
}