using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ResettingLever : Lever
{
    [Header("Reset")]
    [SerializeField, Min(0.01f)] private float hardResetTime = 1.5f;
    [SerializeField, Min(0.01f)] private float softResetTime = 1f;

    [Header("Events")]
    public UnityEvent leverPulled;

    private Coroutine resetCoroutine;
    private bool isHardResetting;
    private bool isSoftResetting;

    public override void OnDragStart()
    {
        if (isHardResetting) return;

        if (isSoftResetting)
        {
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            resetCoroutine = null;
            isSoftResetting = false;
        }

        base.OnDragStart();
    }

    public override void OnDrag(Vector2 mousePosition)
    {
        if (isHardResetting) return;

        float previousPull = currentPull;
        base.OnDrag(mousePosition);

        if (previousPull < 1f && currentPull >= 1f) CompletePull();
    }

    public override void OnDragExit()
    {
        base.OnDragExit();

        if (!isInteractable || isHardResetting || isSoftResetting || currentPull <= 0f) return;

        isSoftResetting = true;
        resetCoroutine = StartCoroutine(ResetLever(softResetTime, false));
    }

    private void CompletePull()
    {
        if (isHardResetting) return;

        dragInitialized = false;
        isHardResetting = true;
        SetInteractable(false);
        leverPulled?.Invoke();

        resetCoroutine = StartCoroutine(ResetLever(hardResetTime, true));
    }

    private IEnumerator ResetLever(float duration, bool hardReset)
    {
        float startPull = currentPull;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentPull = Mathf.Lerp(startPull, 0f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)));
            UpdateLeverPosition();
            yield return null;
        }

        currentPull = 0f;
        UpdateLeverPosition();
        isSoftResetting = false;
        resetCoroutine = null;

        if (hardReset)
        {
            isHardResetting = false;
            SetInteractable(true);
        }
    }
}
