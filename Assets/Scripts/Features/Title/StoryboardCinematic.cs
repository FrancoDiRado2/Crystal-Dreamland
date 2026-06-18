using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class StoryboardCinematic : MonoBehaviour
{
    [Header("Contenedores Principales")]
    [Tooltip("El Canvas completo de esta cinemática")]
    public GameObject cinematicCanvas;
    [Tooltip("El objeto Padre o Canvas de tu Menú Principal normal")]
    public GameObject mainMenuContainer; 

    [Header("Elementos de la Cinemática")]
    [Tooltip("La imagen donde se muestran tus dibujos")]
    public Image storyboardDisplay; 
    [Tooltip("El cuadrado 100% negro que tapa los dibujos (Telón)")]
    public Image fadeCurtain; 

    [Header("Secuencia de Dibujos")]
    public Sprite[] storyboardPanels; 

    [Header("Tiempos (Segundos)")]
    public float fadeDuration = 1.0f; // Tiempo en abrir/cerrar el telón
    public float displayDuration = 3.5f; // Tiempo que miras el dibujo

    private Coroutine cinematicCoroutine;

    private void Start()
    {
        // 1. Apagamos el menú principal de fondo
        if (mainMenuContainer != null) mainMenuContainer.SetActive(false);
        
        // 2. Arrancamos con el telón 100% negro
        SetCurtainAlpha(1f);
        
        // 3. Empieza la película
        cinematicCoroutine = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        // Chequeamos el Enter (Nuevo Input System) para saltar la cinemática
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
        // El dibujo siempre es sólido, la opacidad se la cambiamos al telón
        Color displayColor = storyboardDisplay.color;
        displayColor.a = 1f;
        storyboardDisplay.color = displayColor;

        foreach (Sprite panel in storyboardPanels)
        {
            // Telón cerrado y preparamos el dibujo
            SetCurtainAlpha(1f);
            storyboardDisplay.sprite = panel;

            // FADE IN: El telón negro se vuelve transparente
            yield return FadeCurtainAlpha(1f, 0f, fadeDuration);

            // DISPLAY: Tiempo que el jugador lee/mira
            yield return new WaitForSeconds(displayDuration);

            // FADE OUT: El telón negro vuelve a tapar todo
            yield return FadeCurtainAlpha(0f, 1f, fadeDuration);
        }

        // Fin de la secuencia
        EndCinematic();
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
        if (cinematicCoroutine != null) StopCoroutine(cinematicCoroutine); 
        EndCinematic();
    }

    private void EndCinematic()
    {
        // Apagamos la burbuja de la cinemática
        if (cinematicCanvas != null) cinematicCanvas.SetActive(false);
        
        // Prendemos el Menú Principal
        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
    }
}