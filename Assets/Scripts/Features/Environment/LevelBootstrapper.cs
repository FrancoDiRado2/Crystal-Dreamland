using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Views a conectar")]
    public PortalView portalView;
    public LevelManagerView levelManagerView;
    public EnemyView enemyView;
    public CombatManagerView combatManagerView;
    public CombatCanvasView combatCanvasView;
    
    // 🌟 NUEVO: El espacio para arrastrar la nueva pared
    public PortalWallView portalWallView; 

    [Header("Jugador (MVVM Puro)")]
    public PlayerModel playerModelAsset; 
    public PlayerView playerView;
    public MainHUDView mainHUDView;
    public PowerUIView powerUIView;

    private LevelModel _levelModel;
    private LevelViewModel _levelViewModel;
    private EnemyModel _enemyModel;
    private EnemyViewModel _enemyViewModel;

    private void Awake()
    {
        PlayerViewModel playerViewModel = new PlayerViewModel(playerModelAsset);

        if (playerView != null) playerView.Initialize(playerViewModel);
        if (mainHUDView != null) mainHUDView.Initialize(playerViewModel);
        if (powerUIView != null) powerUIView.Initialize(playerViewModel);

        _levelModel = new LevelModel();
        _levelViewModel = new LevelViewModel(_levelModel);

        _enemyModel = new EnemyModel();
        _enemyViewModel = new EnemyViewModel(_enemyModel);
        _enemyViewModel.OnDefeated += () => _levelViewModel.SetBossDefeated(true);

        if (portalView != null) portalView.Initialize(_levelViewModel);
        if (levelManagerView != null) levelManagerView.Initialize(_levelViewModel);
        if (enemyView != null) enemyView.Initialize(_enemyViewModel, playerViewModel);
        
        // 🌟 NUEVO: Inyectamos la información a la pared
        if (portalWallView != null) portalWallView.Initialize(_levelViewModel);

        CombatViewModel combatViewModel = new CombatViewModel(_enemyViewModel, playerViewModel);
        if (combatCanvasView != null) combatCanvasView.Initialize(combatViewModel, playerViewModel, _enemyViewModel);

        if (combatManagerView != null) combatManagerView.Initialize(_enemyViewModel, combatViewModel);
    }
}