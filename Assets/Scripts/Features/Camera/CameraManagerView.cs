using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManagerView : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    [SerializeField] private GameObject _thirdPersonCam;
    [SerializeField] private GameObject _firstPersonCam;

    private CameraViewModel _viewModel;

    private void Awake()
    {
        // Instanciamos el ViewModel (igual que hacés en TitleView)
        _viewModel = new CameraViewModel();
    }

    private void OnEnable()
    {
        _viewModel.OnCameraStateChanged += UpdateCameraVisibility;
    }

    private void OnDisable()
    {
        _viewModel.OnCameraStateChanged -= UpdateCameraVisibility;
    }

    private void Start()
    {
        // Forzamos el estado inicial al arrancar
        UpdateCameraVisibility(_viewModel.IsFirstPersonActive);
    }

    private void Update()
    {
        // BLOQUEO: Igual que en tu CameraFollow
        if (MainHUDView.Instance != null && !MainHUDView.Instance.HasSetPlayerName) return;

        // Detectar F3
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            _viewModel.ToggleCamera();
        }
    }

    private void UpdateCameraVisibility(bool isFirstPerson)
    {
        _firstPersonCam.SetActive(isFirstPerson);
        _thirdPersonCam.SetActive(!isFirstPerson);
    }
}