using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Aman.Features.Title.Models;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // ¡NUEVO!: Necesario para hacer la magia del Hover desde el script

public class TitleView : MonoBehaviour
{
    [Header("UI Elements - Main Menu")]
    [SerializeField] private GameObject _buttonsContainer; 
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionsButton; 
    [SerializeField] private Button _quitButton;

    [Header("UI Elements - Options")]
    [SerializeField] private GameObject _optionsPanel; 
    [SerializeField] private Button _backButton; // ¡NUEVO!: El botón para volver desde opciones

    [Header("Media")]
    [SerializeField] private VideoPlayer _smokeVideo; 

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private TitleViewModel _viewModel;

    private void Awake()
    {
        var model = new TitleModel();
        _viewModel = new TitleViewModel(model);
    }

    private void OnEnable()
    {
        // Vinculamos los botones a los métodos del ViewModel y al sonido
        _startButton.onClick.AddListener(() => { PlayClickSound(); _viewModel.StartGame(); });
        _optionsButton.onClick.AddListener(() => { PlayClickSound(); _viewModel.OpenOptions(); });
        _quitButton.onClick.AddListener(() => { PlayClickSound(); _viewModel.ExitGame(); });
        
        if (_backButton != null) 
            _backButton.onClick.AddListener(() => { PlayClickSound(); CloseOptions(); });

        // Suscribir los botones al sonido de Hover
        AddHoverSoundToButton(_startButton);
        AddHoverSoundToButton(_optionsButton);
        AddHoverSoundToButton(_quitButton);
        if (_backButton != null) AddHoverSoundToButton(_backButton);

        // Escuchamos los eventos del ViewModel
        _viewModel.OnStartGameRequested += HandleStartGame;
        _viewModel.OnOptionsRequested += HandleOpenOptions; 

        if (_smokeVideo != null)
        {
            _smokeVideo.loopPointReached += OnSmokeVideoFinished;
        }
    }

    private void OnSmokeVideoFinished(VideoPlayer vp)
    {
        _buttonsContainer.SetActive(true);
        vp.gameObject.SetActive(false); 
    }

    private void HandleStartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    private void HandleOpenOptions()
    {
        Debug.Log("Abriendo opciones...");
        _buttonsContainer.SetActive(false); 
        _optionsPanel.SetActive(true);      
    }

    public void CloseOptions() 
    {
        _optionsPanel.SetActive(false);
        _buttonsContainer.SetActive(true);
    }

    // --- Lógica de Sonidos de UI ---

    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    private void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    // Un pequeño truco de código para inyectarle el EventTrigger a los botones sin tocar el Inspector
    private void AddHoverSoundToButton(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveAllListeners();
        _optionsButton.onClick.RemoveAllListeners(); 
        _quitButton.onClick.RemoveAllListeners();
        if (_backButton != null) _backButton.onClick.RemoveAllListeners();
        
        _viewModel.OnStartGameRequested -= HandleStartGame;
        _viewModel.OnOptionsRequested -= HandleOpenOptions; 

        if (_smokeVideo != null)
        {
            _smokeVideo.loopPointReached -= OnSmokeVideoFinished;
        }
    }
}