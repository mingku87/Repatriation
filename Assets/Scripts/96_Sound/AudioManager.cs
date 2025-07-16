using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[System.Serializable]
public class VolumeControl
{
    public Slider volumeSlider;
    public AudioList exposedParameter; 

    [HideInInspector] public float volume = 1f; 
}

public enum AudioList
{
    Master,
    BGM,
    SFX
}

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;  

    public VolumeControl masterControl;  
    public VolumeControl bgmControl;      
    public VolumeControl sfxControl;     

    void Start()
    {
        masterControl.exposedParameter = AudioList.Master;
        bgmControl.exposedParameter = AudioList.BGM;
        sfxControl.exposedParameter = AudioList.SFX;

        InitializeVolumeControl(masterControl);
        InitializeVolumeControl(bgmControl);
        InitializeVolumeControl(sfxControl);

        UpdateAudioVolume(masterControl);
        UpdateAudioVolume(bgmControl);
        UpdateAudioVolume(sfxControl);
    }

    private void InitializeVolumeControl(VolumeControl control)
    {
        control.volumeSlider.onValueChanged.AddListener((value) => {
            control.volume = value;
            UpdateAudioVolume(control);
        });

    }

    private void UpdateAudioVolume(VolumeControl control)
    {
        float finalVolume =  Mathf.Log10(control.volume) * 20f;  

        audioMixer.SetFloat(control.exposedParameter.ToString(), finalVolume);
    }
}
