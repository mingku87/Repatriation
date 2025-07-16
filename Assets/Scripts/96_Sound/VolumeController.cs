using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        masterSlider.onValueChanged.AddListener(SoundManager.Instance.MasterVolume);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.BackGroundVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SFXVolume);

        masterSlider.value = 1f;
        bgmSlider.value = 1f;
        sfxSlider.value = 1f;
    }
}
