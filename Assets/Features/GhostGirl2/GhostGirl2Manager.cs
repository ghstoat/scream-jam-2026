using UnityEngine;

public class GhostGirl2Manager : MonoBehaviour
{
    public static GhostGirl2Manager Instance { get; private set; }

    [SerializeField] Animator animator;
    [SerializeField] AudioSource source;

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

    public void Jump()  
    {
        animator.SetTrigger("JUMP");
        source.Play();
    }
}
