using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections; // ¡Librería clave para las Corrutinas!
using TMPro; 

public class CombatManagerView : MonoBehaviour
{
    [Header("Exploración")]
    public GameObject mainCamera; 
    public GameObject mainHUD;    
    public PlayerView playerView; 
    public Transform playerCombatSpot; 
    public Animator playerAnimator; 

    [Header("Combate")]
    public GameObject combatCamera; 
    public GameObject combatCanvas; 

    [Header("Transición Estilo Pokémon")]
    public RawImage transitionScreen; 
    public Material swirlMaterial;    

    [Header("Game Over / Victoria")]
    public GameObject gameOverCanvas; 
    public GameObject garmanarModel; 
    public GameObject muroInvisible; 
    
    [Header("Textos")]
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

    // El evento dispara esto, y esto arranca la secuencia animada
    private void StartCombatTransition()
    {
        StartCoroutine(StartCombatRoutine());
    }

    // CORRUTINA: Maneja la animación cuadro por cuadro sin fallar
    // CORRUTINA: Maneja la animación cuadro por cuadro sin fallar
    // CORRUTINA: Maneja la animación cuadro por cuadro sin fallar
    private IEnumerator StartCombatRoutine()
    {
        // ¡MAGIA!: Congelamos todo el universo (físicas, cámaras, gravedad)
        Time.timeScale = 0f;

        // 1. Apagamos controles y HUD
        if (playerView != null) 
        {
            playerView.enabled = false; 
            if (playerCombatSpot != null) playerView.TeleportTo(playerCombatSpot);
            if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
        }
        if (mainHUD != null) mainHUD.SetActive(false);

        // 2. SACAMOS FOTO
        RenderTexture snapshot = new RenderTexture(Screen.width, Screen.height, 24);
        Camera activeCam = Camera.main; 
        if (activeCam != null)
        {
            activeCam.targetTexture = snapshot;
            activeCam.Render();
            activeCam.targetTexture = null;
        }

        // 3. ENCHUFAMOS TODO AL MATERIAL ORIGINAL
        transitionScreen.texture = snapshot;
        transitionScreen.material = swirlMaterial; 
        transitionScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        float duration = 1.2f;
        
        // 4. ENROSCAMOS
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Esto ignora la pausa del TimeScale
            float str = Mathf.Lerp(0f, 15f, elapsed / duration);
            
            swirlMaterial.SetFloat("_SwirlStrength", str);
            yield return null; 
        }
        swirlMaterial.SetFloat("_SwirlStrength", 15f);

        // 5. Cambio de cámaras (Por detrás de la espiral)
        if (mainCamera != null) mainCamera.SetActive(false); 
        if (combatCamera != null) combatCamera.SetActive(true);

        // 6. SACAMOS FOTO DE COMBATE
        Camera cCam = combatCamera.GetComponent<Camera>();
        if (cCam != null)
        {
            cCam.targetTexture = snapshot;
            cCam.Render();
            cCam.targetTexture = null;
        }
        transitionScreen.texture = snapshot;

        // 7. DESENROSCAMOS
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float str = Mathf.Lerp(15f, 0f, elapsed / duration);
            
            swirlMaterial.SetFloat("_SwirlStrength", str);
            yield return null;
        }
        swirlMaterial.SetFloat("_SwirlStrength", 0f);

        // 8. Limpieza
        transitionScreen.gameObject.SetActive(false);
        snapshot.Release();
        
        if (combatCanvas != null) combatCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ¡MAGIA!: Descongelamos el mundo para que empiece la pelea
        Time.timeScale = 1f;
    }

    private void EndCombatTransition(bool playerWon)
    {
        StartCoroutine(EndCombatRoutine(playerWon));
    }

    private IEnumerator EndCombatRoutine(bool playerWon)
    {
        yield return new WaitForSeconds(3f); // Pausa para ver la victoria/derrota

        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
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