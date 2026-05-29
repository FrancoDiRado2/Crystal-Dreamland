using System;

public class LevelViewModel
{
    private LevelModel _model;
    
    // Evento para avisarle a Unity que la demo terminó
    public event Action OnDemoCompleted;

    public LevelViewModel(LevelModel model)
    {
        _model = model;
    }

    public void CompleteDemo()
    {
        if (!_model.IsDemoCompleted)
        {
            _model.IsDemoCompleted = true;
            OnDemoCompleted?.Invoke();
        }
    }
}