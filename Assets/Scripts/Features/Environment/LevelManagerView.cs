using UnityEngine;

public class LevelManagerView : MonoBehaviour
{
    private LevelViewModel _viewModel;
    
    [Header("UI (Opcional)")]
    public GameObject demoCompletePanel; // Por si le querés mostrar un "¡Gracias por jugar!"

    public void Initialize(LevelViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.OnDemoCompleted += StopGame;
    }

    private void StopGame()
    {
        // 1. Frenamos las físicas y animaciones de todo el juego
        Time.timeScale = 0f;

        // 2. Liberamos el mouse por si tenés que cerrar la ventana o tocar un botón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Mostramos cartel final (si asignaste uno)
        if (demoCompletePanel != null)
        {
            demoCompletePanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnDemoCompleted -= StopGame;
        }
    }
}