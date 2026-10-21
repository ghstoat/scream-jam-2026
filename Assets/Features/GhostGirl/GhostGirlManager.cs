using UnityEngine;

public class GhostGirlManager : MonoBehaviour
{
    
    [SerializeField] Animator animator;

    private const string HIDDEN = "HIDDEN";
    private const string APPEAR = "APPEAR";
    private const string JUMP = "JUMP";

    public static GhostGirlManager Instance { get; private set; }
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
    private void ResetAllTriggers()
    {
        animator.ResetTrigger(HIDDEN);
        animator.ResetTrigger(APPEAR);
        animator.ResetTrigger(JUMP);
    }

    public void Appear()
    {
        ResetAllTriggers();
        animator.SetTrigger(APPEAR);
    }

    public void Hidden()
    {
        ResetAllTriggers();
        animator.SetTrigger(HIDDEN);
    }

    public void Jump()
    {
        ResetAllTriggers();
        animator.SetTrigger(JUMP);
    }
}
