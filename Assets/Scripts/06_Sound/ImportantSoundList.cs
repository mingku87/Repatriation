using System.Collections.Generic;
using UnityEngine;

// TODO: 사용 방식이 바뀌어서 Naming 수정 필요
public class ImportantSoundList
{
    public List<AudioClip> importantSounds = new List<AudioClip>();
    public ImportantSoundList()
    {
        importantSounds.Add(SoundList.Instance.monsterHit);
    }

    public bool IsDuplicateSound(AudioClip clip)
    {
        return importantSounds.Contains(clip);
    }
}
