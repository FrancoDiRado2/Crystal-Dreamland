using UnityEngine;

public class PortalView : MonoBehaviour
{
    private LevelViewModel _viewModel;

    public void Initialize(LevelViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Asegurate de que Aman tenga el Tag "Player" en el Inspector
        if (other.CompareTag("Player")) 
        {
            _viewModel.CompleteDemo();
        }
    }
}