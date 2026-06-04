public class EnemyModel
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    
    // NUEVO: Estadísticas de combate del jefe
    public int baseDamage = 25;
    public int criticalChance = 30; // 30% de probabilidad
    public int criticalBonus = 5;   // Cuánto suma el crítico
}