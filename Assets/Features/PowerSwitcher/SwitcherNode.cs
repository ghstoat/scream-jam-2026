using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

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

    [SerializeField] GameObject ringLight;
    [SerializeField] Renderer ringLightRenderer;

    [SerializeField] float minRingLightIntensity = 1f;
    [SerializeField] float maxRingLightIntensity = 5f;
    [SerializeField] Color ringLightColor = Color.orange;
    private static readonly int EmissionColorID = Shader.PropertyToID("_Emission");


    private bool broken = false;

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


        Material ringMat = ringLightRenderer.material;
        float intensity = (ratio * (maxRingLightIntensity - minRingLightIntensity)) + minRingLightIntensity;
        Color emission = ringLightColor * intensity;
        ringMat.SetColor(EmissionColorID, emission);
        //pointLight.color = color;
    }

    public override void OnDragStart()
    {
        if (broken) return;

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

    public void SetBroken(bool broken)
    {
        this.broken = broken;
        Lock(broken);
        ringLight.SetActive(!broken);
    }
    public void ResetState()
    {
        state = NodeState.None;
        sfxSource.clip = jackOut;
        sfxSource.Play();
    }

}
