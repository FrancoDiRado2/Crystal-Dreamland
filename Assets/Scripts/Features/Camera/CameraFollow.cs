using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target a seguir")]
    public Transform target; 
    public float heightOffset = 1.2f;

    [Header("Configuración de Órbita")]
    public float distance = 4f;       
    public float sensitivity = 0.2f;    

    [Header("Límites de Rotación")]
    public float minYAngle = -15f;    
    public float maxYAngle = 80f;     

    private float currentX = 0f;
    private float currentY = 20f;     

    void Start()
    {
        // El control del cursor ahora lo lleva el MainHUDView.cs
    }

    void Update()
    {
        // BLOQUEO: Si no se ha puesto el nombre, la cámara no rota
        if (MainHUDView.Instance != null && !MainHUDView.Instance.HasSetPlayerName)
        {
            return;
        }

        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        currentX += mouseDelta.x * sensitivity * 0.1f;
        currentY -= mouseDelta.y * sensitivity * 0.1f;

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
    }

    void LateUpdate()
    {
        // El LateUpdate también respeta el bloqueo para evitar tirones
        if (target == null || (MainHUDView.Instance != null && !MainHUDView.Instance.HasSetPlayerName)) 
            return;

        Vector3 lookAtPosition = target.position + (Vector3.up * heightOffset);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);

        transform.position = lookAtPosition + (rotation * direction);
        transform.LookAt(lookAtPosition);
    }
}