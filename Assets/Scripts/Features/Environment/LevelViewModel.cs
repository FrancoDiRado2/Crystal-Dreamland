using System;

public class LevelViewModel
{
    private LevelModel _model;
    
    public event Action OnDemoCompleted;
    public event Action OnPortalLocked; // NUEVO: Para avisar que falta matar al jefe

    public LevelViewModel(LevelModel model)
    {
        _model = model;
    }

    // NUEVO: Método para cambiar el estado del jefe
    public void SetBossDefeated(bool defeated)
    {
        _model.IsBossDefeated = defeated;
    }

    public void CompleteDemo()
    {
        // Solo termina la demo si el jefe está derrotado
        if (!_model.IsDemoCompleted && _model.IsBossDefeated)
        {
            _model.IsDemoCompleted = true;
            OnDemoCompleted?.Invoke();
        }
        else if (!_model.IsBossDefeated)
        {
            OnPortalLocked?.Invoke(); // El portal rechaza a Aman
        }
    }
}