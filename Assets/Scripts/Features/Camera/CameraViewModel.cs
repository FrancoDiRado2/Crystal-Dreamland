using System;

public class CameraViewModel
{
    // Estado de la cámara
    public bool IsFirstPersonActive { get; private set; }

    // Evento al que la View se va a suscribir
    public event Action<bool> OnCameraStateChanged;

    public CameraViewModel()
    {
        // Arrancamos siempre en tercera persona
        IsFirstPersonActive = false; 
    }

    public void ToggleCamera()
    {
        IsFirstPersonActive = !IsFirstPersonActive;
        OnCameraStateChanged?.Invoke(IsFirstPersonActive);
    }
}