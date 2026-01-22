using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Master Volume Mixer")]
    [Space(5)]
    [SerializeField] private AudioMixer menuMixer;
    private string masterVolumeParameter = "MasterVolume";

    [Header("Main Music Attributes")]
    [Space(5)]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioClip menuMusicClip;

    //[Header("SFX Attributes")]
    //[Space(5)]
    //[SerializeField] private AudioSource uiSfxSource;
    //[SerializeField] private AudioClip clickSFXClip;

    [Header("Settings")]
    [Space(5)]
    [SerializeField] private bool playOnStart;
    [SerializeField] private bool loopMenuMusic;

    [SerializeField, Range(0f, 0.5f)] private float masterVolume = 0.25f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float uiSFXVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        if (menuMusicSource == null/* || uiSfxSource == null*/)
        {
            Debug.LogError("Menu Music Sources have not been populated");
        }

        if (menuMusicSource != null)
        {
            menuMusicSource.loop = loopMenuMusic;
            menuMusicSource.volume = musicVolume;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            BeginMenuMusic();
        }
    }

    private void Update()
    {
        // IN UPDATE FOR TESTING REMOVE WHEN DONE
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
    }

    public void SetMasterVolume(float volume01)
    {
        if (menuMixer == null) return;

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

        menuMixer.SetFloat(masterVolumeParameter, db);
    }

    public float GetMasterVolume01()
    {
        if (menuMixer == null) return 1f;
        if (!menuMixer.GetFloat(masterVolumeParameter, out float db)) return 1f;

        // Convert dB back to linear 0..1
        float v = Mathf.Pow(10f, db / 20f);
        return Mathf.Clamp01(v);
    }

    private void BeginMenuMusic()
    {
        if (menuMusicSource == null/* || uiSfxSource == null*/) return;

        menuMusicSource.clip = menuMusicClip;
        menuMusicSource.loop = loopMenuMusic;
        menuMusicSource.volume = musicVolume;
        menuMusicSource.Play();
    }

    private void StopMenuMusic()
    {
        if (menuMusicSource == null) return;
        menuMusicSource.Stop();
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

    public void SetMusicVolume(float volume01)
    {
        musicVolume = Mathf.Clamp01(volume01);
        if (menuMusicSource != null) menuMusicSource.volume = musicVolume;
    }
}
