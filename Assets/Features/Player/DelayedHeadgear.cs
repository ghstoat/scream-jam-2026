
using UnityEngine;

public class DelayedHeadgear : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform headTarget;

    [Header("Follow Settings")]
    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.12f;

    [SerializeField] private Vector3 positionOffset;

    [SerializeField] private Vector3 rotationOffset;

    private Vector3 positionVelocity;

    private void LateUpdate()
    {
        if (headTarget == null) return;

        transform.position = Vector3.SmoothDamp(transform.position,headTarget.TransformPoint(positionOffset),ref positionVelocity,smoothTime);

        Quaternion targetRotation =headTarget.rotation *Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-Time.deltaTime / smoothTime));
    }
}
