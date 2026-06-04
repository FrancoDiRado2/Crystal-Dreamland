using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject pauseMenuPanel;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;
    private float preMuteMasterVolume; // Guarda el volumen antes de mutear

private void Start()
    {
        // 1. El menú arranca apagado
        pauseMenuPanel.SetActive(false);

        // 2. Forzamos a que el Toggle arranque apagado por código también
        muteToggle.isOn = false;

        // 3. Suscribimos los eventos
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        muteToggle.onValueChanged.AddListener(SetMute);

        // 4. Inicializamos los valores en los sliders
        masterSlider.value = 1f;
        musicSlider.value = 0.8f;
        sfxSlider.value = 1f;

        // 5. FORZAMOS el envío de esos valores al AudioMixer de entrada
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Solo evitamos la pausa si el jugador no puso su nombre y el cartel sigue activo.
            // Usamos un chequeo seguro: si el nombre es vacío, no pausa.
            if (MainHUDView.Instance != null && !MainHUDView.Instance.HasSetPlayerName)
            {
                return; 
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Congela el tiempo
        isPaused = true;

        // Liberar el cursor para poder usar el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Descongela el tiempo
        isPaused = false;

        // Volver a ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANTE: Descongelar el tiempo antes de cambiar de escena
        // Cambiá "MainMenu" por el nombre exacto de tu escena de menú principal
        SceneManager.LoadScene("Main Menu"); 
    }

    // --- LÓGICA DE AUDIO (Fórmula Logarítmica) ---
    private void SetMasterVolume(float value)
    {
        if (!muteToggle.isOn)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        }
    }

    private void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    private void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }

    private void SetMute(bool isMuted)
    {
        if (isMuted)
        {
            // Guardamos el valor actual del slider antes de mutear
            preMuteMasterVolume = masterSlider.value;
            audioMixer.SetFloat("MasterVolume", -80f); // -80db es silencio total
            masterSlider.interactable = false; // Bloquea el slider
        }
        else
        {
            // Restauramos usando el valor del slider
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(preMuteMasterVolume) * 20);
            masterSlider.interactable = true; // Desbloquea el slider
        }
    }
}