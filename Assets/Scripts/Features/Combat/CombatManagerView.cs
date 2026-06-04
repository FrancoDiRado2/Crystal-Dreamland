using UnityEngine;
using UnityEngine.SceneManagement; // NUEVO: Necesario para recargar el nivel
using System.Threading.Tasks;

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración (Lo que se apaga)")]
    public GameObject mainCamera; 
    public GameObject mainHUD;    
    public PlayerView playerView; 

    [Header("Combate (Lo que se prende)")]
    public GameObject combatCamera; 
    public GameObject combatCanvas; 

    [Header("Game Over (NUEVO)")]
    public GameObject gameOverCanvas; // El nuevo canvas de Derrota

    private EnemyViewModel _enemyViewModel;
    private CombatViewModel _combatViewModel; 

    public void Initialize(EnemyViewModel enemyViewModel, CombatViewModel combatViewModel)
    {
        _enemyViewModel = enemyViewModel;
        _combatViewModel = combatViewModel;

        _enemyViewModel.OnCombatStarted += StartCombatTransition;
        // NUEVO: Ahora esta función recibe un bool
        _combatViewModel.OnCombatEnded += EndCombatTransition; 
    }

    private void StartCombatTransition()
    {
        Debug.Log("Iniciando transición visual al combate...");

        if (playerView != null) playerView.enabled = false; 
        if (mainHUD != null) mainHUD.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);

        if (combatCamera != null) combatCamera.SetActive(true);
        if (combatCanvas != null) combatCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // NUEVO: Ahora recibe "playerWon"
    private async void EndCombatTransition(bool playerWon)
    {
        await Task.Delay(3000); 

        // Apagamos la UI de combate en ambos casos
        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            Debug.Log("Victoria: Volviendo a la exploración...");
            
            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            // Devolvemos el control a Aman
            if (playerView != null) playerView.enabled = true; 

            // Ocultamos el mouse de nuevo
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Debug.Log("Derrota: Mostrando Game Over...");
            
            // Si pierde, no apagamos la cámara de combate, solo prendemos el cartel de derrota
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
            
            // Aseguramos que el mouse siga libre para clickear "Reintentar"
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // NUEVO: Función que conectaremos al botón de "Reintentar"
    public void RestartLevel()
    {
        Debug.Log("Reiniciando nivel...");
        // Recarga la escena actual desde cero, reseteando todas las variables y la posición
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (_enemyViewModel != null) _enemyViewModel.OnCombatStarted -= StartCombatTransition;
        if (_combatViewModel != null) _combatViewModel.OnCombatEnded -= EndCombatTransition;
    }
}