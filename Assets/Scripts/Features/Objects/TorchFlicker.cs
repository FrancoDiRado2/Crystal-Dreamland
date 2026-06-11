using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [Header("Referencia a la luz")]
    public Light torchLight;

    [Header("Configuración del fuego")]
    public float minIntensity = 1.5f; // Lo más oscuro que se va a poner
    public float maxIntensity = 3.0f; // Lo más brillante que se va a poner
    public float flickerSpeed = 0.1f; // Qué tan rápido cambia (0.1 es rápido y realista)

    private float targetIntensity;

    void Start()
    {
        // Si nos olvidamos de asignarla en el inspector, la busca sola
        if (torchLight == null) torchLight = GetComponent<Light>();
        
        // Empieza a llamar a la función de cambiar intensidad repetidamente
        InvokeRepeating(nameof(ChangeIntensity), 0f, flickerSpeed);
    }

    void ChangeIntensity()
    {
        // Elige una nueva intensidad al azar entre el mínimo y el máximo
        targetIntensity = Random.Range(minIntensity, maxIntensity);
    }

    void Update()
    {
        if (torchLight != null)
        {
            // Transición suavizada (Lerp) para que no cambie de golpe y se vea como fuego natural
            torchLight.intensity = Mathf.Lerp(torchLight.intensity, targetIntensity, Time.deltaTime * 15f);
        }
    }
}