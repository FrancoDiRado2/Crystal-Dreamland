using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Views a conectar")]
    public PortalView portalView;
    public LevelManagerView levelManagerView;
    public EnemyView enemyView; // NUEVO: Arrastrá acá a tu enemigo 3D
    
    [Header("Referencia Global")]
    public PlayerViewModel playerViewModel; // NUEVO: Arrastrá a Aman acá

    private LevelModel _levelModel;
    private LevelViewModel _levelViewModel;
    
    private EnemyModel _enemyModel;
    private EnemyViewModel _enemyViewModel;

    private void Awake()
    {
        // 1. Armamos el Nivel
        _levelModel = new LevelModel();
        _levelViewModel = new LevelViewModel(_levelModel);

        // 2. Armamos al Enemigo
        _enemyModel = new EnemyModel();
        _enemyViewModel = new EnemyViewModel(_enemyModel);

        // 3. CONEXIÓN CLAVE: Cuando el enemigo muere, avisamos al Nivel que destrabe la puerta
        _enemyViewModel.OnDefeated += () => _levelViewModel.SetBossDefeated(true);

        // 4. Inyectamos las dependencias a las Views
        if (portalView != null) portalView.Initialize(_levelViewModel);
        if (levelManagerView != null) levelManagerView.Initialize(_levelViewModel);
        
        // Inyectamos al enemigo pasándole su propia lógica Y la del jugador
        if (enemyView != null) enemyView.Initialize(_enemyViewModel, playerViewModel);
    }
}