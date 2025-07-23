using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioType
{
    Master,
    BGM,
    SFX
}

public class AudioManager : DDOLSingleton<AudioManager>
{
    public AudioMixer audioMixer;
    [SerializeField] private List<VolumeSlider> volumeSliders;

    private Dictionary<AudioType, int> snapVolumes = new()
    {
        { AudioType.Master, 0 },
        { AudioType.BGM, 0 },
        { AudioType.SFX, 0 }
    };

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    void Initialize()
    {
        foreach (var vs in volumeSliders) vs.Initialize(50);
    }
}