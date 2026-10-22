using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;

[Serializable]
public class SFX
{
    public AudioClip clip;
    public float volume;
}



public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] GameObject instanceContainer;
    [SerializeField] GameObject sfxInstancePrefab;
    [OnValueChanged("InitSFXInstances")] [SerializeField] int sfxInstanceCount;

    private List<SFXInstance> instanceList = new List<SFXInstance>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(this.gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        InitSFXInstances();

    }

    private void InitSFXInstances()
    {
        instanceList.Clear();
        for (int i = instanceContainer.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(instanceContainer.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < sfxInstanceCount; i++)
        {
            AddNewSFXInstance();
        }

    }

    private void AddNewSFXInstance()
    {
        GameObject sfxGO = Instantiate(sfxInstancePrefab, instanceContainer.transform);

        SFXInstance sfx = sfxGO.GetComponent<SFXInstance>();

        instanceList.Add(sfx);
    }

    public void PlaySFX(AudioClip clip, float volume, bool overwriteDuplicateClips, string overwriteKey = null)
    {
        if (clip == null)
        {
            Debug.LogError("!!! Tried to play clip with no reference");
            return;
        }

        string key = overwriteKey;

        if (key == null) key = clip.name;

        if (overwriteDuplicateClips) //try to override duplicate clips
        {
            foreach (SFXInstance sfx in instanceList)
            {
                if (sfx.ClipKey == key)
                {
                    sfx.PlayClip(clip, volume, key);
                    return;
                }
            }
        }

        foreach (SFXInstance sfx in instanceList) //search for empty clips
        {
            if (!sfx.IsPlaying)
            {
                sfx.PlayClip(clip, volume, key);
                return;
            }
        }

        Debug.LogWarning($"!!! SFX Manager has run out of SFX Instances! | Currently at {instanceList.Count} SFX Instances | Creating a new SFX Instance to accomodate");

        AddNewSFXInstance();

        instanceList[^1].PlayClip(clip, volume, key);
    }

}
