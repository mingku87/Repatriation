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
    [SerializeField] private List<VolumeSlider> volumeSliders;

    public void Initialize() => volumeSliders.ForEach(vs => vs.Initialize());
    public void SaveVolume() => volumeSliders.ForEach(vs => vs.SaveVolume());
    public void RevertVolume() => volumeSliders.ForEach(vs => vs.RevertVolume());
    public void SetAudioVolume(Dictionary<AudioType, int> volumes) => volumeSliders.ForEach(vs => vs.SetVolume(volumes[vs.audioType]));

    public Dictionary<AudioType, int> GetVolume()
    {
        Dictionary<AudioType, int> volumes = new();
        foreach (var vs in volumeSliders) volumes.Add(vs.audioType, vs.GetVolume());
        return volumes;
    }
}