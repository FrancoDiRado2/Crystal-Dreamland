using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainHUDView : MonoBehaviour
{
    // SINGLETON: Permite que otros scripts accedan a la info sin buscar el objeto
    public static MainHUDView Instance;

    [Header("Referencias del Jugador")]
    [SerializeField] private PlayerViewModel _playerVM; 

    [Header("Cartel de Inicio (Centro)")]
    [SerializeField] private GameObject _namePopupPanel; 
    [SerializeField] private TMP_InputField _popupNameInput; 

    [Header("Panel Jugador (Arriba Izquierda)")]
    [SerializeField] private TextMeshProUGUI _hudNameText; 
    [SerializeField] private TextMeshProUGUI _levelText; 
    [SerializeField] private TextMeshProUGUI _powerText; 
    [SerializeField] private Slider _healthSlider; 

    [Header("Check Boss (Mecánica)")]
    [SerializeField] private TextMeshProUGUI _bossCheckText; 
    [SerializeField] private int _bossRequiredPower = 25; 

    public bool HasSetPlayerName { get; private set; } = false;

    private void Awake()
    {
        // Inicializamos el Singleton
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        if (_playerVM != null) _playerVM.OnPowerChanged += UpdatePowerUI;
        if (_popupNameInput != null)
        {
            _popupNameInput.onEndEdit.AddListener(OnNameInputFinished);
        }
    }

    private void OnDisable()
    {
        if (_playerVM != null) _playerVM.OnPowerChanged -= UpdatePowerUI;
        if (_popupNameInput != null) _popupNameInput.onEndEdit.RemoveAllListeners();
    }

    private void Start()
    {
        if (_levelText != null) _levelText.text = "Level: 1";
        if (_healthSlider != null) _healthSlider.value = _healthSlider.maxValue;
        if (_playerVM != null) UpdatePowerUI(_playerVM.GetCurrentPower());

        // 1. Mostrar cartel y resetear nombre visual
        if (_namePopupPanel != null) _namePopupPanel.SetActive(true);
        if (_hudNameText != null) _hudNameText.text = "Name: ???";

        // 2. Liberar el mouse para que el jugador pueda interactuar con el cartel
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Forzar el foco en el cuadro de texto
        if (_popupNameInput != null)
        {
            _popupNameInput.Select();
            _popupNameInput.ActivateInputField();
        }
    }

    private void UpdatePowerUI(int currentPower)
    {
        if (_powerText != null) _powerText.text = $"Power: {currentPower}";

        if (_bossCheckText != null)
        {
            if (currentPower >= _bossRequiredPower)
            {
                _bossCheckText.text = "Status: POWERFUL (Ready to fight)";
                _bossCheckText.color = Color.green;
            }
            else
            {
                _bossCheckText.text = $"Status: WEAK (Need {_bossRequiredPower - currentPower} more power)";
                _bossCheckText.color = Color.yellow;
            }
        }
    }

    private void OnNameInputFinished(string value) 
    { 
        if (!string.IsNullOrWhiteSpace(value))
        {
            HasSetPlayerName = true;
            
            if (_hudNameText != null) _hudNameText.text = "Name: " + value;
            if (_namePopupPanel != null) _namePopupPanel.SetActive(false);

            // Bloqueamos y ocultamos el mouse para empezar a jugar
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Si el nombre está vacío, forzamos el foco de nuevo
            _popupNameInput.Select();
            _popupNameInput.ActivateInputField();
        }
    }
}