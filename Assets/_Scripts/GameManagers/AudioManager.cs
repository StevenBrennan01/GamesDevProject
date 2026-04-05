using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Master Volume Mixer")]
    [Space(5)]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    [Header("Music Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameMusicClip;

    [Header("UI SFX Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private AudioClip clickSFXClip;

    [Header("Settings")]
    [Space(5)]
    [SerializeField] private bool resetPlayerPrefs;
    [SerializeField] private bool playMenuMusicOnStart;
    [SerializeField] private bool loopMenuMusic;

    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float musicFadeStorer = 1f;

    // PlayerPrefs keys
    private const string MasterKey = "audio.master";
    private const string MusicKey = "audio.music";
    private const string SfxKey = "audio.uisfx";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Use below to reset player saved prefs
        if (resetPlayerPrefs)
        {
            PlayerPrefs.DeleteKey(MasterKey);
            PlayerPrefs.DeleteKey(MusicKey);
            PlayerPrefs.DeleteKey(SfxKey);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        LoadVolumes();

        ApplySceneMusic(sceneName);
    }
    private void Start()
    {
        ApplyVolumes();
        StartCoroutine(ApplyVolumesNextFrame());
    }

    private System.Collections.IEnumerator ApplyVolumesNextFrame()
    {
        yield return null;
        ApplyVolumes();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplySceneMusic(newScene.name);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }
    }

    private void ApplySceneMusic(string sceneName)
    {
        if (musicSource == null) return;

        if (sceneName == "MainMenu")
        {
            if (playMenuMusicOnStart && menuMusicClip != null)
            {
                BeginMusic(musicSource, menuMusicClip);
            }
        }
        else if (sceneName == "FYPLevel")
        {
            if (gameMusicClip != null)
            {
                BeginMusic(musicSource, gameMusicClip);
            }
        }
        // else: do nothing for other scenes for now
    }

    // -------- Saving / Loading --------
    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxKey, sfxVolume);
    }

    public void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        // Master via mixer parameter
        SetMasterVolume(masterVolume);

        // Music: prefer mixer if you exposed MusicVolume; else fall back to source.volume
        if (audioMixer != null && !string.IsNullOrWhiteSpace(musicVolumeParameter))
        {
            SetMixerVolume(musicVolumeParameter, musicVolume * musicFadeStorer);
        }
        else
        {
            SetMusicVolume(musicVolume);
        }

        // UI SFX: simplest is source volume (or expose UiSfxVolume in mixer later)
        if (audioMixer != null && !string.IsNullOrWhiteSpace(sfxVolumeParameter))
        {
            SetMixerVolume(sfxVolumeParameter, sfxVolume);
        }
    }

    // -------- Volume setters (called by sliders) --------
    public void SetMusicVolume(float music01)
    {
        musicVolume = Mathf.Clamp01(music01);

        if (audioMixer != null && !string.IsNullOrWhiteSpace(musicVolumeParameter))
        {
            SetMixerVolume(musicVolumeParameter, musicVolume * musicFadeStorer);
        }
    }

    public void SetMasterVolume(float master01)
    {
        masterVolume = Mathf.Clamp01(master01);
        SetMixerVolume(masterVolumeParameter, masterVolume);
    }

    public void SetSfxVolume(float sfx01)
    {
        sfxVolume = Mathf.Clamp01(sfx01);
        SetMixerVolume (sfxVolumeParameter, sfxVolume);
    }

    // Read-only getters (useful for setting slider initial values)
    public float MasterVolume01 => masterVolume;
    public float MusicVolume01 => musicVolume;
    public float SfxVolume01 => sfxVolume;

    private void SetMixerVolume(string parameter, float volume01)
    {
        if (audioMixer == null) return;
        if (string.IsNullOrWhiteSpace(parameter)) return;

        volume01 = Mathf.Clamp01(volume01);

        float db = (volume01 < 0.0001f) ? -80f : Mathf.Log10(volume01) * 20f;

        bool ok = audioMixer.SetFloat(parameter, db);
        if (!ok)
        {
            Debug.LogWarning($"AudioMixer parameter '{parameter}' not found/exposed.", this);
        }
    }

    private void BeginMusic(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;

        if (source.isPlaying && source.clip == clip) return;

        source.clip = clip;
        source.loop = loopMenuMusic;

        //ApplyVolumes();
        source.Play();
    }

    private void StopMusic(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
    }

    //private void PlayOneShotHover()
    //{
    //    uiSfxSource.PlayOneShot(hoverSFXClip, uiSFXVolume);
    //    // play one shot hover sound here
    //}

    public void PlaySfxPreview()
    {
        PlayOneShotClick();
    }

    public void PlayOneShotClick()
    {
        if (uiSfxSource == null || clickSFXClip == null) return;
        uiSfxSource.PlayOneShot(clickSFXClip, sfxVolume);
    }

    public void FadeMusic(float targetFade01, float fadeSeconds)
    {
        targetFade01 = Mathf.Clamp01(targetFade01);

        if (audioMixer == null || string.IsNullOrWhiteSpace(musicVolumeParameter)) return;

        float startFade = musicFadeStorer;

        LeanTween.value(gameObject, startFade, targetFade01, fadeSeconds)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate(f =>
            {
                musicFadeStorer = f;
                SetMixerVolume(musicVolumeParameter, musicVolume * musicFadeStorer);
            });
    }

    public void RestoreMusicInstant()
    {
        musicFadeStorer = 1f;
        ApplyVolumes();
    }
}