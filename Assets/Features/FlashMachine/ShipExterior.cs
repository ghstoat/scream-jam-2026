using UnityEngine;
using UnityEngine.Events;


public class ShipExterior : MonoBehaviour
{
    [SerializeField] Animator flashAnimator;

    private const string FLASH = "FLASH";

    private void Start()
    {
        FlashManager.Instance.onFlash.AddListener(onFlash);
    }

    private void OnDisable()
    {
        FlashManager.Instance.onFlash.RemoveListener(onFlash);

    }

    private void onFlash()
    {
        flashAnimator.SetTrigger(FLASH);
    }

}
