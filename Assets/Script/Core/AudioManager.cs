using System;
using System.Collections.Generic;
using UnityEngine;

public enum BgmId
{
    Ui,

    Battle
}

public enum SfxId
{
    PlayerAttack,
    BlockNonBreak,
    BlockBreak,
    MonsterNonDeath,
    MonsterDeath,
    Pickup,
    PlayerHit,
    Upgrade,
    PlayerDeath,
    UiClick,
    Skill1,
    Skill2,
    Skill3,
    GameEnd,
    Boss1Death,
    Boss2Death
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    private class BgmEntry
    {
        public BgmId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = .5f;
    }

    [Serializable]
    private class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = .5f;
    }

    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private List<BgmEntry> bgmEntries = new();

    [SerializeField] private int sfxChannelCount = 8;

    [SerializeField] private List<SfxEntry> sfxEntries = new();

    private Dictionary<BgmId, BgmEntry> bgmEntriesById;
    private Dictionary<SfxId, SfxEntry> sfxEntriesById;
    private AudioSource[] sfxSources;
    private int nextSfxChannel;
    private BgmId? currentBgm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmEntriesById = new Dictionary<BgmId, BgmEntry>();
        foreach (BgmEntry entry in bgmEntries)
        {
            bgmEntriesById[entry.id] = entry;
        }

        sfxEntriesById = new Dictionary<SfxId, SfxEntry>();
        foreach (SfxEntry entry in sfxEntries)
        {
            sfxEntriesById[entry.id] = entry;
        }

        sfxSources = new AudioSource[sfxChannelCount];
        for (int i = 0; i < sfxChannelCount; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxSources[i] = source;
        }

        PlayBgm(BgmId.Ui);
    }

    public void PlayBgm(BgmId id)
    {
        if (currentBgm == id)
        {
            return;
        }

        if (!bgmEntriesById.TryGetValue(id, out BgmEntry entry) || entry.clip == null)
        {
            return;
        }

        currentBgm = id;
        bgmSource.clip = entry.clip;
        bgmSource.volume = entry.volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        currentBgm = null;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySfx(SfxId id)
    {
        if (!sfxEntriesById.TryGetValue(id, out SfxEntry entry) || entry.clip == null)
        {
            return;
        }

        AudioSource source = sfxSources[nextSfxChannel];
        nextSfxChannel = (nextSfxChannel + 1) % sfxSources.Length;
        source.PlayOneShot(entry.clip, entry.volume);
    }
}
