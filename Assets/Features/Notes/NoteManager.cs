using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NoteManager : MonoBehaviour
{
    [SerializeField] GameObject noteScreen;
    [SerializeField] Image note;

    private bool locked;

    [SerializeField] AudioSource openNoteSouce;

    public bool open { get; private set; } = false;
    private bool canClose = false;
    public static NoteManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"!!! Multiple {this.name} found in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        Lock(false);
        CloseNote();
    }

    private void LateUpdate()
    {
        if (canClose && open && Mouse.current.leftButton.wasPressedThisFrame) CloseNote();
    }

    public void Lock(bool locked)
    {
        this.locked = locked;
        if (locked)
        {
            CloseNote();
        }
    }

    public void OpenNote(Sprite noteSprite)
    {
        if (open || locked) return;

        canClose = false;
        open = true;
        noteScreen.SetActive(true);
        openNoteSouce.Play();
        note.sprite = noteSprite;

        StartCoroutine(Wait());
    }

    public void CloseNote()
    {
        if (!open) return;

        canClose = false;
        open = false;
        noteScreen.SetActive(false);
    }

    IEnumerator Wait()
    {
        yield return null;
        canClose = true;

    }

}
