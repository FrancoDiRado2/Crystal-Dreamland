using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro; 

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración (Lo que se apaga)")]
    public GameObject mainCamera; 
    public GameObject mainHUD;    
    public PlayerView playerView; 
    public Transform playerCombatSpot; // La marca donde aparece Aman

    [Header("Combate (Lo que se prende)")]
    public GameObject combatCamera; 
    public GameObject combatCanvas; 

    [Header("Game Over / Victoria (Detalles finales)")]
    public GameObject gameOverCanvas; 
    public GameObject garmanarModel; 
    public GameObject muroInvisible; 
    
    [Header("Textos del HUD")]
    public TextMeshProUGUI portalStatusText; 
    public TextMeshProUGUI powerStatusText; 
    public GameObject objectiveText;

    private EnemyViewModel _enemyViewModel;
    private CombatViewModel _combatViewModel; 

    public void Initialize(EnemyViewModel enemyViewModel, CombatViewModel combatViewModel)
    {
        _enemyViewModel = enemyViewModel;
        _combatViewModel = combatViewModel;

        _enemyViewModel.OnCombatStarted += StartCombatTransition;
        _combatViewModel.OnCombatEnded += EndCombatTransition; 
    }

    private void StartCombatTransition()
    {
        if (playerView != null) 
        {
            playerView.enabled = false; 
            if (playerCombatSpot != null)
            {
                playerView.TeleportTo(playerCombatSpot);
            }
        }
        
        if (mainHUD != null) mainHUD.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);

        if (combatCamera != null) combatCamera.SetActive(true);
        if (combatCanvas != null) combatCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private async void EndCombatTransition(bool playerWon)
    {
        // Espera 3 segundos para que el usuario vea el resultado
        await Task.Delay(3000); 

        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            // Volver a la exploración
            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            if (playerView != null) playerView.enabled = true; 

            // Efectos de Victoria
            if (garmanarModel != null) garmanarModel.SetActive(false);
            if (muroInvisible != null) muroInvisible.SetActive(true);

            // Actualización de textos
            if (portalStatusText != null) portalStatusText.text = "¡Cross the Door!";
            if (powerStatusText != null) powerStatusText.text = "Free Way";
            if (objectiveText != null) objectiveText.SetActive(false);

            // Limpieza del texto del combate
            if (_combatViewModel != null) _combatViewModel.ClearTurnMessage(); 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Mostrar Game Over
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (_enemyViewModel != null) _enemyViewModel.OnCombatStarted -= StartCombatTransition;
        if (_combatViewModel != null) _combatViewModel.OnCombatEnded -= EndCombatTransition;
    }
}