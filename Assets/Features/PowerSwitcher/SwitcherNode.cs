using UnityEngine;

public enum NodeState
{
    None,
    A,
    B
}



public class SwitcherNode : Interactable
{
    public NodeState state {  get; private set; } = NodeState.None;
    private PowerSwitcher ctx;

    [SerializeField] Transform jackPosTransform;
    public Vector3 JackPos => jackPosTransform.position;

    private float powerRatio;
    [SerializeField] float maxLightIntensity = 0.1f;
    [SerializeField] Light light;

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip jackIn;
    [SerializeField] AudioClip jackOut;

    public void Init(PowerSwitcher powerSwitcher)
    {
        ctx = powerSwitcher;
        state = NodeState.None;

    }

    public void UpdatePowerRatio(float ratio)
    {
        powerRatio = ratio; 

        float lightIntensity = maxLightIntensity * powerRatio;
        light.intensity = lightIntensity;
    }

    public override void OnDragStart()
    {
        base.OnDragStart();

        if (state == NodeState.None)
        {
            ctx.SetNextNodeState(this);
        }
        else
        {
            ctx.ClearNodeState(this);
        }

        OnDragExit();
    }

    public void SetState(NodeState state) //called by ctx
    {
        this.state = state;
        sfxSource.clip = jackIn;
        sfxSource.Play();
    }


    public void ResetState()
    {
        state = NodeState.None;
        sfxSource.clip = jackOut;
        sfxSource.Play();
    }

}
