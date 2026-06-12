using UnityEngine;

public class Crystal : MonoBehaviour
{
    [SerializeField] private int _powerAmount = 5; // Mantenemos tu variable de poder
    [SerializeField] private AudioClip _collectSound;
    [SerializeField] private float _volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerView playerView = other.GetComponent<PlayerView>();
            
            if (playerView != null && playerView.ViewModel != null)
            {
                // 🌟 SUMA PODER (Para no romper al jefe)
                playerView.ViewModel.AddPower(_powerAmount);
                
                // 🌟 SUMA CRISTAL (Para tu nuevo contador)
                playerView.ViewModel.AddCrystal();
            }

            if (_collectSound != null)
            {
                AudioSource.PlayClipAtPoint(_collectSound, transform.position, _volume);
            }

            Destroy(gameObject);
        }
    }
}