using UnityEngine;
using TMPro;
using System.Collections;

public class LevelNarrator : MonoBehaviour
{
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

    private static bool _yaHablo = false;
    private bool _isPaused = false; // 🌟 NUEVO: Evita que la corrutina salte audios

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false);

        if (_yaHablo)
        {
            gameObject.SetActive(false);
            return;
        }

        _yaHablo = true; 
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
                
                // 🌟 FIX: Ahora espera si el audio está sonando O si el juego está en pausa
                yield return new WaitWhile(() => _audioSource.isPlaying || _isPaused);
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

    public void CortarNarracion()
    {
        StopAllCoroutines(); 
        if (_audioSource != null) _audioSource.Stop(); 
        if (_subtitleText != null) _subtitleText.gameObject.SetActive(false); 
        gameObject.SetActive(false); 
    }

    public void PausarNarrador()
    {
        _isPaused = true; 
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Pause();
        }
    }

    public void ReanudarNarrador()
    {
        _isPaused = false; 
        if (_audioSource != null)
        {
            _audioSource.UnPause();
        }
    }
}