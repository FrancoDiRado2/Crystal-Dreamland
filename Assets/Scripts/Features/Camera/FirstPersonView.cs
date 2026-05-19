using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonView : MonoBehaviour
{
    [Header("Target a rotar")]
    [SerializeField] private Transform _playerBody;
    
    [Header("Configuración")]
    [SerializeField] private float _sensitivity = 1.0f; // Veo que lo pusiste a 1, lo seteamos por defecto
    
    // UMBRAL: Si el mouse salta más de este valor en un frame, lo ignoramos
    // (Ajustá este valor si notás micro-tirones al girar muy rápido, pero 100 suele ser seguro en Editor)
    [SerializeField] private float _ignoreMouseSpikeThreshold = 100f; 

    private float _xRotation = 0f;

    private void Update()
    {
        // BLOQUEO: Respeta la misma regla del HUD
        if (MainHUDView.Instance != null && !MainHUDView.Instance.HasSetPlayerName) return;
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // --- FIX AVANZADO PARA EL GIRO 180 ---
        // Si hay un salto absurdo en X o Y, cancelamos el input de este frame.
        if (Mathf.Abs(mouseDelta.x) > _ignoreMouseSpikeThreshold || Mathf.Abs(mouseDelta.y) > _ignoreMouseSpikeThreshold)
        {
            mouseDelta = Vector2.zero;
        }

        // Rotación vertical (cabeza - en transform local)
        _xRotation -= mouseDelta.y * _sensitivity * 0.1f;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Rotación horizontal (cuerpo entero - en transform de PlayerBody)
        if (_playerBody != null)
        {
            // Usamos Space.Self para asegurarnos que rote sobre el eje Y local del personaje
            _playerBody.Rotate(Vector3.up * mouseDelta.x * _sensitivity * 0.1f, Space.Self);
        }
    }
}