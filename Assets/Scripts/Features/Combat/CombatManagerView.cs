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
    public RawImage transitionScreen; 
    public Material swirlMaterial;    

    [Header("UI Victoria Combate")]
    public GameObject victoryPanel; 

    [Header("Game Over / Victoria")]
    public GameObject gameOverCanvas; 
    public RawImage bloodScreen; 
    public CanvasGroup gameOverUIGroup; 

    public GameObject garmanarModel; 
    public GameObject muroInvisible; 
    
    [Header("Efectos Final Jefe (Implosión)")]
    public ParticleSystem garmanarDeathParticles; 
    
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
            Time.timeScale = 0.2f; // Cámara lenta épica
            if (sfxSource != null && victoryMusicSfx != null) sfxSource.PlayOneShot(victoryMusicSfx);
            if (sfxSource != null && enemyDeathSfx != null) sfxSource.PlayOneShot(enemyDeathSfx);
            if (combatBgmSource != null) combatBgmSource.Stop();

            if (victoryPanel != null) victoryPanel.SetActive(true);

            if (garmanarModel != null)
            {
                Vector3 originalPos = garmanarModel.transform.position;
                float shakeTime = 0f;
                // Tiembla durante 1.5 segundos
                while (shakeTime < 1.5f) 
                {
                    shakeTime += Time.unscaledDeltaTime;
                    garmanarModel.transform.position = originalPos + (Random.insideUnitSphere * 0.25f);
                    yield return null;
                }
                garmanarModel.transform.position = originalPos;

                // 🌟 MAGIA: Reproducimos las partículas PRIMERO
                if (garmanarDeathParticles != null) 
                {
                    // Lo desvinculamos de Garmanar para que nada lo afecte
                    garmanarDeathParticles.transform.SetParent(null);
                    garmanarDeathParticles.Play();
                }

                // 🌟 FIX: En vez de apagar el GameObject entero, apagamos sus "pieles" (Renderers)
                // Así se vuelve invisible pero sigue existiendo para escupir las partículas
                Renderer[] renderers = garmanarModel.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers) r.enabled = false;
                
                // También apagamos sus colisiones por si acaso
                Collider[] colliders = garmanarModel.GetComponentsInChildren<Collider>();
                foreach (var c in colliders) c.enabled = false;
            }

            // Tiempo muerto eliminado: 2 segundos en slowmo es suficiente para ver la explosión
            yield return new WaitForSecondsRealtime(2f);
            
            if (victoryPanel != null) victoryPanel.SetActive(false);

            Time.timeScale = 1f; // Vuelve a velocidad normal
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

            if (playerView != null)
            {
                StartCoroutine(HierarchyCollapsePlayer(playerView.transform, 1.5f)); 
            }

            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);

            if (gameOverUIGroup != null) 
            {
                gameOverUIGroup.alpha = 0f;
                gameOverUIGroup.interactable = false;
                gameOverUIGroup.blocksRaycasts = false;
            }

            if (bloodScreen != null)
            {
                bloodScreen.color = new Color(0.5f, 0f, 0f, 0f);
                StartCoroutine(FadeBloodBackground(2.5f));
            }

            yield return new WaitForSecondsRealtime(2f);

            Time.timeScale = 0f;
            
            if (gameOverUIGroup != null) 
            {
                StartCoroutine(FadeInGameOverMenu(gameOverUIGroup, 1.5f));
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

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