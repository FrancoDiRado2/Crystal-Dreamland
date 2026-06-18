using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Animación Final")]
    public GameObject delayedMenuAnimation; 
    [Tooltip("Cuántos segundos ANTES de que termine de quemarse querés que arranque el humo")]
    public float smokeEarlyStart = 0.5f; // 🌟 NUEVO: El control del Pre-roll

    private Coroutine cinematicCoroutine;

    private void Start()
    {
        if (mainMenuContainer != null) mainMenuContainer.SetActive(false);
        if (blackBackground != null) blackBackground.SetActive(true);
        if (delayedMenuAnimation != null) delayedMenuAnimation.SetActive(false); 

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

            // 1. FADE IN
            yield return FadeCurtainAlpha(1f, 0f, fadeDuration);

            // 2. SUBTÍTULO
            if (subtitles.Length > i && subtitleText != null)
            {
                subtitleText.text = subtitles[i];
            }

            // 3. AUDIO
            if (narratorClips.Length > i && narratorClips[i] != null && narratorSource != null)
            {
                narratorSource.clip = narratorClips[i];
                narratorSource.Play();
            }

            // 4. ESPERAR
            yield return new WaitForSeconds(waitTime);

            // 5. BORRAR SUBTÍTULO
            if (subtitleText != null) subtitleText.text = "";

            // 6. TRANSICIÓN
            if (isLastPanel && burnMaterial != null)
            {
                if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
                if (blackBackground != null) blackBackground.SetActive(false);

                // Se quema la imagen (y prende el humo por dentro un ratito antes)
                yield return BurnImage(burnDuration);
            }
            else
            {
                yield return FadeCurtainAlpha(0f, 1f, fadeDuration);
            }
        }

        EndCinematic();
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
        bool smokePreloaded = false;
        
        // Calculamos en qué segundo tiene que arrancar el humo
        float timeToStartSmoke = Mathf.Max(0f, duration - smokeEarlyStart);

        while (t < duration)
        {
            t += Time.deltaTime;

            // 🌟 Si ya llegamos a ese segundo límite, prendemos el humo silenciosamente
            if (!smokePreloaded && t >= timeToStartSmoke)
            {
                if (delayedMenuAnimation != null) delayedMenuAnimation.SetActive(true);
                smokePreloaded = true;
            }

            float burnValue = Mathf.Lerp(0f, 1f, t / duration);
            burnMaterial.SetFloat("_BurnAmount", burnValue); 
            yield return null;
        }
        
        burnMaterial.SetFloat("_BurnAmount", 1f);
        
        // Seguro por si el tiempo era raro y no se prendió
        if (!smokePreloaded && delayedMenuAnimation != null) delayedMenuAnimation.SetActive(true);
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
        StopAllCoroutines(); 
        if (narratorSource != null) narratorSource.Stop();
        if (subtitleText != null) subtitleText.text = "";
        EndCinematic();
    }

    private void EndCinematic()
    {
        storyboardDisplay.material = null; 
        if (burnMaterial != null) burnMaterial.SetFloat("_BurnAmount", 0f);

        if (cinematicCanvas != null) cinematicCanvas.SetActive(false);
        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
        if (delayedMenuAnimation != null) delayedMenuAnimation.SetActive(true);
    }
}