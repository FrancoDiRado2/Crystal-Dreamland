using UnityEngine;
using TMPro; // Fundamental para usar TextMeshPro
using System.Collections;

public class PowerUIView : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerViewModel _playerViewModel;
    [SerializeField] private TextMeshProUGUI _notificationText;

    [Header("Configuración")]
    [SerializeField] private float _displayTime = 2f; // Segundos que dura el texto

    private void OnEnable()
    {
        // Nos suscribimos al evento cuando la UI se activa
        if (_playerViewModel != null)
        {
            _playerViewModel.OnPowerChanged += ShowPowerNotification;
        }
    }

    private void OnDisable()
    {
        // Nos desuscribimos por seguridad cuando se destruye/apaga
        if (_playerViewModel != null)
        {
            _playerViewModel.OnPowerChanged -= ShowPowerNotification;
        }
    }

    private void Start()
    {
        // Asegurarnos de que el texto arranque invisible
        _notificationText.gameObject.SetActive(false);
    }

    private void ShowPowerNotification(int newTotalPower)
    {
        // 1. Armamos el mensaje
        _notificationText.text = $"Crystal collected! Total Power: {newTotalPower}";
        
        // 2. Prendemos el texto
        _notificationText.gameObject.SetActive(true);

        // 3. Reiniciamos el contador de tiempo (por si agarra 2 cristales muy rápido)
        StopAllCoroutines();
        StartCoroutine(HideTextRoutine());
    }

    private IEnumerator HideTextRoutine()
    {
        // Esperamos los segundos configurados
        yield return new WaitForSeconds(_displayTime);
        
        // Apagamos el texto
        _notificationText.gameObject.SetActive(false);
    }
}