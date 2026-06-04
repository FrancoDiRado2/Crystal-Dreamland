using UnityEngine;
using TMPro;
using System.Collections;

public class PowerUIView : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI _notificationText;

    [Header("Configuración")]
    [SerializeField] private float _displayTime = 2f; 

    private PlayerViewModel _playerViewModel;

    public void Initialize(PlayerViewModel viewModel)
    {
        _playerViewModel = viewModel;
        _playerViewModel.OnPowerChanged += ShowPowerNotification;
    }

    private void Start()
    {
        if (_notificationText != null) _notificationText.gameObject.SetActive(false);
    }

    private void ShowPowerNotification(int newTotalPower)
    {
        if (_notificationText != null)
        {
            _notificationText.text = $"Crystal collected! Total Power: {newTotalPower}";
            _notificationText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideTextRoutine());
        }
    }

    private IEnumerator HideTextRoutine()
    {
        yield return new WaitForSeconds(_displayTime);
        if (_notificationText != null) _notificationText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_playerViewModel != null) _playerViewModel.OnPowerChanged -= ShowPowerNotification;
    }
}