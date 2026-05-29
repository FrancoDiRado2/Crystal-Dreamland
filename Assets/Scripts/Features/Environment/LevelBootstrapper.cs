using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Views a conectar")]
    public PortalView portalView;
    public LevelManagerView levelManagerView;

    private LevelModel _levelModel;
    private LevelViewModel _levelViewModel;

    private void Awake()
    {
        // 1. Instanciamos el Model (Datos)
        _levelModel = new LevelModel();

        // 2. Instanciamos el ViewModel (Lógica)
        _levelViewModel = new LevelViewModel(_levelModel);

        // 3. Inyectamos las dependencias a las Views
        if (portalView != null) portalView.Initialize(_levelViewModel);
        if (levelManagerView != null) levelManagerView.Initialize(_levelViewModel);
    }
}