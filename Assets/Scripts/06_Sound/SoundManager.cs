using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : SingletonObject<SoundManager>
{
    public AudioSource backGroundSound;
    public AudioSource soundEffect;
    public AudioMixer mixer;
    private float sfxVolume = 1.0f;
    private float backgroundVolume;
    private float masterVolume;
    private const float DUCKING_VOLUME = 0.5f;
    private float originalBGMVolume;
    private bool isDucking = false;
    private ImportantSoundList importantSoundList;
    private const int MAX_CONCURRENT_SOUNDS = 5;
    private const float VOLUME_REDUCTION_FACTOR = 1f; // TODO: 적정 값을 찾아야 함
    private List<AudioClip> activeClips = new List<AudioClip>();

    private void ApplyVolume()
    {
        if (masterVolume == 0f)
        {
            mixer.SetFloat("SFX", -80f);
            mixer.SetFloat("BackGroundSound", -80f);
            return;
        }

        float bgVolume = masterVolume * backgroundVolume;
        float sfxVol = masterVolume * sfxVolume;

        mixer.SetFloat("BackGroundSound", bgVolume > 0 ? Mathf.Log10(bgVolume) * 20 : -80f);
        mixer.SetFloat("SFX", sfxVol > 0 ? Mathf.Log10(sfxVol) * 20 : -80f);
    }

    public void BackGroundVolume(float val)
    {
        backgroundVolume = val;
        ApplyVolume();
    }

    public void SFXVolume(float val)
    {
        sfxVolume = val;
        ApplyVolume();
    }

    public void MasterVolume(float val)
    {
        masterVolume = val;
        ApplyVolume();
    }

    public void StartDucking()
    {
        if (!isDucking)
        {
            isDucking = true;
            originalBGMVolume = backGroundSound.volume;
            backGroundSound.volume *= DUCKING_VOLUME;
        }
    }

    public void StopDucking()
    {
        if (isDucking)
        {
            isDucking = false;
            backGroundSound.volume = originalBGMVolume;
        }
    }

    public void SFXPlay(AudioClip clip, string name = "")
    {
        if (clip == null) return;

        float volume = CalculateVolume(clip);
        soundEffect.PlayOneShot(clip, volume);

        StartCoroutine(TrackActiveClip(clip));

        if (!IsDuplicateSound(clip))
        {
            StartDucking();
            StartCoroutine(StopDuckingAfterDelay(clip.length));
        }
    }
    private float CalculateVolume(AudioClip clip)
    {
        float baseVolume = sfxVolume / 2;
        if (activeClips.Count >= MAX_CONCURRENT_SOUNDS && IsDuplicateSound(clip))
        {
            return baseVolume * VOLUME_REDUCTION_FACTOR;
        }
        return baseVolume;
    }
    private IEnumerator TrackActiveClip(AudioClip clip)
    {
        activeClips.Add(clip);
        yield return new WaitForSeconds(clip.length);
        activeClips.Remove(clip);
    }

    private bool IsDuplicateSound(AudioClip clip)
    {
        return importantSoundList.IsDuplicateSound(clip);
    }

    private IEnumerator StopDuckingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopDucking();
    }


    public void PlayPlayerSFX(string playerAction)
    {
    }

    public void BackGroundPlay(AudioClip clip)
    {
        backGroundSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BackGround")[0];
        backGroundSound.clip = clip;
        backGroundSound.loop = true;
        backGroundSound.volume = 1.0f;
        backGroundSound.Play();
    }

    public void PlayTitleBGM()
    {
        BackGroundPlay(SoundList.Instance.titleBGM);
    }

    public void PlayButtonClickSFX()
    {
        SFXPlay(SoundList.Instance.buttonClick);
    }

    public void PlayBackGroundSFX(AudioClip clip)
    {
        soundEffect.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        soundEffect.clip = clip;
        soundEffect.loop = true;
        soundEffect.Play();
    }

    public void StopBackGroundSFX()
    {
        soundEffect.Stop();
    }

    public void PlayChapterBGM()
    {
    }

    public void PlayStageBGM()
    {
    }
}