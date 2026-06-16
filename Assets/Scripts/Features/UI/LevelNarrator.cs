using UnityEngine;
using TMPro;
using System.Collections;

public class LevelNarrator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Secuencia de Narración")]
    public AudioClip[] narratorClips;
    [TextArea] public string[] spanishSubtitles;

    [Header("Configuración de Subtítulos")]
    [Tooltip("Tiempo entre cada letra. 0.02f es rápido para seguirle el ritmo a la voz acelerada.")]
    public float typingSpeed = 0.02f; // 🌟 NUEVO: Velocidad de escritura

    private void Start()
    {
        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false);
        StartCoroutine(PlayNarrativeSequence());
    }

    private IEnumerator PlayNarrativeSequence()
    {
        yield return new WaitForSeconds(1.5f);

        int totalLines = Mathf.Min(narratorClips.Length, spanishSubtitles.Length);

        for (int i = 0; i < totalLines; i++)
        {
            if (narratorClips[i] != null && _subtitleText != null)
            {
                _subtitleText.gameObject.SetActive(true);

                // Preparamos y disparamos el audio
                _audioSource.clip = narratorClips[i];
                _audioSource.Play();

                // 🌟 NUEVO: Disparamos el efecto de máquina de escribir y esperamos a que termine
                yield return StartCoroutine(TypeText(spanishSubtitles[i]));

                // Por si el texto terminó de escribirse muy rápido, nos aseguramos de 
                // esperar a que el audio termine de hablar antes de pasar a la siguiente frase.
                yield return new WaitWhile(() => _audioSource.isPlaying);

                // Una pausa de respiración un poco más corta para mantener el dinamismo
                yield return new WaitForSeconds(0.4f);
            }
        }

        // Apagamos el texto al terminar todo
        if (_subtitleText != null)
            _subtitleText.gameObject.SetActive(false);
    }

    // 🌟 NUEVA FUNCIÓN: Efecto Máquina de Escribir
    private IEnumerator TypeText(string line)
    {
        _subtitleText.text = ""; // Vaciamos el texto antes de empezar
        foreach (char letter in line.ToCharArray())
        {
            _subtitleText.text += letter; // Sumamos letra por letra
            yield return new WaitForSeconds(typingSpeed); // Esperamos milisegundos
        }
    }
}