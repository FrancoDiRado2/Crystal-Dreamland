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
    public Transform playerCombatSpot; 
    
    // NUEVO: Referencia directa al Animator de Aman para destrabarla
    public Animator playerAnimator; 

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
            
            // Nos aseguramos de decirle al animator que Aman no está caminando
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0f);
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
        await Task.Delay(3000); 

        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            if (playerView != null) 
            {
                playerView.enabled = true; 
                
                // NUEVO: Destrabamos la T-pose forzando el Float a 0 y reproduciendo el estado por defecto
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Speed", 0f);
                    playerAnimator.Play("Idle"); // Asegurate de que el estado en tu Animator se llame "Idle" o cambialo acá
                }
            }

            if (garmanarModel != null) garmanarModel.SetActive(false);
            if (muroInvisible != null) muroInvisible.SetActive(true);

            if (portalStatusText != null) portalStatusText.text = "¡Cross the Door!";
            if (powerStatusText != null) powerStatusText.text = "Free Way";
            if (objectiveText != null) objectiveText.SetActive(false);

            if (_combatViewModel != null) _combatViewModel.ClearTurnMessage(); 

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