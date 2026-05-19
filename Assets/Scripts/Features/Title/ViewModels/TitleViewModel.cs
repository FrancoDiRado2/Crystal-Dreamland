using System;
using Aman.Features.Title.Models;

public class TitleViewModel
{
    private readonly TitleModel _model;

    public event Action OnStartGameRequested;
    public event Action OnOptionsRequested; // NUEVO: Evento para abrir opciones
    
    public string VersionText => $"Version: {_model.GameVersion}";
    public bool CanContinue => _model.HasSaveData;

    public TitleViewModel(TitleModel model)
    {
        _model = model;
    }

    public void StartGame()
    {
        OnStartGameRequested?.Invoke();
    }

    public void OpenOptions() // NUEVO: Se ejecuta al presionar el botón
    {
        OnOptionsRequested?.Invoke();
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            UnityEngine.Application.Quit();
        #endif
    }
}