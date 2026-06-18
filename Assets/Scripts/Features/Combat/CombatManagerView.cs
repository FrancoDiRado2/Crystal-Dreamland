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

    [Header("Inmersión Combate")]
    public GameObject groundTorch; 
    private bool _playerHadTorch = false; 

    [Header("Transición Estilo Pokémon")]
    public RawImage transitionScreen; // 🌟 ESTO VUELVE A SER SOLO DEL REMOLINO INTACTO
    public Material swirlMaterial;    

    [Header("UI Victoria Combate")]
    public GameObject victoryPanel; 

    [Header("Game Over / Victoria")]
    public GameObject gameOverCanvas; // Tu Canvas apagado de siempre
    [Tooltip("La RawImage roja que pusiste adentro del GameOverCanvas")]
    public RawImage bloodScreen; // 🌟 NUEVO: El fondo rojo exclusivo
    [Tooltip("Un CanvasGroup que contenga tu Texto y Botón de Try Again")]
    public CanvasGroup gameOverUIGroup; // 🌟 NUEVO: Para que los botones aparezcan suavemente

    public GameObject garmanarModel; 
    public GameObject muroInvisible; 
    
    [Header("Efectos Final Jefe (Implosión)")]
    public ParticleSystem garmanarDeathParticles; 
    public Material garmanarDissolveMaterial; 
    
    [Header("Textos")]
    public TextMeshProUGUI portalStatusText; 
    public TextMeshProUGUI portalStatusShadow; 
    public TextMeshProUGUI powerStatusText; 
    public TextMeshProUGUI powerStatusShadow; 
    public GameObject objectiveText;

    [Header("AUDIO - Combate")]
    public AudioSource globalMusicSource; 
    public AudioSource combatBgmSource;   
    public AudioSource sfxSource;         
    
    public AudioClip combatMusic;         
    public AudioClip transitionSfx;       
    public AudioClip victoryMusicSfx;     
    public AudioClip defeatMusicSfx;      
    public AudioClip enemyDeathSfx;       

    private EnemyViewModel _enemyViewModel;
    private CombatViewModel _combatViewModel; 

    public void Initialize(EnemyViewModel enemyViewModel, CombatViewModel combatViewModel)
    {
        _enemyViewModel = enemyViewModel;
        _combatViewModel = combatViewModel;

        _enemyViewModel.OnCombatStarted += StartCombatTransition;
        _combatViewModel.OnCombatEnded += EndCombatTransition; 
        
        if (globalMusicSource != null)
        {
            globalMusicSource.volume = 0.8f; 
        }
    }

    private void StartCombatTransition()
    {
        StartCoroutine(StartCombatRoutine());
    }

    private IEnumerator StartCombatRoutine()
    {
        if (LevelNarrator.Instance != null) LevelNarrator.Instance.CortarNarracion();

        if (transitionScreen != null) transitionScreen.raycastTarget = true;
        if (sfxSource != null && transitionSfx != null) sfxSource.PlayOneShot(transitionSfx);
        
        if (globalMusicSource != null) StartCoroutine(FadeOutMusic(globalMusicSource, 1f));
        
        Time.timeScale = 0f;

        if (playerView != null) 
        {
            _playerHadTorch = playerView.IsTorchActive;
            if (_playerHadTorch) playerView.ForceTorchState(false);

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

        if (groundTorch != null) groundTorch.SetActive(_playerHadTorch);

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
            combatBgmSource.pitch = 1f; 
            combatBgmSource.volume = 0.6f;
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
        if (groundTorch != null) groundTorch.SetActive(false);

        if (playerWon)
        {
            // --- VICTORIA ---
            Time.timeScale = 0.2f; 
            if (sfxSource != null && victoryMusicSfx != null) sfxSource.PlayOneShot(victoryMusicSfx);
            if (sfxSource != null && enemyDeathSfx != null) sfxSource.PlayOneShot(enemyDeathSfx);
            if (combatBgmSource != null) combatBgmSource.Stop();

            if (victoryPanel != null) victoryPanel.SetActive(true);

            if (garmanarModel != null)
            {
                Vector3 originalPos = garmanarModel.transform.position;
                float shakeTime = 0f;
                while (shakeTime < 0.5f) 
                {
                    shakeTime += Time.unscaledDeltaTime;
                    garmanarModel.transform.position = originalPos + (Random.insideUnitSphere * 0.2f);
                    yield return null;
                }
                garmanarModel.transform.position = originalPos;

                if (garmanarDeathParticles != null) garmanarDeathParticles.Play();

                if (garmanarDissolveMaterial != null)
                {
                    Renderer[] renderers = garmanarModel.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers) r.material = garmanarDissolveMaterial;

                    float t = 0f;
                    while (t < 2f) 
                    {
                        t += Time.unscaledDeltaTime;
                        garmanarDissolveMaterial.SetFloat("_BurnAmount", Mathf.Lerp(0f, 1f, t / 2f));
                        yield return null;
                    }
                }
                garmanarModel.SetActive(false); 
            }

            yield return new WaitForSecondsRealtime(2.5f);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            Time.timeScale = 1f; 
            if (globalMusicSource != null) 
            {
                globalMusicSource.UnPause();
                globalMusicSource.volume = 0.8f; 
            }

            if (combatCamera != null) combatCamera.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(true);
            if (mainHUD != null) mainHUD.SetActive(true);

            if (playerView != null) 
            {
                playerView.enabled = true; 
                if (_playerHadTorch) playerView.ForceTorchState(true);

                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Speed", 0f);
                    playerAnimator.Play("Idle"); 
                }
            }

            if (muroInvisible != null) muroInvisible.SetActive(true);
            
            if (portalStatusText != null) 
            {
                portalStatusText.text = "¡Cross the Gate!";
                if (portalStatusShadow != null) portalStatusShadow.text = portalStatusText.text;
            }
            if (powerStatusText != null) 
            {
                powerStatusText.text = "Free Way";
                if (powerStatusShadow != null) powerStatusShadow.text = powerStatusText.text;
            }
            if (objectiveText != null) objectiveText.SetActive(false);
            if (_combatViewModel != null) _combatViewModel.ClearTurnMessage(); 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // --- GAME OVER ---
            if (playerView != null) playerView.PlayDeathAnimation(); 
            if (sfxSource != null && defeatMusicSfx != null) sfxSource.PlayOneShot(defeatMusicSfx);

            Time.timeScale = 0.3f;
            if (combatBgmSource != null) StartCoroutine(DistortMusic(combatBgmSource));

            // Caída de Aman como tronco
            if (playerView != null)
            {
                StartCoroutine(HierarchyCollapsePlayer(playerView.transform, 1.5f)); 
            }

            // Prendemos el Canvas que siempre tuviste
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);

            // Escondemos los botones temporalmente
            if (gameOverUIGroup != null) 
            {
                gameOverUIGroup.alpha = 0f;
                gameOverUIGroup.interactable = false;
                gameOverUIGroup.blocksRaycasts = false;
            }

            // Hacemos el fade in de la sangre de forma independiente
            if (bloodScreen != null)
            {
                bloodScreen.color = new Color(0.5f, 0f, 0f, 0f);
                StartCoroutine(FadeBloodBackground(2.5f));
            }

            // Esperamos un momento trágico mientras ella cae (tiempo real)
            yield return new WaitForSecondsRealtime(2f);

            Time.timeScale = 0f;
            
            // Aparecen los textos inmaculados
            if (gameOverUIGroup != null) 
            {
                StartCoroutine(FadeInGameOverMenu(gameOverUIGroup, 1.5f));
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- CORRUTINAS DE DRAMA ---
    private IEnumerator DistortMusic(AudioSource source)
    {
        float t = 0f;
        while (t < 2f)
        {
            t += Time.unscaledDeltaTime;
            source.pitch = Mathf.Lerp(1f, 0.4f, t / 2f); 
            source.volume = Mathf.Lerp(0.6f, 0.1f, t / 2f); 
            yield return null;
        }
        source.Stop();
    }

    private IEnumerator FadeBloodBackground(float duration)
    {
        if (bloodScreen == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 0.75f, t / duration); 
            bloodScreen.color = new Color(0.5f, 0f, 0f, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeInGameOverMenu(CanvasGroup cg, float duration)
    {
        cg.alpha = 0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator HierarchyCollapsePlayer(Transform playerTransform, float duration)
    {
        if (playerAnimator != null) playerAnimator.enabled = false;
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        GameObject pivotGO = new GameObject("DeathPivot_Feet");
        pivotGO.transform.position = playerTransform.position + Vector3.down * 1f; 
        pivotGO.transform.rotation = playerTransform.rotation;

        playerTransform.SetParent(pivotGO.transform);

        Quaternion startRot = pivotGO.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(-90f, 0f, 0f); 

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            pivotGO.transform.rotation = Quaternion.Slerp(startRot, endRot, t / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOutMusic(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        source.Pause();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (_enemyViewModel != null) _enemyViewModel.OnCombatStarted -= StartCombatTransition;
        if (_combatViewModel != null) _combatViewModel.OnCombatEnded -= EndCombatTransition;
    }
}