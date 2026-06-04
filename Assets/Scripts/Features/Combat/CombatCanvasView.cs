using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatCanvasView : MonoBehaviour
{
    [Header("UI Jugador")]
    public Slider amanHealthBar;
    public TextMeshProUGUI amanPowerText;
    
    [Header("UI Jefe")]
    public Slider bossHealthBar;
    
    [Header("Centro")]
    public TextMeshProUGUI turnText;
    
    [Header("Botones")]
    public Button attackButton;
    public Button healButton;

    private CombatViewModel _combatVM;
    private PlayerViewModel _playerVM;
    private EnemyViewModel _enemyVM; 

    public void Initialize(CombatViewModel combatVM, PlayerViewModel playerVM, EnemyViewModel enemyVM)
    {
        _combatVM = combatVM;
        _playerVM = playerVM;
        _enemyVM = enemyVM; 

        attackButton.onClick.AddListener(_combatVM.PlayerAttack);
        healButton.onClick.AddListener(_combatVM.PlayerHeal);

        _combatVM.OnTurnMessageChanged += UpdateTurnText;
        _playerVM.OnPowerChanged += UpdatePowerText; 
        _enemyVM.OnHealthChanged += UpdateBossHealth;
        
        // NUEVO: Escuchamos si Aman recibe daño
        _playerVM.OnHealthChanged += UpdateAmanHealth;

        UpdatePowerText(_playerVM.GetCurrentPower()); 
        
        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = 100;
            bossHealthBar.value = 100;
        }
        
        // NUEVO: Seteamos la vida inicial de Aman en el Slider
        if (amanHealthBar != null)
        {
            amanHealthBar.maxValue = 100;
            amanHealthBar.value = 100;
        }
    }

    private void UpdateTurnText(string message)
    {
        if (turnText != null) turnText.text = message;
        attackButton.interactable = _combatVM.IsPlayerTurn;
        healButton.interactable = _combatVM.IsPlayerTurn;
    }

    private void UpdatePowerText(int currentPower)
    {
        if (amanPowerText != null) amanPowerText.text = $"Aman - Poder: {currentPower}";
    }

    private void UpdateBossHealth(int currentHealth)
    {
        if (bossHealthBar != null) bossHealthBar.value = currentHealth;
    }

    // NUEVO: Método que baja la barra de Aman
    private void UpdateAmanHealth(int currentHealth)
    {
        if (amanHealthBar != null) amanHealthBar.value = currentHealth;
    }

    private void OnDisable()
    {
        attackButton.onClick.RemoveAllListeners();
        healButton.onClick.RemoveAllListeners();
        
        if (_combatVM != null) _combatVM.OnTurnMessageChanged -= UpdateTurnText;
        if (_playerVM != null) _playerVM.OnPowerChanged -= UpdatePowerText;
        if (_enemyVM != null) _enemyVM.OnHealthChanged -= UpdateBossHealth;
        if (_playerVM != null) _playerVM.OnHealthChanged -= UpdateAmanHealth; // Limpieza
    }
}