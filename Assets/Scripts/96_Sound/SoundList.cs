using UnityEngine;

[CreateAssetMenu(fileName = "SoundList", menuName = "Audio/AudioClipList")]
public class SoundList : SingletonScriptableObject<SoundList>
{
    // Chapter BGM
    public AudioClip titleBGM;
    public AudioClip chapter1BGM;

    // UI SFX
    public AudioClip buttonClick;
    public AudioClip menuOpen;
    public AudioClip menuClose;
    public AudioClip altarOpen;
    public AudioClip altarClose;
    public AudioClip altarBuy;
    public AudioClip altarEquip;
    public AudioClip altarUnequip;

    // Monster SFX
    public AudioClip monsterHit;

    // Player SFX
    public AudioClip playerHit;
    public AudioClip playerDead;

    // Item SFX
    public AudioClip timeSlow;
    public AudioClip soulGet; //
    public AudioClip elevator;//
}