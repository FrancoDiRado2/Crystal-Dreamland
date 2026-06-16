using UnityEngine;
using TMPro;
using System.Collections;

public class LevelNarrator : MonoBehaviour
{
    // 🌟 Instancia global para poder llamarlo desde otros scripts
    public static LevelNarrator Instance; 

    [Header("Referencias")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Secuencia de Narración")]
    public AudioClip[] narratorClips;
    [TextArea] public string[] spanishSubtitles;

    [Header("Configuración de Subtítulos")]
    [Tooltip("Tiempo entre cada letra. 0.02f es rápido para seguirle el ritmo a la voz acelerada.")]
    public float typingSpeed = 0.02f;

    // 🌟 Esta variable estática SOBREVIVE a los reinicios de escena (Try Again)
    private static bool _yaHablo = false;

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false);

        // Si ya habló en esta ejecución del juego, apagamos el narrador y no hacemos nada
        if (_yaHablo)
        {
            gameObject.SetActive(false);
            return;
        }

        _yaHablo = true; // Marcamos que ya habló para el futuro
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
                _audioSource.clip = narratorClips[i];
                _audioSource.Play();

                yield return StartCoroutine(TypeText(spanishSubtitles[i]));
                yield return new WaitWhile(() => _audioSource.isPlaying);
                yield return new WaitForSeconds(0.4f);
            }
        }

        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false);
    }

    private IEnumerator TypeText(string line)
    {
        _subtitleText.text = ""; 
        foreach (char letter in line.ToCharArray())
        {
            _subtitleText.text += letter; 
            yield return new WaitForSeconds(typingSpeed); 
        }
    }

    // 🌟 NUEVA FUNCIÓN: Corta todo de golpe si empieza el combate
    public void CortarNarracion()
    {
        StopAllCoroutines(); // Frena la máquina de escribir
        if (_audioSource != null) _audioSource.Stop(); // Calla al narrador
        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false); // Esconde el texto
        gameObject.SetActive(false); // Se apaga a sí mismo
    }
}