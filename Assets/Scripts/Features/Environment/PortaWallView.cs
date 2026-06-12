using UnityEngine;

public class PortalWallView : MonoBehaviour
{
    private LevelViewModel _viewModel;

    public void Initialize(LevelViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    // Volvemos a usar Trigger porque Aman es un CharacterController
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_viewModel.IsBossDefeated)
            {
                // Si el jefe murió, apagamos el muro entero para dejarte pasar al portal final
                gameObject.SetActive(false);
            }
            else
            {
                // Si el jefe vive, mostramos el cartel
                MainHUDView.Instance.ShowWarning("First beat the enemy");
            }
        }
    }
}