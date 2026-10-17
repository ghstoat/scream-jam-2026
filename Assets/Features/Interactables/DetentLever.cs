using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DetentLever : Lever
{
    [Header("Reset")]
    [SerializeField] private float resetTime = 0.25f;

    private float targetPull;
    private Coroutine resetCoroutine;
    private bool isResetting;

    public UnityEvent OnDown;
    public UnityEvent OnUp;

    public override void OnDrag(Vector2 mousePosition)
    {
        if (isResetting) return;

        float previousPull = currentPull;
        base.OnDrag(mousePosition);

        if (previousPull < 1f && currentPull >= 1f)
        {
            targetPull = 1f;
            OnDown?.Invoke();
        }
        else if (previousPull > 0f && currentPull <= 0f)
        {
            targetPull = 0f;
            OnUp?.Invoke();
        }
    }

    public override void OnDragExit()
    {
        base.OnDragExit();

        if (resetCoroutine != null) StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetToEndpoint());
    }

    private IEnumerator ResetToEndpoint()
    {
        isResetting = true;

        float startPull = currentPull;
        float elapsed = 0f;

        while (elapsed < resetTime)
        {
            elapsed += Time.deltaTime;

            currentPull = Mathf.Lerp(startPull, targetPull, Mathf.SmoothStep(0f, 1f, elapsed / resetTime));

            UpdateLeverPosition();
            yield return null;
        }

        currentPull = targetPull;
        UpdateLeverPosition();

        isResetting = false;
        resetCoroutine = null;
    }
}