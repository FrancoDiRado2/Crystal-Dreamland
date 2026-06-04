using UnityEngine;

public class Crystal : MonoBehaviour
{
    [SerializeField] private int _powerAmount = 5;
    [SerializeField] private AudioClip _collectSound;
    [SerializeField] private float _volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Busca la View del jugador y a través de ella accede al cerebro
            PlayerView playerView = other.GetComponent<PlayerView>();
            
            if (playerView != null && playerView.ViewModel != null)
            {
                playerView.ViewModel.AddPower(_powerAmount);
            }

            if (_collectSound != null)
            {
                AudioSource.PlayClipAtPoint(_collectSound, transform.position, _volume);
            }

            Destroy(gameObject);
        }
    }
}