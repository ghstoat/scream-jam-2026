using NaughtyAttributes;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    [SerializeField] GameObject target;
    [SerializeField] Rigidbody rb;

    private bool hasRigidbody = true;

    public float yaw;
    private float targetYaw;

    void Start()
    {

        if(rb == null)
        {
            try
            { 
                rb = gameObject.GetComponent<Rigidbody>();
            }
            catch
            {
                hasRigidbody = false;
            }

        }
    }
    
    void LateUpdate()
    {
        if(hasRigidbody)
        {
            RigidbodyRotate();
        }
        else
        {
            TransformRotate();
        }

    }

    private void RigidbodyRotate()
    {
        float opp = target.transform.position.x - transform.position.x;
        float adj = target.transform.position.z - transform.position.z;
        float radAngleToTarget = Mathf.Atan2(opp, adj);

        targetYaw = (Mathf.Rad2Deg * radAngleToTarget) + 180;

        yaw = targetYaw;

        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }

    [Button]
    private void TransformRotate()
    {
        float opp = target.transform.position.x - transform.position.x;
        float adj = target.transform.position.z - transform.position.z;
        float radAngleToTarget = Mathf.Atan2(opp, adj);
        targetYaw = (Mathf.Rad2Deg * radAngleToTarget) + 180;
        yaw = targetYaw;
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}
