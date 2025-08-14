using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioType
{
    Master,
    BGM,
    SFX
}

public class AudioManager : SingletonObject<AudioManager>
{
    public AudioMixer audioMixer;
    private static Dictionary<AudioType, int> volumes;
    [SerializeField] private List<VolumeSlider> volumeSliders;

    public void Initialize() => volumeSliders.ForEach(vs => vs.Initialize());
    public void SetAudioVolumes(Dictionary<AudioType, int> volumes)
    {
        AudioManager.volumes = new(volumes);
        volumeSliders.ForEach(vs => vs.ApplyVolume(volumes[vs.audioType]));
    }
    public void SetAudioVolume(AudioType audioType, int volume) => volumes[audioType] = volume;
    public static Dictionary<AudioType, int> GetVolumes() => volumes;
    public static int GetVolume(AudioType audioType) => volumes[audioType];
    public void SaveVolume() => volumeSliders.ForEach(vs => vs.SaveVolume());
    public void RevertVolume() => volumeSliders.ForEach(vs => vs.RevertVolume());
}