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
                // Mostramos el texto
                _subtitleText.text = spanishSubtitles[i];
                _subtitleText.gameObject.SetActive(true);

                // Preparamos y disparamos el audio
                _audioSource.clip = narratorClips[i];
                _audioSource.Play();

                // 🌟 EL TRUCO: Le decimos al código que espere 1 solo fotograma.
                // Esto le da tiempo a Unity para poner el "isPlaying" en verdadero.
                yield return null;

                // Ahora sí, se va a quedar clavado acá escuchando el audio hasta que termine
                yield return new WaitWhile(() => _audioSource.isPlaying);

                // Una pausa de medio segundo de respiración antes de la siguiente frase
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Apagamos el texto al terminar todo
        if (_subtitleText != null)
            _subtitleText.gameObject.SetActive(false);
    }
}