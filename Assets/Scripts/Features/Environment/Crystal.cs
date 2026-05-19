using UnityEngine;

public class Crystal : MonoBehaviour
{
    [Header("Configuración de Poder")]
    [SerializeField] private int _powerAmount = 5;

    [Header("Sonido de Recolección")]
    [SerializeField] private AudioClip _collectSound; // Aquí va el "¡Plink!"
    [SerializeField] [Range(0f, 1f)] private float _volume = 0.8f; // Control de volumen

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que tocó el cristal es Aman (el Player)
        if (other.CompareTag("Player"))
        {
            // 1. Le avisamos al ViewModel que sume poder
            PlayerViewModel vm = other.GetComponent<PlayerViewModel>();
            if (vm != null)
            {
                vm.AddPower(_powerAmount);
            }

            // 2. MAGIA DE AUDIO: Reproduce el sonido en la posición actual
            if (_collectSound != null)
            {
                AudioSource.PlayClipAtPoint(_collectSound, transform.position, _volume);
            }

            // 3. Desaparece el cristal
            Destroy(gameObject);
        }
    }
}