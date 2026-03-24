using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider volumeSlider;

    const string VOLUME_KEY = "MasterVolumeValue";

    void Start()
    {
        float savedValue = LoadVolume();
        volumeSlider.value = savedValue;
        SetVolume(savedValue);

        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        SetVolume(value);
        SaveVolume(value);
    }

    public void SetVolume(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        mixer.SetFloat("MasterVolume", volume);
    }

    public void SaveVolume(float value)
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public float LoadVolume()
    {
        return PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
    }
}