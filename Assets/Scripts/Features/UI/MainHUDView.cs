using UnityEngine;
using TMPro;
using System.Collections;

public class MainHUDView : MonoBehaviour
{
    public static MainHUDView Instance;

    private PlayerViewModel _playerVM; 

    [Header("Contenedor General de Juego")]
    [SerializeField] private GameObject _hudGameplayPanel; // 🌟 NUEVO: El contenedor de TODO el HUD de juego

    [Header("Cartel de Inicio (Centro)")]
    [SerializeField] private GameObject _namePopupPanel; 
    [SerializeField] private TMP_InputField _popupNameInput; 

    [Header("Panel Jugador (Arriba Izquierda)")]
    [SerializeField] private TextMeshProUGUI _hudNameText; 
    [SerializeField] private TextMeshProUGUI _nameShadow; 

    [SerializeField] private TextMeshProUGUI _levelText; 
    [SerializeField] private TextMeshProUGUI _levelShadow; 

    [SerializeField] private TextMeshProUGUI _powerText; 
    [SerializeField] private TextMeshProUGUI _powerShadow; 

    [Header("Misiones")]
    [SerializeField] private TextMeshProUGUI _crystalsText; 
    [SerializeField] private TextMeshProUGUI _crystalCountShadow; 

    [Header("Avisos en Pantalla")]
    public TextMeshProUGUI warningText; 

    [Header("Check Boss (Mecánica Original)")]
    [SerializeField] private TextMeshProUGUI _bossCheckText; 
    [SerializeField] private TextMeshProUGUI _bossCheckShadow; 
    [SerializeField] private int _bossRequiredPower = 25; 

    public bool HasSetPlayerName { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Initialize(PlayerViewModel playerVM)
    {
        _playerVM = playerVM;
        _playerVM.OnPowerChanged += UpdatePowerUI;
        _playerVM.OnCrystalsChanged += UpdateCrystalUI;

        UpdatePowerUI(_playerVM.GetCurrentPower());
        UpdateCrystalUI(_playerVM.CrystalsCollected); 
    }

    private void OnEnable()
    {
        if (_popupNameInput != null) _popupNameInput.onEndEdit.AddListener(OnNameInputFinished);
    }

    private void OnDisable()
    {
        if (_playerVM != null) 
        {
            _playerVM.OnPowerChanged -= UpdatePowerUI;
            _playerVM.OnCrystalsChanged -= UpdateCrystalUI;
        }
        if (_popupNameInput != null) _popupNameInput.onEndEdit.RemoveAllListeners();
    }

    private void Start()
    {
        if (_levelText != null) 
        {
            _levelText.text = "Level: 1";
            if (_levelShadow != null) _levelShadow.text = _levelText.text; 
        }
        
        if (warningText != null) warningText.gameObject.SetActive(false);
        
        // 🌟 APAGAMOS TODO EL HUD DE JUEGO AL EMPEZAR
        if (_hudGameplayPanel != null) _hudGameplayPanel.SetActive(false);
        
        if (_namePopupPanel != null) _namePopupPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Time.timeScale = 0f; 
    }

    private void OnNameInputFinished(string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            if (_hudNameText != null) 
            {
                _hudNameText.text = playerName;
                if (_nameShadow != null) _nameShadow.text = _hudNameText.text; 
            }

            if (_namePopupPanel != null) _namePopupPanel.SetActive(false);
            
            // 🌟 ENCENDEMOS TODO EL HUD DE JUEGO YA QUE SE PUSO EL NOMBRE
            if (_hudGameplayPanel != null) _hudGameplayPanel.SetActive(true);
            
            HasSetPlayerName = true;
            Time.timeScale = 1f; 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void UpdateCrystalUI(int currentCrystals)
    {
        if (_crystalsText != null) 
        {
            _crystalsText.text = $"{currentCrystals}/4 Crystals";
            if (_crystalCountShadow != null) _crystalCountShadow.text = _crystalsText.text; 
        }
    }

    private void UpdatePowerUI(int currentPower)
    {
        if (_powerText != null) 
        {
            _powerText.text = $"Power: {currentPower}";
            if (_powerShadow != null) _powerShadow.text = _powerText.text; 
        }
        
        if (_bossCheckText != null)
        {
            if (currentPower >= _bossRequiredPower)
            {
                _bossCheckText.text = "Portal: Ready";
            }
            else
            {
                _bossCheckText.text = $"Portal: {currentPower}/{_bossRequiredPower}";
            }
            
            if (_bossCheckShadow != null) _bossCheckShadow.text = _bossCheckText.text; 
        }
    }

    public void ShowWarning(string mensaje)
    {
        if (warningText != null)
        {
            StopAllCoroutines(); 
            StartCoroutine(CartelRoutine(mensaje));
        }
    }

    private IEnumerator CartelRoutine(string mensaje)
    {
        warningText.text = mensaje;
        warningText.gameObject.SetActive(true); 
        yield return new WaitForSeconds(2f); 
        warningText.gameObject.SetActive(false); 
    }
}