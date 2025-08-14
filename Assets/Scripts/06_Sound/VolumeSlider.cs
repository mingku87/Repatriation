using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] public AudioType audioType;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Button volumeUpButton;
    [SerializeField] private Button volumeDownButton;
    [SerializeField] private Slider volumeSlider;
    private const float MUTE_DB = -80f;
    private const float MIN_LIN = 0.0001f;
    private const int STEP = 1;
    private int sliderValue => (int)volumeSlider.value;

    public void Initialize()
    {
        volumeSlider.minValue = DefaultSettings.volumeMin;
        volumeSlider.maxValue = DefaultSettings.volumeMax;
        volumeSlider.wholeNumbers = true;

        volumeSlider.onValueChanged.AddListener(val => ApplyVolume((int)val));
        volumeUpButton.onClick.AddListener(() => ApplyVolume(sliderValue + STEP));
        volumeDownButton.onClick.AddListener(() => ApplyVolume(sliderValue - STEP));

        ApplyVolume(AudioManager.GetVolume(audioType));
    }
    public void ApplyVolume(int volume)
    {
        volume = Mathf.Clamp(volume, DefaultSettings.volumeMin, DefaultSettings.volumeMax);
        UpdateUI(volume);
        ApplyVolumeToMixer(volume);
    }
    public void SaveVolume() => AudioManager.Instance.SetAudioVolume(audioType, sliderValue);
    public void RevertVolume() => ApplyVolume(AudioManager.GetVolume(audioType));
    public void UpdateUI(int volume)
    {
        volumeText.text = volume.ToString();
        volumeSlider.SetValueWithoutNotify(volume);
    }
    private void ApplyVolumeToMixer(int volume)
    {
        float lin = Mathf.Clamp01(volume / 100.0f);
        float dB = (lin <= MIN_LIN) ? MUTE_DB : Mathf.Log10(lin) * 20.0f;
        AudioManager.Instance.audioMixer.SetFloat(audioType.ToString(), dB);
    }
}