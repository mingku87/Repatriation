using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioType audioType;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Button volumeUpButton;
    [SerializeField] private Button volumeDownButton;
    [SerializeField] private Slider volumeSlider;
    private int snapVolume;
    private const float MUTE_DB = -80f;
    private const float MIN_LIN = 0.0001f;
    private const int STEP = 1;

    public void Initialize(int volume)
    {
        volumeSlider.minValue = DefaultSettings.volumeMin;
        volumeSlider.maxValue = DefaultSettings.volumeMax;
        volumeSlider.wholeNumbers = true;

        volumeSlider.onValueChanged.AddListener(val => SetVolume((int)val));
        volumeUpButton.onClick.AddListener(() => SetVolume(GetVolume() + STEP));
        volumeDownButton.onClick.AddListener(() => SetVolume(GetVolume() - STEP));

        SetVolume(volume);
        SnapVolume();
    }

    public void UpdateUI(int volume)
    {
        volumeText.text = volume.ToString();
        volumeSlider.SetValueWithoutNotify(volume);
    }

    public void SnapVolume() => snapVolume = GetVolume();
    public void RevertToSnapshot() => SetVolume(snapVolume);
    public int GetVolume() => (int)volumeSlider.value;
    public void SetVolume(int volume)
    {
        volume = Mathf.Clamp(volume, DefaultSettings.volumeMin, DefaultSettings.volumeMax);
        UpdateUI(volume);
        ApplyVolumeToMixer(volume);
    }
    private void ApplyVolumeToMixer(int volume)
    {
        float lin = Mathf.Clamp01(volume / 100.0f);
        float dB = (lin <= MIN_LIN) ? MUTE_DB : Mathf.Log10(lin) * 20.0f;
        AudioManager.Instance.audioMixer.SetFloat(audioType.ToString(), dB);
    }
}