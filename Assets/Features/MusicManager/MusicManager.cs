using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;


public enum MusicControls
{
    None,
    PlayMusic,
    StopMusic,
    RestartMusic,
    PauseMusic,
    ResumeMusic
}

[Serializable]
public class MusicController
{
    public MusicControls musicController = MusicControls.None;
    [EnableIf("UsingPlayMusic")][AllowNesting] public AudioClip music;
    [Tooltip("Recommend 0.25 for now")]
    [EnableIf("UsingPlayMusic")][AllowNesting] public float volume;
    [Tooltip("Recommend 1")]
    [EnableIf("ShowFadeDurationField")][AllowNesting] public float fadeDuration;
    [Tooltip("Recommend 0")]
    [EnableIf("ShowStartOffsetField")][AllowNesting] public float startOffset;
    [Tooltip("Recommend True")]
    [EnableIf("UsingPlayMusic")][AllowNesting] public bool loop;
    [Tooltip("Recommend False")]
    [EnableIf("UsingPlayMusic")][AllowNesting] public bool restartMusicIfAlreadyPlaying;

    private bool UsingPlayMusic() { return musicController == MusicControls.PlayMusic; }
    private bool ShowFadeDurationField() { return musicController != MusicControls.None; }
    private bool ShowStartOffsetField() { return (musicController == MusicControls.PlayMusic || musicController == MusicControls.RestartMusic); }

    public void PlayMusic()
    {
        switch (musicController)
        {
            case MusicControls.PlayMusic:
                MusicManager.Instance.PlayMusic(music, volume, startOffset, fadeDuration, loop, restartMusicIfAlreadyPlaying); break;
            case MusicControls.StopMusic:
                MusicManager.Instance.StopMusic(fadeDuration); break;
            case MusicControls.RestartMusic:
                MusicManager.Instance.RestartMusic(fadeDuration, startOffset); break;
            case MusicControls.PauseMusic:
                MusicManager.Instance.PauseMusic(fadeDuration); break;
            case MusicControls.ResumeMusic:
                MusicManager.Instance.ResumeMusic(fadeDuration); break;
        }
    }
}


public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] AudioSource musicSource;

    private Coroutine musicRoutine;
    private float targetVolume = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void PlayMusic(AudioClip clip, float volume, float startOffset, float fadeDuration, bool loop, bool restartMusicIfAlreadyPlaying)
    {

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.volume = volume;

            musicSource.loop = loop;

            if (restartMusicIfAlreadyPlaying) RestartMusic(fadeDuration, startOffset);

            return;
        }

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        targetVolume = volume;

        musicRoutine = StartCoroutine(ChangeMusic(clip,startOffset, fadeDuration, loop));
    }


    private IEnumerator ChangeMusic(AudioClip newClip, float startOffset, float fadeDuration, bool loop)
    {
        if (musicSource.isPlaying)
        {
            yield return FadeVolume(musicSource.volume, 0f, fadeDuration);

            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.time = Mathf.Clamp(startOffset,0f,newClip.length);

        musicSource.volume = 0f;
        musicSource.Play();

        yield return FadeVolume( 0f, targetVolume,fadeDuration );

        musicRoutine = null;
    }


    private IEnumerator FadeVolume(float startVolume, float endVolume, float duration)
    {
        if (duration <= 0f)
        {
            musicSource.volume = endVolume;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / duration;

            musicSource.volume = Mathf.Lerp(
                startVolume,
                endVolume,
                t
            );

            yield return null;
        }

        musicSource.volume = endVolume;
    }


    public void StopMusic(float fadeDuration)
    {
        if (musicSource.clip == null) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(StopMusicRoutine(fadeDuration));
    }


    private IEnumerator StopMusicRoutine(float fadeDuration)
    {
        yield return FadeVolume( musicSource.volume, 0f, fadeDuration);

        musicSource.Stop();
        musicSource.clip = null;

        musicRoutine = null;
    }


    public void RestartMusic(float fadeDuration, float startOffset)
    {
        if (musicSource.clip == null) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(RestartMusicRoutine(fadeDuration, startOffset));
    }


    private IEnumerator RestartMusicRoutine(float fadeDuration, float startOffset)
    {
        yield return FadeVolume( musicSource.volume, 0f, fadeDuration);

        musicSource.time = Mathf.Clamp( startOffset, 0f, musicSource.clip.length);

        musicSource.Play();

        yield return FadeVolume( 0f, targetVolume, fadeDuration);

        musicRoutine = null;
    }


    public void PauseMusic(float fadeDuration)
    {
        if (!musicSource.isPlaying) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PauseMusicRoutine(fadeDuration));
    }


    private IEnumerator PauseMusicRoutine(float fadeDuration)
    {
        yield return FadeVolume( musicSource.volume, 0f,fadeDuration);

        musicSource.Pause();
        musicRoutine = null;
    }


    public void ResumeMusic(float fadeDuration)
    {
        if (musicSource.clip == null || musicSource.isPlaying)return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(ResumeMusicRoutine(fadeDuration));
    }


    private IEnumerator ResumeMusicRoutine(float fadeDuration)
    {
        musicSource.Play();

        yield return FadeVolume( 0f, targetVolume, fadeDuration);

        musicRoutine = null;
    }
}