using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro; 
using UnityEngine.Video; // LIBRERÍA NECESARIA PARA LOS VIDEOS

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración (Lo que se apaga)")]
    public GameObject mainCamera; 
    public GameObject mainHUD;    
    public PlayerView playerView; 
    public Transform playerCombatSpot; 
    public Animator playerAnimator; 

    [Header("Combate (Lo que se prende)")]
    public GameObject combatCamera; 
    public GameObject combatCanvas; 

    [Header("Cinemáticas de Video")]
    public GameObject videoScreen; // El RawImage que va a tapar la pantalla
    public VideoPlayer introVideo; // Reproductor del video de entrada
    public VideoPlayer outroVideo; // Reproductor del video de victoria

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

    // Cambiado a async void para poder esperar al video
    private async void StartCombatTransition()
    {
        // 1. Apagamos controles del jugador para que no se mueva en el fondo
        if (playerView != null) 
        {
            playerView.enabled = false; 
            if (playerCombatSpot != null)
            {
                playerView.TeleportTo(playerCombatSpot);
            }
            
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0f);
            }
        }
        
        if (mainHUD != null) mainHUD.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);

        // 2. PRENDEMOS EL VIDEO DE INTRO
        if (videoScreen != null && introVideo != null)
        {
            videoScreen.SetActive(true);
            introVideo.Play();

            // Esperamos los milisegundos exactos que dure el video
            int videoDurationMs = (int)(introVideo.clip.length * 1000);
            await Task.Delay(videoDurationMs);

            videoScreen.SetActive(false); // Apagamos el telón
        }

        // 3. Empieza el combate
        if (combatCamera != null) combatCamera.SetActive(true);
        if (combatCanvas != null) combatCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private async void EndCombatTransition(bool playerWon)
    {
        // Pausa original para ver el cartel de Victoria/Derrota
        await Task.Delay(3000); 

        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            // PRENDEMOS EL VIDEO DE VICTORIA
            if (videoScreen != null && outroVideo != null)
            {
                videoScreen.SetActive(true);
                outroVideo.Play();

                int outroDurationMs = (int)(outroVideo.clip.length * 1000);
                await Task.Delay(outroDurationMs);

                videoScreen.SetActive(false);
            }

            // Devolvemos al jugador a la normalidad
            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            if (playerView != null) 
            {
                playerView.enabled = true; 
                
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Speed", 0f);
                    playerAnimator.Play("Idle"); 
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