using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Views a conectar")]
    public PortalView portalView;
    public LevelManagerView levelManagerView;
    public EnemyView enemyView;
    public CombatManagerView combatManagerView;
    public CombatCanvasView combatCanvasView;

    [Header("Jugador (MVVM Puro)")]
    public PlayerModel playerModelAsset; // Tu ScriptableObject de stats
    public PlayerView playerView;
    public MainHUDView mainHUDView;
    public PowerUIView powerUIView;

    private LevelModel _levelModel;
    private LevelViewModel _levelViewModel;
    private EnemyModel _enemyModel;
    private EnemyViewModel _enemyViewModel;

    private void Awake()
    {
        // 1. CREAMOS EL CEREBRO DE AMAN (Le pasamos los stats del scriptable object)
        PlayerViewModel playerViewModel = new PlayerViewModel(playerModelAsset);

        // 2. INYECTAMOS EL CEREBRO A TODAS LAS VISTAS DEL JUGADOR
        if (playerView != null) playerView.Initialize(playerViewModel);
        if (mainHUDView != null) mainHUDView.Initialize(playerViewModel);
        if (powerUIView != null) powerUIView.Initialize(playerViewModel);

        // 3. ARMAMOS EL NIVEL
        _levelModel = new LevelModel();
        _levelViewModel = new LevelViewModel(_levelModel);

        // 4. ARMAMOS AL ENEMIGO
        _enemyModel = new EnemyModel();
        _enemyViewModel = new EnemyViewModel(_enemyModel);
        _enemyViewModel.OnDefeated += () => _levelViewModel.SetBossDefeated(true);

        // 5. INYECTAMOS LOS SISTEMAS DEL MUNDO
        if (portalView != null) portalView.Initialize(_levelViewModel);
        if (levelManagerView != null) levelManagerView.Initialize(_levelViewModel);
        if (enemyView != null) enemyView.Initialize(_enemyViewModel, playerViewModel);
        
        // BORRAMOS el combatManagerView de acá arriba (Paso 5) y lo pasamos abajo

        // 6. ARMAMOS EL COMBATE 
        CombatViewModel combatViewModel = new CombatViewModel(_enemyViewModel, playerViewModel);
        if (combatCanvasView != null) combatCanvasView.Initialize(combatViewModel, playerViewModel, _enemyViewModel);

        // 7. INYECTAMOS EL MANAGER DE TRANSICIÓN (Ahora le pasamos los dos)
        if (combatManagerView != null) combatManagerView.Initialize(_enemyViewModel, combatViewModel);
    }
}