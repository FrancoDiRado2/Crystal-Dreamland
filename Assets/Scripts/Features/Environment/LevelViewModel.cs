using System;

public class LevelViewModel
{
    private LevelModel _model;
    
    public event Action OnDemoCompleted;
    public event Action OnPortalLocked; 

    // 🌟 SOLO AGREGAMOS ESTA LÍNEA (Para que la pared pregunte si el jefe murió)
    public bool IsBossDefeated => _model.IsBossDefeated; 

    public LevelViewModel(LevelModel model)
    {
        _model = model;
    }

    public void SetBossDefeated(bool defeated)
    {
        _model.IsBossDefeated = defeated;
    }

    public void CompleteDemo()
    {
        if (!_model.IsDemoCompleted && _model.IsBossDefeated)
        {
            _model.IsDemoCompleted = true;
            OnDemoCompleted?.Invoke();
        }
        else if (!_model.IsBossDefeated)
        {
            OnPortalLocked?.Invoke(); 
        }
    }
}