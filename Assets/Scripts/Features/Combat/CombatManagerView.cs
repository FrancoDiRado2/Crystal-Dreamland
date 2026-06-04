using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro; // NUEVO: Para cambiar los textos

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración (Lo que se apaga)")]
    public GameObject mainCamera; 
    public GameObject mainHUD;    
    public PlayerView playerView; 

    [Header("Combate (Lo que se prende)")]
    public GameObject combatCamera; 
    public GameObject combatCanvas; 

    [Header("Game Over / Victoria (Detalles finales)")]
    public GameObject gameOverCanvas; 
    public GameObject garmanarModel; // NUEVO: Para desaparecerlo
    public GameObject muroInvisible; // NUEVO: Para bloquear la vuelta al bosque
    
    // NUEVO: Los textos del HUD que queremos cambiar al ganar
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
        if (playerView != null) playerView.enabled = false; 
        if (mainHUD != null) mainHUD.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);

        if (combatCamera != null) combatCamera.SetActive(true);
        if (combatCanvas != null) combatCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private async void EndCombatTransition(bool playerWon)
    {
        await Task.Delay(3000); 

        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            if (playerView != null) playerView.enabled = true; 

            // 1. DESAPARECER A GARMANAR
            if (garmanarModel != null) garmanarModel.SetActive(false);

            // 2. PRENDER EL MURO INVISIBLE
            if (muroInvisible != null) muroInvisible.SetActive(true);

            // 3. CAMBIAR LOS TEXTOS DEL HUD
            if (portalStatusText != null) portalStatusText.text = "¡Cross the Door!";
            if (powerStatusText != null) powerStatusText.text = "Free Way";
            if (objectiveText != null) objectiveText.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
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