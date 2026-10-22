using System.Collections;
using UnityEngine;

public class SFXInstance : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    private string clipKey;
    private Coroutine playRoutine = null;

    public string ClipKey => clipKey;
    public bool IsPlaying => playRoutine != null;

    private void Awake()
    {
        ResetToEmpty();
    }

    public void PlayClip(AudioClip clip, float volume, string key)
    {
        Stop();

        if (clip == null)
        {
            Debug.LogWarning($"{name}: Tried to play a null AudioClip.");
            return;
        }

        playRoutine = StartCoroutine(PlayRoutine(clip, volume, key));
    }

    private IEnumerator PlayRoutine(AudioClip clip, float volume, string key)
    {
        source.clip = clip;
        source.volume = volume;
        clipKey = key;

        source.Play();

        yield return new WaitForSecondsRealtime(clip.length);

        if (source.clip == clip && source.isPlaying)
        {
            source.Stop();
        }

        ResetToEmpty();

        playRoutine = null;
    }

    public void Stop()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (source.isPlaying)
        {
            source.Stop();
        }

        ResetToEmpty();
    }

    private void ResetToEmpty()
    {
        clipKey = null;
    }
}