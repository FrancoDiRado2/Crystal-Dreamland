using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Video; 

public class StoryboardCinematic : MonoBehaviour
{
    [Header("Contenedores Principales")]
    public GameObject cinematicCanvas;
    public GameObject mainMenuContainer; 

    [Header("Elementos de la Cinemática")]
    public GameObject blackBackground; 
    public Image storyboardDisplay; 
    public Image fadeCurtain; 
    public TextMeshProUGUI subtitleText; 

    [Header("Secuencia (Imágenes, Audios y Textos)")]
    public Sprite[] storyboardPanels; 
    public AudioClip[] narratorClips; 
    
    [TextArea(2, 5)] 
    public string[] subtitles; 

    [Header("Audio Source")]
    public AudioSource narratorSource;

    [Header("Efecto Papiro Quemado")]
    public Material burnMaterial; 
    public float burnDuration = 2.5f;

    [Header("Tiempos y Zoom")]
    public float fadeDuration = 2.0f; 
    public float minDisplayDuration = 3.5f; 
    public float zoomAmount = 1.08f; 

    [Header("Efecto del Humo (Video Separado)")]
    [Tooltip("El VideoPlayer que tiene SOLAMENTE el humo.")]
    public VideoPlayer smokeVideoPlayer; 
    [Tooltip("Segundo exacto donde hace el 'soplo' épico (Ej: 4.5)")]
    public double smokeStartTime = 0f; 
    [Tooltip("Cuántos segundos ANTES de terminar el fuego arranca el humo")]
    public float smokeEarlyStart = 0.5f;

    [Header("Efecto 'Soplo' de Botones")]
    public CanvasGroup menuButtonsGroup; 
    public float buttonFadeDelay = 0.5f; 
    public float buttonFadeDuration = 1.0f; 

    private Coroutine cinematicCoroutine;
    private bool buttonsDone = false;
    private bool isSkipping = false;

    private void Start()
    {
        // 🌟 NUEVO: Ocultamos el cursor al arrancar la peli
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (storyboardDisplay != null) storyboardDisplay.gameObject.SetActive(true);
        if (fadeCurtain != null) fadeCurtain.gameObject.SetActive(true);

        if (mainMenuContainer != null) mainMenuContainer.SetActive(false);
        if (blackBackground != null) blackBackground.SetActive(true);

        if (menuButtonsGroup != null)
        {
            menuButtonsGroup.alpha = 0f;
            menuButtonsGroup.interactable = false;
            menuButtonsGroup.blocksRaycasts = false;
        }

        SetCurtainAlpha(1f);
        if (subtitleText != null) subtitleText.text = "";

        storyboardDisplay.material = null; 
        if (burnMaterial != null) burnMaterial.SetFloat("_BurnAmount", 0f);

        cinematicCoroutine = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                SkipCinematic();
            }
        }
    }

    private IEnumerator PlaySequence()
    {
        Color displayColor = storyboardDisplay.color;
        displayColor.a = 1f;
        storyboardDisplay.color = displayColor;

        for (int i = 0; i < storyboardPanels.Length; i++)
        {
            SetCurtainAlpha(1f);
            storyboardDisplay.sprite = storyboardPanels[i];
            storyboardDisplay.transform.localScale = Vector3.one; 

            float waitTime = minDisplayDuration; 
            if (narratorClips.Length > i && narratorClips[i] != null && narratorSource != null)
            {
                waitTime = Mathf.Max(minDisplayDuration, narratorClips[i].length + 0.5f);
            }

            bool isLastPanel = (i == storyboardPanels.Length - 1);
            float currentOutDuration = (isLastPanel && burnMaterial != null) ? burnDuration : fadeDuration;
            
            float totalSlideDuration = fadeDuration + waitTime + currentOutDuration;
            StartCoroutine(ZoomImage(totalSlideDuration));

            yield return FadeCurtainAlpha(1f, 0f, fadeDuration);

            if (subtitles.Length > i && subtitleText != null) subtitleText.text = subtitles[i];

            if (narratorClips.Length > i && narratorClips[i] != null && narratorSource != null)
            {
                narratorSource.clip = narratorClips[i];
                narratorSource.Play();
            }

            yield return new WaitForSeconds(waitTime);

            if (subtitleText != null) subtitleText.text = "";

            if (isLastPanel && burnMaterial != null)
            {
                if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
                if (blackBackground != null) blackBackground.SetActive(false);

                if (smokeVideoPlayer != null) smokeVideoPlayer.Prepare();

                yield return BurnImage(burnDuration);

                if (storyboardDisplay != null) storyboardDisplay.gameObject.SetActive(false);
                yield return new WaitUntil(() => buttonsDone);
            }
            else
            {
                yield return FadeCurtainAlpha(0f, 1f, fadeDuration);
            }
        }

        EndCinematic();
    }

    private IEnumerator FadeInButtonsRoutine()
    {
        buttonsDone = false;
        if (menuButtonsGroup != null)
        {
            menuButtonsGroup.alpha = 0f;
            menuButtonsGroup.interactable = false;
            menuButtonsGroup.blocksRaycasts = false;

            yield return new WaitForSeconds(buttonFadeDelay);

            float t = 0f;
            while (t < buttonFadeDuration)
            {
                t += Time.deltaTime;
                menuButtonsGroup.alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
                yield return null;
            }
            
            menuButtonsGroup.alpha = 1f;
            menuButtonsGroup.interactable = true;
            menuButtonsGroup.blocksRaycasts = true;
        }
        buttonsDone = true;
    }

    private IEnumerator ZoomImage(float duration)
    {
        float t = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(zoomAmount, zoomAmount, 1f);

        while (t < duration)
        {
            t += Time.deltaTime;
            storyboardDisplay.transform.localScale = Vector3.Lerp(startScale, endScale, t / duration);
            yield return null;
        }
    }

    private IEnumerator BurnImage(float duration)
    {
        storyboardDisplay.material = burnMaterial;
        float t = 0f;
        bool elementsPreloaded = false;
        
        float timeToStartElements = Mathf.Max(0f, duration - smokeEarlyStart);

        while (t < duration)
        {
            t += Time.deltaTime;

            if (!elementsPreloaded && t >= timeToStartElements)
            {
                if (smokeVideoPlayer != null && !isSkipping) 
                {
                    smokeVideoPlayer.time = smokeStartTime; 
                    smokeVideoPlayer.Play();
                }
                StartCoroutine(FadeInButtonsRoutine());
                elementsPreloaded = true;
            }

            float burnValue = Mathf.Lerp(0f, 1f, t / duration);
            burnMaterial.SetFloat("_BurnAmount", burnValue); 
            yield return null;
        }
        
        burnMaterial.SetFloat("_BurnAmount", 1f);
        
        if (!elementsPreloaded) 
        {
            if (smokeVideoPlayer != null && !isSkipping) 
            {
                smokeVideoPlayer.time = smokeStartTime;
                smokeVideoPlayer.Play();
            }
            StartCoroutine(FadeInButtonsRoutine());
        }
    }

    private IEnumerator FadeCurtainAlpha(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCurtain == null) yield break;

        float t = 0f;
        Color color = fadeCurtain.color;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            fadeCurtain.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        fadeCurtain.color = color;
    }

    private void SetCurtainAlpha(float alpha)
    {
        if (fadeCurtain != null)
        {
            Color c = fadeCurtain.color;
            c.a = alpha;
            fadeCurtain.color = c;
        }
    }

    private void SkipCinematic()
    {
        if (isSkipping) return; 
        isSkipping = true;

        StopAllCoroutines(); 
        if (narratorSource != null) narratorSource.Stop();
        if (subtitleText != null) subtitleText.text = "";
        
        EndCinematic();
    }

    private void EndCinematic()
    {
        // 🌟 NUEVO: Liberamos la manito medieval cuando termina todo
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        storyboardDisplay.material = null; 
        if (burnMaterial != null) burnMaterial.SetFloat("_BurnAmount", 0f);

        if (storyboardDisplay != null) storyboardDisplay.gameObject.SetActive(false);
        if (fadeCurtain != null) fadeCurtain.gameObject.SetActive(false);
        if (blackBackground != null) blackBackground.SetActive(false);
        if (cinematicCanvas != null) cinematicCanvas.SetActive(false);

        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
        
        if (smokeVideoPlayer != null) 
        {
            if (isSkipping)
            {
                smokeVideoPlayer.time = 0f; 
            }
            smokeVideoPlayer.Play();
        }

        if (menuButtonsGroup != null)
        {
            menuButtonsGroup.alpha = 1f; 
            menuButtonsGroup.interactable = true;
            menuButtonsGroup.blocksRaycasts = true;
        }
    }
}