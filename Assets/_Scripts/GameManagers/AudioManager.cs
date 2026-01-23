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
    private string masterVolumeParameter = "MasterVolume";

    [Header("Music Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameMusicClip;

    [Header("UI SFX Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private AudioClip clickSFXClip;

    [Header("Player SFX Attributes")]
    [Space(5)]


    [Header("Settings")]
    [Space(5)]
    [SerializeField] private bool playMenuMusicOnStart;
    [SerializeField] private bool loopMenuMusic;

    [SerializeField, Range(0f, 0.5f)] private float masterVolume = 0.25f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float uiSFXVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            if (playMenuMusicOnStart)
            {
                BeginMusic(musicSource, menuMusicClip);
            }
        }
        if (sceneName == "FYPLevel")
        {
            BeginMusic(musicSource, gameMusicClip);
        }
    }

    private void Update()
    {
        // IN UPDATE FOR TESTING REMOVE WHEN DONE
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
    }

    public void SetMusicVolume(float volume01)
    {
        musicVolume = Mathf.Clamp01(volume01);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetMasterVolume(float volume01)
    {
        if (audioMixer == null) return;

        volume01 = Mathf.Clamp01(volume01);

        float db;
        if (volume01 < 0.0001f)
        {
            db = -80f;
        }
        else
        {
            db = Mathf.Log10(volume01) * 20f;
        }

        audioMixer.SetFloat(masterVolumeParameter, db);
    }

    public float GetMasterVolume01()
    {
        if (audioMixer == null) return 1f;
        if (!audioMixer.GetFloat(masterVolumeParameter, out float db)) return 1f;

        // Convert dB back to linear 0..1
        float v = Mathf.Pow(10f, db / 20f);
        return Mathf.Clamp01(v);
    }

    private void BeginMusic(AudioSource source, AudioClip clip)
    {
        if (source == null) return;

        source.clip = clip;
        source.loop = loopMenuMusic;
        source.volume = musicVolume;
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

    public void PlayOneShotClick()
    {
        //uiSfxSource.PlayOneShot(clickSFXClip, uiSFXVolume);
        Debug.Log("A button has indeedy been clicked");
    }

    public void PlayOneShotFootstep()
    {

    }
}