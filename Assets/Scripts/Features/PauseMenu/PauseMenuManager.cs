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
    private float preMuteMasterVolume; 

    // 🌟 NUEVAS VARIABLES PARA GUARDAR EL ESTADO DEL MOUSE
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        muteToggle.isOn = false;

        // 🌟 SINCRONIZAMOS LOS SLIDERS CON EL MIXER (Para mantener los ajustes del Main Menu)
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
        // 🌟 GUARDAMOS CÓMO ESTABA EL MOUSE ANTES DE PAUSAR
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;

        // 🌟 RESTAURAMOS EL ESTADO ANTERIOR DEL MOUSE (Combate o Exploración)
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu"); 
    }

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