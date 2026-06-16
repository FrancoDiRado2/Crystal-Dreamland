using UnityEngine;
using TMPro;
using System.Collections;

public class MainHUDView : MonoBehaviour
{
    public static MainHUDView Instance;

    private PlayerViewModel _playerVM; 

    [Header("Cartel de Inicio (Centro)")]
    [SerializeField] private GameObject _namePopupPanel; 
    [SerializeField] private TMP_InputField _popupNameInput; 

    [Header("Panel Jugador (Arriba Izquierda)")]
    [SerializeField] private TextMeshProUGUI _hudNameText; 
    [SerializeField] private TextMeshProUGUI _nameShadow; // 🌟 Sombra Name

    [SerializeField] private TextMeshProUGUI _levelText; 
    [SerializeField] private TextMeshProUGUI _levelShadow; // 🌟 Sombra Level

    [SerializeField] private TextMeshProUGUI _powerText; 
    [SerializeField] private TextMeshProUGUI _powerShadow; // 🌟 Sombra Power

    [Header("Misiones")]
    [SerializeField] private TextMeshProUGUI _crystalsText; 
    [SerializeField] private TextMeshProUGUI _crystalCountShadow; // 🌟 Sombra Crystals

    // 🌟 ACÁ AGREGAMOS EL TEXTO PARA LOS AVISOS
    [Header("Avisos en Pantalla")]
    public TextMeshProUGUI warningText; 

    [Header("Check Boss (Mecánica Original)")]
    [SerializeField] private TextMeshProUGUI _bossCheckText; 
    [SerializeField] private TextMeshProUGUI _bossCheckShadow; // 🌟 Sombra Boss Check
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
            if (_levelShadow != null) _levelShadow.text = _levelText.text; // Sincroniza
        }
        
        // 🌟 NOS ASEGURAMOS QUE EL AVISO ESTÉ APAGADO AL EMPEZAR
        if (warningText != null) warningText.gameObject.SetActive(false);
        
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
                if (_nameShadow != null) _nameShadow.text = _hudNameText.text; // Sincroniza
            }

            if (_namePopupPanel != null) _namePopupPanel.SetActive(false);
            
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
            if (_crystalCountShadow != null) _crystalCountShadow.text = _crystalsText.text; // Sincroniza
        }
    }

    private void UpdatePowerUI(int currentPower)
    {
        if (_powerText != null) 
        {
            _powerText.text = $"Power: {currentPower}";
            if (_powerShadow != null) _powerShadow.text = _powerText.text; // Sincroniza
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
            
            if (_bossCheckShadow != null) _bossCheckShadow.text = _bossCheckText.text; // Sincroniza
        }
    }

    // 🌟 NUEVA FUNCIÓN: PRENDE EL CARTEL ROJO 2 SEGUNDOS Y LO APAGA
    public void ShowWarning(string mensaje)
    {
        if (warningText != null)
        {
            StopAllCoroutines(); // Por si chocás con el jefe dos veces rápido
            StartCoroutine(CartelRoutine(mensaje));
        }
    }

    private IEnumerator CartelRoutine(string mensaje)
    {
        warningText.text = mensaje;
        warningText.gameObject.SetActive(true); // Lo hacemos visible
        yield return new WaitForSeconds(2f); // Esperamos 2 segundos
        warningText.gameObject.SetActive(false); // Lo volvemos a ocultar
    }
}