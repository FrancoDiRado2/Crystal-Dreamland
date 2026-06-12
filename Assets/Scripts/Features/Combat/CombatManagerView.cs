using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using System.Collections; 
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
    public GameObject victoryPanel; 

    [Header("Game Over / Victoria")]
    public GameObject gameOverCanvas; 
    public GameObject garmanarModel; 
    public GameObject muroInvisible; 
    
    [Header("Textos")]
    public TextMeshProUGUI portalStatusText; 
    public TextMeshProUGUI powerStatusText; 
    public GameObject objectiveText;

    [Header("AUDIO - Combate")]
    public AudioSource globalMusicSource; 
    public AudioSource combatBgmSource;   
    public AudioSource sfxSource;         
    
    public AudioClip combatMusic;         
    public AudioClip transitionSfx;       
    public AudioClip victoryMusicSfx;     
    public AudioClip defeatMusicSfx;      
    public AudioClip enemyDeathSfx;       // <-- 🎵 NUEVO: El grito de muerte de Garmanar

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
        if (transitionScreen != null) transitionScreen.raycastTarget = true;

        if (sfxSource != null && transitionSfx != null) sfxSource.PlayOneShot(transitionSfx);
        
        if (globalMusicSource != null) globalMusicSource.Pause();

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

        if (combatBgmSource != null && combatMusic != null)
        {
            combatBgmSource.clip = combatMusic;
            combatBgmSource.Play();
        }

        Time.timeScale = 1f;
    }

    private void EndCombatTransition(bool playerWon)
    {
        StartCoroutine(EndCombatRoutine(playerWon));
    }

    private IEnumerator EndCombatRoutine(bool playerWon)
    {
        if (combatCanvas != null) combatCanvas.SetActive(false);
        
        // Apagamos la música de combate épica
        if (combatBgmSource != null) combatBgmSource.Stop();

        if (playerWon)
        {
            // === SECUENCIA DE VICTORIA ===
            Time.timeScale = 0.2f; 

            // 🎵 SUENA LA VICTORIA Y EL GRITO DE GARMANAR JUNTOS
            if (sfxSource != null && victoryMusicSfx != null) sfxSource.PlayOneShot(victoryMusicSfx);
            if (sfxSource != null && enemyDeathSfx != null) sfxSource.PlayOneShot(enemyDeathSfx);

            if (victoryPanel != null) victoryPanel.SetActive(true);

            if (garmanarModel != null)
            {
                float t = 0f;
                Vector3 escalaOriginal = garmanarModel.transform.localScale;
                Vector3 posicionOriginal = garmanarModel.transform.position;
                
                while (t < 1.5f)
                {
                    t += Time.unscaledDeltaTime;
                    float progress = t / 1.5f;
                    
                    garmanarModel.transform.localScale = Vector3.Lerp(escalaOriginal, new Vector3(escalaOriginal.x, 0f, escalaOriginal.z), progress);
                    garmanarModel.transform.position = posicionOriginal + (Vector3.down * (progress * 1.5f));
                    garmanarModel.SetActive(!garmanarModel.activeSelf);
                    
                    yield return null;
                }
                garmanarModel.SetActive(false); 
            }

            yield return new WaitForSecondsRealtime(1.5f);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            Time.timeScale = 1f; 

            // 🎵 VOLVEMOS A LA MÚSICA DEL BOSQUE
            if (globalMusicSource != null) globalMusicSource.UnPause();

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
            if (portalStatusText != null) portalStatusText.text = "¡Cross the Gate!";
            if (powerStatusText != null) powerStatusText.text = "Free Way";
            if (objectiveText != null) objectiveText.SetActive(false);
            if (_combatViewModel != null) _combatViewModel.ClearTurnMessage(); 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // === SECUENCIA DE DERROTA ===
            if (playerView != null) playerView.PlayDeathAnimation();

            // 🎵 SUENA LA PISTA DE DERROTA (GAME OVER)
            if (sfxSource != null && defeatMusicSfx != null) sfxSource.PlayOneShot(defeatMusicSfx);

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