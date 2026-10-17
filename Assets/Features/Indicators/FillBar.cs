using UnityEngine;

public class FillBar : MonoBehaviour
{
    [SerializeField] GameObject fill;
    
    private float maxScaleY;

    private float targetScaleY;
    private float scaleY;
    [SerializeField] float fillSpeed;

    private void Start()
    {
        maxScaleY = transform.localScale.y;
    }

    public void SetTargetScaleYFromRatio(float ratio)
    {
        float scaleRatio = Mathf.Clamp01(ratio);
        targetScaleY = scaleRatio * maxScaleY;


    }

    private void Update()
    {
        scaleY = Mathf.MoveTowards(scaleY, targetScaleY, fillSpeed * Time.deltaTime);

        Vector3 currentScale = fill.transform.localScale;
        currentScale.y = scaleY;
        fill.transform.localScale = currentScale;
    }
}
