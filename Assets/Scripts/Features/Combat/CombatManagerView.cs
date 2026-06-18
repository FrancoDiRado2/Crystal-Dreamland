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
    public RawImage transitionScreen; // 🌟 Esta es la RawImage que usaremos para la sangre de fondo
    public Material swirlMaterial;    

    [Header("UI Victoria Combate")]
    public GameObject victoryPanel; 

    [Header("Game Over / Victoria")]
    public GameObject gameOverCanvas; // 🌟 Tu panel que contiene el TXT y el Botón
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
            // --- VICTORIA (Gemanar muere) ---
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
            // --- GAME OVER (Aman muere) ---
            if (playerView != null) playerView.PlayDeathAnimation(); 
            if (sfxSource != null && defeatMusicSfx != null) sfxSource.PlayOneShot(defeatMusicSfx);

            Time.timeScale = 0.3f;
            if (combatBgmSource != null) StartCoroutine(DistortMusic(combatBgmSource));

            // 1. 🌟 FIX DE CAÍDA: Usamos el truco de la jerarquía para que caiga desde los pies
            if (playerView != null)
            {
                StartCoroutine(HierarchyCollapsePlayer(playerView.transform, 1.5f)); 
            }

            // 2. 🌟 FIX DE UI: Limpiamos y ordenamos la pantalla roja
            if (transitionScreen != null)
            {
                // Limpiamos la RawImage por si tenía el shader de swirl puesto
                transitionScreen.material = null; 
                transitionScreen.texture = null;
                
                // Color rojo lúgubre
                transitionScreen.color = new Color(0.5f, 0f, 0f, 0f); 
                transitionScreen.gameObject.SetActive(true);

                // Mandamos la imagen de sangre AL FONDO de su Canvas
                transitionScreen.transform.SetAsFirstSibling(); 
                transitionScreen.raycastTarget = false; // Que no bloquee clics

                // Fade In de la sangre de fondo
                StartCoroutine(FadeBloodBackground(2.5f));
            }

            Time.timeScale = 0f;
            
            if (gameOverCanvas != null) 
            {
                gameOverCanvas.SetActive(true);
                
                // Mandamos tu menú de Try Again AL FRENTE de todo
                gameOverCanvas.transform.SetAsLastSibling(); 

                CanvasGroup cg = gameOverCanvas.GetComponent<CanvasGroup>();
                if (cg == null) cg = gameOverCanvas.AddComponent<CanvasGroup>();
                
                // Aseguramos que el color del menú Try Again sea limpio (Blanco puro/Original)
                Graphic[] graphics = gameOverCanvas.GetComponentsInChildren<Graphic>();
                foreach (var g in graphics)
                {
                    if (g.gameObject != transitionScreen.gameObject) // No tocar la sangre
                    {
                        Color c = g.color;
                        c.a = graphics is TextMeshProUGUI ? c.a : 1f; // Respetar alphas de textos
                        // Si el botón o el texto se veían rojos, esto los limpia:
                        if (g is Image && g.color.r > 0.8f && g.color.g < 0.2f) g.color = Color.white; 
                    }
                }

                // Fade In ESPECTRAL del menú (limpio)
                StartCoroutine(FadeInGameOverMenu(cg, 1.5f));
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
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            // Opacidad máxima de 0.75f para que no tape todo, se vea lúgubre pero legible
            float alpha = Mathf.Lerp(0f, 0.75f, t / duration); 
            transitionScreen.color = new Color(0.5f, 0f, 0f, alpha);
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
    }

    private IEnumerator HierarchyCollapsePlayer(Transform playerTransform, float duration)
    {
        // 1. Apagar cerebros
        if (playerAnimator != null) playerAnimator.enabled = false;
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. Crear el objeto "Pivote de Pies" invisible
        GameObject pivotGO = new GameObject("DeathPivot_Feet");
        // Lo posicionamos exactamente donde están los pies de Aman (suponiendo ombligo a 1m)
        pivotGO.transform.position = playerTransform.position + Vector3.down * 1f; 
        pivotGO.transform.rotation = playerTransform.rotation;

        // 3. Emparentar (Aman ahora es hija del pivote invisible)
        playerTransform.SetParent(pivotGO.transform);

        // 4. Rotar el PIVOTE (cae como tabla)
        Quaternion startRot = pivotGO.transform.rotation;
        // Rotamos 90 grados hacia atrás (eje X local negativo)
        Quaternion endRot = startRot * Quaternion.Euler(-90f, 0f, 0f); 

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            pivotGO.transform.rotation = Quaternion.Slerp(startRot, endRot, t / duration);
            yield return null;
        }
        
        // Opcional: Soltar jerarquía al terminar
        // playerTransform.SetParent(null);
        // Destroy(pivotGO);
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