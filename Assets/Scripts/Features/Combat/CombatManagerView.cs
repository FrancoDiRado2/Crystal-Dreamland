using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections; //Librería de Corrutinas
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

    [Header("UI Victoria Combate")]
    public GameObject victoryPanel; // <-- ¡NUEVO!: Arrastrá acá tu cartel de VICTORY

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

    private void StartCombatTransition()
    {
        StartCoroutine(StartCombatRoutine());
    }

    private IEnumerator StartCombatRoutine()
    {
        // Nos aseguramos de que el panel de transición reciba clics/capturas al inicio
        if (transitionScreen != null) transitionScreen.raycastTarget = true;

        Time.timeScale = 0f;

        if (playerView != null) 
        {
            playerView.enabled = false; 
            if (playerCombatSpot != null) playerView.TeleportTo(playerCombatSpot);
            if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
        }
        if (mainHUD != null) mainHUD.SetActive(false);

        RenderTexture snapshot = new RenderTexture(Screen.width, Screen.height, 24);
        Camera activeCam = Camera.main; 
        if (activeCam != null)
        {
            activeCam.targetTexture = snapshot;
            activeCam.Render();
            activeCam.targetTexture = null;
        }

        transitionScreen.texture = snapshot;
        transitionScreen.material = swirlMaterial; 
        transitionScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        float duration = 1.2f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float str = Mathf.Lerp(0f, 15f, elapsed / duration);
            swirlMaterial.SetFloat("_SwirlStrength", str);
            yield return null; 
        }
        swirlMaterial.SetFloat("_SwirlStrength", 15f);

        if (mainCamera != null) mainCamera.SetActive(false); 
        if (combatCamera != null) combatCamera.SetActive(true);

        Camera cCam = combatCamera.GetComponent<Camera>();
        if (cCam != null)
        {
            cCam.targetTexture = snapshot;
            cCam.Render();
            cCam.targetTexture = null;
        }
        transitionScreen.texture = snapshot;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float str = Mathf.Lerp(15f, 0f, elapsed / duration);
            swirlMaterial.SetFloat("_SwirlStrength", str);
            yield return null;
        }
        swirlMaterial.SetFloat("_SwirlStrength", 0f);

        transitionScreen.gameObject.SetActive(false);
        snapshot.Release();
        
        if (combatCanvas != null) combatCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
    }

    private void EndCombatTransition(bool playerWon)
    {
        StartCoroutine(EndCombatRoutine(playerWon));
    }

        private IEnumerator EndCombatRoutine(bool playerWon)
    {
        // 1. Apagamos la interfaz de combate vieja (botones de ataque)
        if (combatCanvas != null) combatCanvas.SetActive(false);

        if (playerWon)
        {
            // === SECUENCIA DE VICTORIA ===
            Time.timeScale = 0.2f; // Cámara lenta épica para el impacto final

            // 2. ¡Prendemos el cartel! Al estar en el TransitionCanvas, no se apaga.
            if (victoryPanel != null) victoryPanel.SetActive(true);

            if (garmanarModel != null)
            {
                float t = 0f;
                Vector3 escalaOriginal = garmanarModel.transform.localScale;
                Vector3 posicionOriginal = garmanarModel.transform.position;
                
                while (t < 1.5f)
                {
                    t += Time.unscaledDeltaTime; // Progreso en tiempo real
                    float progress = t / 1.5f;
                    
                    // Efecto Pokémon: Aplastar e hundir hacia el piso
                    garmanarModel.transform.localScale = Vector3.Lerp(escalaOriginal, new Vector3(escalaOriginal.x, 0f, escalaOriginal.z), progress);
                    garmanarModel.transform.position = posicionOriginal + (Vector3.down * (progress * 1.5f));
                    
                    // Titileo
                    garmanarModel.SetActive(!garmanarModel.activeSelf);
                    
                    yield return null;
                }
                garmanarModel.SetActive(false); 
            }

            // 3. ¡CORRECCIÓN CLAVE!: Esperamos 1.5 segundos reales para que el jugador lea el cartel
            // sin importar que el mundo 3D vaya a cámara lenta.
            yield return new WaitForSecondsRealtime(1.5f);
            
            if (victoryPanel != null) victoryPanel.SetActive(false);

            Time.timeScale = 1f; // Volvemos el tiempo a la normalidad

            // 4. Encendemos cámaras y HUD de exploración del mapa
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
            // === SECUENCIA DE DERROTA ===
            if (transitionScreen != null)
            {
                transitionScreen.material = null; 
                transitionScreen.texture = null;
                transitionScreen.color = new Color(0.4f, 0f, 0f, 0f); 
                transitionScreen.gameObject.SetActive(true);

                float t = 0f;
                while (t < 2f) 
                {
                    t += Time.unscaledDeltaTime;
                    float alpha = Mathf.Lerp(0f, 0.85f, t / 2f); 
                    transitionScreen.color = new Color(0.4f, 0f, 0f, alpha);
                    Time.timeScale = Mathf.Lerp(1f, 0f, t / 2f); 
                    yield return null;
                }

                transitionScreen.raycastTarget = false;
            }

            Time.timeScale = 0f;
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