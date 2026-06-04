using UnityEngine;

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración (Lo que se apaga)")]
    public GameObject mainCamera; // La cámara de Aman
    public GameObject mainHUD;    // El Canvas normal (minimapa, stats)
    public PlayerView playerView; // El script de Aman para frenarla

    [Header("Combate (Lo que se prende)")]
    public GameObject combatCamera; // La cámara fija de lado
    public GameObject combatCanvas; // La UI con los botones de ataque

    private EnemyViewModel _enemyViewModel;

    // Lo inyectamos desde el Bootstrapper
    public void Initialize(EnemyViewModel enemyViewModel)
    {
        _enemyViewModel = enemyViewModel;
        _enemyViewModel.OnCombatStarted += StartCombatTransition;
    }

    private void StartCombatTransition()
    {
        Debug.Log("Iniciando transición visual al combate...");

        // 1. Apagamos el mundo de exploración
        if (playerView != null) playerView.enabled = false; // Aman deja de recibir inputs de caminar
        if (mainHUD != null) mainHUD.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);

        // 2. Prendemos el escenario de duelo
        if (combatCamera != null) combatCamera.SetActive(true);
        if (combatCanvas != null) combatCanvas.SetActive(true);

        // 3. Liberamos el mouse para que puedas clickear los botones de ataque
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (_enemyViewModel != null)
        {
            _enemyViewModel.OnCombatStarted -= StartCombatTransition;
        }
    }
}