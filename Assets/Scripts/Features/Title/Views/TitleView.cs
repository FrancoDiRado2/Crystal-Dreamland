using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Aman.Features.Title.Models;
using UnityEngine.SceneManagement;

public class TitleView : MonoBehaviour
{
    [Header("UI Elements - Main Menu")]
    [SerializeField] private GameObject _buttonsContainer; 
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionsButton; // NUEVO: Botón de opciones
    [SerializeField] private Button _quitButton;

    [Header("UI Elements - Options")]
    [SerializeField] private GameObject _optionsPanel; // NUEVO: Panel de configuración

    [Header("Media")]
    [SerializeField] private VideoPlayer _smokeVideo; 

    private TitleViewModel _viewModel;

    private void Awake()
    {
        var model = new TitleModel();
        _viewModel = new TitleViewModel(model);
    }

    private void OnEnable()
    {
        // Vinculamos los botones
        _startButton.onClick.AddListener(_viewModel.StartGame);
        _optionsButton.onClick.AddListener(_viewModel.OpenOptions); // NUEVO
        _quitButton.onClick.AddListener(_viewModel.ExitGame);
        
        // Escuchamos los eventos del ViewModel
        _viewModel.OnStartGameRequested += HandleStartGame;
        _viewModel.OnOptionsRequested += HandleOpenOptions; // NUEVO

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
        _buttonsContainer.SetActive(false); // Oculta botones principales
        _optionsPanel.SetActive(true);      // Muestra el panel de opciones
    }

    // Esta función la vas a vincular directamente desde el botón "Volver" en el panel de opciones en Unity
    public void CloseOptions() 
    {
        _optionsPanel.SetActive(false);
        _buttonsContainer.SetActive(true);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveAllListeners();
        _optionsButton.onClick.RemoveAllListeners(); // NUEVO
        _quitButton.onClick.RemoveAllListeners();
        
        _viewModel.OnStartGameRequested -= HandleStartGame;
        _viewModel.OnOptionsRequested -= HandleOpenOptions; // NUEVO

        if (_smokeVideo != null)
        {
            _smokeVideo.loopPointReached -= OnSmokeVideoFinished;
        }
    }
    
}