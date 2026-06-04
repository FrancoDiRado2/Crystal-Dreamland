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

    public void Initialize(CombatViewModel combatVM, PlayerViewModel playerVM)
    {
        _combatVM = combatVM;
        _playerVM = playerVM;

        // 1. Suscribimos los botones a la lógica
        attackButton.onClick.AddListener(_combatVM.PlayerAttack);
        healButton.onClick.AddListener(_combatVM.PlayerHeal);

        // 2. Escuchamos los cambios de texto (turnos)
        _combatVM.OnTurnMessageChanged += UpdateTurnText;
        
        // 3. Mostramos los cristales actuales de Aman
        amanPowerText.text = $"Aman - Poder: {_playerVM.GetCurrentPower()}";
    }

    private void UpdateTurnText(string message)
    {
        if (turnText != null) turnText.text = message;
        
        // Solo prendemos los botones si es el turno del jugador
        attackButton.interactable = _combatVM.IsPlayerTurn;
        healButton.interactable = _combatVM.IsPlayerTurn;
    }

    private void OnDisable()
    {
        attackButton.onClick.RemoveAllListeners();
        healButton.onClick.RemoveAllListeners();
        if (_combatVM != null) _combatVM.OnTurnMessageChanged -= UpdateTurnText;
    }
}