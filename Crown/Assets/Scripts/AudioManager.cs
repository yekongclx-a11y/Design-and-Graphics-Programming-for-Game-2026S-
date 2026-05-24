using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Tracks")]
    public AudioClip bgmMain;
    public AudioClip bgmTheme;
    public AudioClip bgmEnding;

    [Header("Sound Effects (SFX)")]
    public AudioClip sfxNpcEnter;
    public AudioClip sfxValueUp;
    public AudioClip sfxValueDown;
    public AudioClip sfxGameOver;

    [Header("Volume Settings (Runtime values)")]
    [Range(0f, 1f)] public float musicVolume = 0.15f;
    [Range(0f, 1f)] public float sfxVolume = 0.3f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 【自查修复】：初始化前优先读取本地存储的音量设置，若无则使用默认值
            musicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.3f);
            sfxVolume = PlayerPrefs.GetFloat("Settings_SFXVolume", 0.8f);
            
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    // ─────────────────────────────────────────────────────────
    // 🎛️ 核心扩展：实时音量控制与持久化总线 (供 UI Slider 调用)
    // ─────────────────────────────────────────────────────────
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
        
        // 强力写入本地缓存，WebGL 刷新不丢失
        PlayerPrefs.SetFloat("Settings_MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        
        PlayerPrefs.SetFloat("Settings_SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    // ─────────────────────────────────────────────────────────
    // 🎵 音乐/音效播放核心逻辑
    // ─────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        // 如果当前正在播放同一首 BGM，则直接跳过，防止切歌时产生卡顿爆音
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        // 播放音效时，实时动态对齐最新的音量设置数值
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // 快捷路由总线
    public void PlayMainMusic() => PlayMusic(bgmMain);
    public void PlayThemeMusic() => PlayMusic(bgmTheme);
    public void PlayEndingMusic() => PlayMusic(bgmEnding);
    public void PlayNpcEnter() => PlaySFX(sfxNpcEnter);
    public void PlayValueUp() => PlaySFX(sfxValueUp);
    public void PlayValueDown() => PlaySFX(sfxValueDown);
    public void PlayGameOver() => PlaySFX(sfxGameOver);
}