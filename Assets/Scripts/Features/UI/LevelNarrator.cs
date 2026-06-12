using UnityEngine;
using TMPro;
using System.Collections;

public class LevelNarrator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Configuración del Evento")]
    [SerializeField] private AudioClip _narratorClip;
    [TextArea] [SerializeField] private string _spanishSubtitle;

    private void Start()
    {
        // Iniciamos la narración automáticamente al cargar el nivel
        StartCoroutine(PlayNarrative());
    }

    private IEnumerator PlayNarrative()
    {
        // Pequeña espera para que el jugador se ubique al arrancar
        yield return new WaitForSeconds(1f);

        if (_audioSource != null && _narratorClip != null)
        {
            // Disparamos audio y texto al mismo tiempo
            _audioSource.PlayOneShot(_narratorClip);
            
            if (_subtitleText != null)
            {
                _subtitleText.text = _spanishSubtitle;
                _subtitleText.gameObject.SetActive(true);
            }

            // Mantenemos el subtítulo mientras dura el audio
            yield return new WaitForSeconds(_narratorClip.length);

            // Ocultamos
            if (_subtitleText != null)
                _subtitleText.gameObject.SetActive(false);
        }
    }
}