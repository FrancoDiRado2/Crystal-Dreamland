using UnityEngine;
using TMPro;

public class MainHUDView : MonoBehaviour
{
    public static MainHUDView Instance;

    private PlayerViewModel _playerVM; 

    [Header("Cartel de Inicio (Centro)")]
    [SerializeField] private GameObject _namePopupPanel; 
    [SerializeField] private TMP_InputField _popupNameInput; 

    [Header("Panel Jugador (Arriba Izquierda)")]
    [SerializeField] private TextMeshProUGUI _hudNameText; 
    [SerializeField] private TextMeshProUGUI _levelText; 
    [SerializeField] private TextMeshProUGUI _powerText; 
    // Removimos la referencia al slider de vida ya que no se necesita acá

    [Header("Check Boss (Mecánica Original)")]
    [SerializeField] private TextMeshProUGUI _bossCheckText; 
    [SerializeField] private int _bossRequiredPower = 25; 

    public bool HasSetPlayerName { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Inyectado desde el Bootstrapper
    public void Initialize(PlayerViewModel playerVM)
    {
        _playerVM = playerVM;
        _playerVM.OnPowerChanged += UpdatePowerUI;

        // Inicializamos los valores en base al poder real
        UpdatePowerUI(_playerVM.GetCurrentPower());
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
        }
        if (_popupNameInput != null) _popupNameInput.onEndEdit.RemoveAllListeners();
    }

    private void Start()
    {
        if (_levelText != null) _levelText.text = "Level: 1";
        
        if (_namePopupPanel != null) _namePopupPanel.SetActive(true);
        
        // Hacemos que aparezca el mouse solo para escribir el nombre
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Time.timeScale = 0f; 
    }

    private void OnNameInputFinished(string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            if (_hudNameText != null) _hudNameText.text = playerName;
            if (_namePopupPanel != null) _namePopupPanel.SetActive(false);
            
            HasSetPlayerName = true;
            Time.timeScale = 1f; 

            // ¡Arreglo del mouse! Al cerrar el cartel, se bloquea y desaparece del juego
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void UpdatePowerUI(int currentPower)
    {
        if (_powerText != null) _powerText.text = $"Power: {currentPower}";
        
        // Devolvemos tu lógica y textos exactos para el chequeo de cristales del portal
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
        }
    }
}