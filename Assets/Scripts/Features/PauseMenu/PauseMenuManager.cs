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

    [Header("Audio (Volumen)")]
    public AudioMixer audioMixer;

    [Header("Audio (SFX Botones)")]
    public AudioSource uiAudioSource; // 🌟 NUEVO: Para reproducir los clicks
    public AudioClip hoverSound;      // 🌟 NUEVO: Sonido al pasar el mouse
    public AudioClip clickSound;      // 🌟 NUEVO: Sonido al hacer clic

    private bool isPaused = false;
    private float preMuteMasterVolume; 

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        muteToggle.isOn = false;

        SyncSlidersWithMixer();

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        muteToggle.onValueChanged.AddListener(SetMute);
    }

    private void SyncSlidersWithMixer()
    {
        if (audioMixer == null) return;

        float outValue;
        if (masterSlider != null && audioMixer.GetFloat("MasterVolume", out outValue))
            masterSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));

        if (musicSlider != null && audioMixer.GetFloat("MusicVolume", out outValue))
            musicSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));

        if (sfxSlider != null && audioMixer.GetFloat("SFXVolume", out outValue))
            sfxSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;

        // 🌟 NUEVO: Silenciamos temporalmente al narrador
        if (LevelNarrator.Instance != null)
        {
            LevelNarrator.Instance.PausarNarrador();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;

        // 🌟 NUEVO: Reanudamos al narrador
        if (LevelNarrator.Instance != null)
        {
            LevelNarrator.Instance.ReanudarNarrador();
        }

        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu"); 
    }

    // --- MÉTODOS PARA LOS SONIDOS DE LA UI ---
    public void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    // --- MÉTODOS DE VOLUMEN ---
    private void SetMasterVolume(float value)
    {
        if (!muteToggle.isOn)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    private void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    private void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    private void SetMute(bool isMuted)
    {
        if (isMuted)
        {
            preMuteMasterVolume = masterSlider.value;
            audioMixer.SetFloat("MasterVolume", -80f); 
            masterSlider.interactable = false; 
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(preMuteMasterVolume, 0.0001f)) * 20f);
            masterSlider.interactable = true; 
        }
    }
}