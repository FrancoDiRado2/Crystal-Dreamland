public class EnemyModel
{
    public int MaxHealth { get; set; } = 100;
    public int CurrentHealth { get; set; } = 100;
    public bool IsDefeated { get; set; } = false;
    
    // Lo igualamos a lo que pide tu HUD
    public int RequiredPowerToFight { get; set; } = 25; 
}