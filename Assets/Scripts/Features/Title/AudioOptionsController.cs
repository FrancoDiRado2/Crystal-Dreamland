using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioOptionsController : MonoBehaviour
{
    [Header("Referencia al Mixer Principal")]
    public AudioMixer audioMixer;

    [Header("Controles de la UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;

    [Header("Nombres de los Parámetros expuestos en el Mixer")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SFXVolume";

    private bool _isMuted = false;
    private float _prevMaster, _prevMusic, _prevSFX;

    private void Start()
    {
        // 1. Antes de escuchar a los sliders, leemos los valores REALES del Mixer
        SyncSlidersWithMixer();

        // 2. AHORA SÍ, empezamos a escuchar los cambios que haga el usuario
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (muteToggle != null) muteToggle.onValueChanged.AddListener(ToggleMuteAll);
    }

    // Esta función lee los decibelios del Mixer y mueve los Sliders para que coincidan visualmente
    private void SyncSlidersWithMixer()
    {
        if (audioMixer == null) return;

        float outValue;

        if (masterSlider != null && audioMixer.GetFloat(masterParam, out outValue))
        {
            // Convertimos de Logarítmico (decibelios) a Lineal (0 a 1) para el Slider
            masterSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));
        }

        if (musicSlider != null && audioMixer.GetFloat(musicParam, out outValue))
        {
            musicSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));
        }

        if (sfxSlider != null && audioMixer.GetFloat(sfxParam, out outValue))
        {
            sfxSlider.SetValueWithoutNotify(Mathf.Pow(10f, outValue / 20f));
        }
    }

    public void SetMasterVolume(float value)
    {
        if (_isMuted) return;
        // Convertimos el valor lineal del slider (0 a 1) a escala logarítmica de decibelios (-80dB a 0dB)
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat(masterParam, db);
    }

    public void SetMusicVolume(float value)
    {
        if (_isMuted) return;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat(musicParam, db);
    }

    public void SetSFXVolume(float value)
    {
        if (_isMuted) return;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat(sfxParam, db);
    }

    public void ToggleMuteAll(bool mute)
    {
        _isMuted = mute;

        if (mute)
        {
            // Clavamos todo el Mixer en el mínimo absoluto (-80 decibelios es silencio total)
            audioMixer.SetFloat(masterParam, -80f);
            audioMixer.SetFloat(musicParam, -80f);
            audioMixer.SetFloat(sfxParam, -80f);
        }
        else
        {
            // Si desmuteamos, recalculamos los volúmenes según dónde hayan quedado los sliders
            if (masterSlider != null) SetMasterVolume(masterSlider.value);
            if (musicSlider != null) SetMusicVolume(musicSlider.value);
            if (sfxSlider != null) SetSFXVolume(sfxSlider.value);
        }
    }
}