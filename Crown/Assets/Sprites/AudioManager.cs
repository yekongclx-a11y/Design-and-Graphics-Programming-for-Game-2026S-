using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip bgmMain;
    public AudioClip bgmTheme;
    public AudioClip bgmEnding;

    [Header("SFX")]
    public AudioClip sfxNpcEnter;
    public AudioClip sfxValueUp;
    public AudioClip sfxValueDown;
    public AudioClip sfxGameOver;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayMainMusic() => PlayMusic(bgmMain);
    public void PlayThemeMusic() => PlayMusic(bgmTheme);
    public void PlayEndingMusic() => PlayMusic(bgmEnding);
    public void PlayNpcEnter() => PlaySFX(sfxNpcEnter);
    public void PlayValueUp() => PlaySFX(sfxValueUp);
    public void PlayValueDown() => PlaySFX(sfxValueDown);
    public void PlayGameOver() => PlaySFX(sfxGameOver);

    public void StopMusic()
    {
        musicSource.Stop();
    }
}