using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform _target; // La puerta del villano
    [SerializeField] private RectTransform _arrowIcon; // El RectTransform de la imagen

    [Header("Ajustes")]
    [SerializeField] private float _rotationOffset = -90f; 
    // Ajustá esto si tu flecha apunta para el costado por defecto.

    private void Update()
    {
        if (_target == null || _arrowIcon == null) return;

        // 1. Calculamos la dirección horizontal desde la cámara hacia el objetivo
        Vector3 camPos = Camera.main.transform.position;
        Vector3 targetPos = _target.position;

        Vector3 directionToTarget = targetPos - camPos;
        directionToTarget.y = 0; // Ignoramos la altura para una navegación plana

        // 2. Calculamos el ángulo de la dirección hacia el objetivo
        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        // 3. Calculamos el ángulo actual de la cámara (hacia dónde mira el jugador)
        float cameraAngle = Camera.main.transform.eulerAngles.y;

        // 4. La rotación final es la diferencia entre ambos
        // El signo negativo es porque en la UI de Unity, el sentido horario es negativo
        float finalRotation = cameraAngle - targetAngle + _rotationOffset;

        // Aplicamos la rotación en el eje Z (el que hace girar las imágenes 2D)
        _arrowIcon.localRotation = Quaternion.Euler(0, 0, finalRotation);
    }
}