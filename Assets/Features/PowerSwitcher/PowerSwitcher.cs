using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
public class NodeData
{
    public SwitcherNode node;
    public PoweredSystem poweredSystem;
}


public class PowerSwitcher : MonoBehaviour
{
    [Header("Lever Reference")]
    [SerializeField] ResettingLever powerResetLever;
    public ResettingLever PowerResetLever => powerResetLever;

    [Header("Node References")]
    [SerializeField] private Transform nodeParent;
    [SerializeField] private List<NodeData> nodes = new();
    private Dictionary<int, PoweredSystem> poweredSystemDict = new();

    [Header("Grid Settings")]
    [SerializeField, Min(1)] private int columns = 4;
    [SerializeField, Min(1)] private int rows = 3;
    [SerializeField, Min(0f)] private float columnSpacing = 2f;
    [SerializeField, Min(0f)] private float rowSpacing = 2f;

    [Header("Node Prefab")]
    [SerializeField] private GameObject nodePrefab;

    [Header("Valve")]
    [SerializeField] float valveStrength;
    [SerializeField] Rotatable valve;
    [SerializeField] AudioSource valveSpin;
    [SerializeField] AudioSource gearLoop;

    [Header("Gauges")]
    [SerializeField] Gauge reservoirGauge;
    [SerializeField] Gauge nodeAGauge;
    [SerializeField] Gauge nodeBGauge;

    [Header("Reservoir")]
    [SerializeField] SwitcherNode reservoirNode;
    public SwitcherNode ReservoirNode => reservoirNode;

    [Header("Jack Assignment")]
    [SerializeField, Min(0.01f)] private float jackMoveDuration = 0.25f;
    private Coroutine jackARoutine;
    private Coroutine jackBRoutine;

    [SerializeField] GameObject jackA;
    [SerializeField] GameObject jackB;

    private Vector3 defJackAPos;
    private Vector3 defJackBPos;

    private SwitcherNode pointA;
    private SwitcherNode pointB;

    [SerializeField] GameObject tagA;
    [SerializeField] GameObject tagB;

    [Header("Player Dialogue")]
    [SerializeField] PlayerDialogue jacksNotAssigned;


    public NodeState lastState = NodeState.None;

    private void Start()
    {
        CreateNodes();

        DeclareDict();

        foreach (NodeData node in nodes)
        {
            node.node.Init(this);
        }

        defJackAPos = jackA.transform.position;
        defJackBPos = jackB.transform.position;

        DisconnectJackA();
        DisconnectJackB();

    }

    private void Update()
    {
        UpdateNodeLights();
        UpdateValveRotation();
        UpdateGauges();
    }

    public void DrainAllSystems()
    {
        CockpitManager.Instance.SetState(CockpitState.Reset);

        PowerManager.Instance.DrainAllSystems();
    }

    private void DeclareDict()
    {
        poweredSystemDict.Clear();

        poweredSystemDict = new Dictionary<int, PoweredSystem>
        {
            { 0, PowerReservoir.Instance },
            { 2, OxygenManager.Instance },
            { 6, VentManager.Instance },
            { 7, RadioManager.Instance },
            { 11, TemperatureManager.Instance },
            { 15, FlashManager.Instance }
        };

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].node.Init(this);

            if (poweredSystemDict.ContainsKey(i))
            {
                PoweredSystem system = poweredSystemDict[i];
                nodes[i].poweredSystem = system;

                nodes[i].node.SetBroken(false);

            }
            else
            {
                nodes[i].node.SetBroken(true);
            }
        }

    }

    private void UpdateValveRotation()
    {

        if (valve.RotationDelta== 0)
        {
            if (valveSpin.volume != 0) valveSpin.volume = 0;
            if (gearLoop.volume != 0) gearLoop.volume = 0;
            return;

        }
        
        float ratio = Mathf.Abs(valve.RotationDelta) / 15;

        float vol = ratio * 1;
        valveSpin.volume = vol;
        
            

        if (!PowerManager.Instance.IsSystemsABAssigned())
        {
            PlayerDialogueManager.Instance.ForceDialogue(jacksNotAssigned);
            if (gearLoop.volume != 0) gearLoop.volume = 0;

            return;
        }



        if (PowerManager.Instance.CanMovePowerFromAToB(-valve.RotationDelta) )
        {
            gearLoop.volume = vol;
        }
        else
        {
            if (gearLoop.volume != 0) gearLoop.volume = 0;
        }

        PowerManager.Instance.MovePowerFromAToB(-(int)(valve.RotationDelta * valveStrength));
    }

    private void UpdateNodeLights()
    {
        foreach (NodeData nodeData in nodes)
        {
            if (nodeData.poweredSystem == null) continue;

            nodeData.node.UpdatePowerRatio(nodeData.poweredSystem.PowerRatio);
        }
    }

    private void UpdateGauges()
    {
        reservoirGauge.SetTargetAngleFromRatio(PowerReservoir.Instance.PowerRatio);

        PoweredSystem poweredSystemA = GetPoweredSystemFromNode(pointA);
        if (poweredSystemA == null) nodeAGauge.SetTargetAngleFromRatio(0);
        else nodeAGauge.SetTargetAngleFromRatio(poweredSystemA.PowerRatio);

        PoweredSystem poweredSystemB = GetPoweredSystemFromNode(pointB);
        if (poweredSystemB == null) nodeBGauge.SetTargetAngleFromRatio(0);
        else nodeBGauge.SetTargetAngleFromRatio(poweredSystemB.PowerRatio);
    }

    private PoweredSystem GetPoweredSystemFromNode(SwitcherNode node)
    {
        foreach (NodeData n in nodes)
        {
            if (n.node == node)
            {
                return n.poweredSystem;
            }
        }

        return null;
    }

    [Button]
    private void CreateNodes()
    {
        ClearNodes();

        nodes.Add(new NodeData());
        nodes[0].node = reservoirNode;

        int index = 1;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject node = Instantiate(nodePrefab, nodeParent);

                node.name = $"Node_{row}_{column}";

                node.transform.localPosition = new Vector3(column * columnSpacing, -row * rowSpacing, 0f);

                node.transform.localRotation = Quaternion.identity;
                node.transform.localScale = Vector3.one;

                SwitcherNode nodeScript = node.GetComponentInChildren<SwitcherNode>();
                nodeScript.Init(this);
                nodes.Add(new NodeData());

                nodes[index].node = nodeScript;
                index++;
            }
        }

        Debug.Log($"Created {nodes.Count} nodes ({columns} columns x {rows} rows).", this);
    }

    public void SetNextNodeState(SwitcherNode node)
    {
        NodeState state = GetNextState();

        foreach (NodeData n in nodes)
        {
            if (n.node.state == state)
            {
                n.node.ResetState();
                break;
            }
        }

        if (state == NodeState.A) ConnectJackA(node);
        else if (state == NodeState.B) ConnectJackB(node);

        lastState = state;
        node.SetState(state);
    }

    private NodeState GetNextState()
    {
        if (pointA == null) return NodeState.A;

        if (pointB == null) return NodeState.B;

        if (lastState == NodeState.A) return NodeState.B;
        return NodeState.A;
    }

    public void ClearNodeState(SwitcherNode node)
    {
        if (node.state == NodeState.A) DisconnectJackA();
        else if (node.state == NodeState.B) DisconnectJackB();

        node.ResetState();
    }

    public void ConnectJackA(SwitcherNode node)
    {
        if (jackARoutine != null) StopCoroutine(jackARoutine);
        jackARoutine = StartCoroutine(JackMovementRoutine(jackA.transform, node.JackPos, true, node));
    }

    public void ConnectJackB(SwitcherNode node)
    {
        if (jackBRoutine != null) StopCoroutine(jackBRoutine);
        jackBRoutine = StartCoroutine(JackMovementRoutine(jackB.transform, node.JackPos, false, node));
    }

    public void DisconnectJackA() 
    { 
        pointA = null;

        if (jackARoutine != null) StopCoroutine(jackARoutine);
        jackARoutine = StartCoroutine(JackMovementRoutine(jackA.transform, defJackAPos, true, null));
    }
    public void DisconnectJackB()
    {
        pointB = null;

        if (jackBRoutine != null) StopCoroutine(jackBRoutine);
        jackBRoutine = StartCoroutine(JackMovementRoutine(jackB.transform, defJackBPos, false, null));
    }

    public void SetPointA(SwitcherNode node)
    {
        pointA = node;
        bool found = false;

        foreach (NodeData nodeData in nodes)
        {
            if (nodeData.node != node) continue;

            found = true;
            PowerManager.Instance.SetSystemA(nodeData.poweredSystem);

            tagA.SetActive(true);
        }

        if (!found) tagA.SetActive(false);

    }

    public void SetPointB(SwitcherNode node)
    {
        pointB = node;
        bool found = false;

        foreach (NodeData nodeData in nodes)
        {
            if (nodeData.node != node) continue;

            found = true;
            PowerManager.Instance.SetSystemB(nodeData.poweredSystem);

            tagB.SetActive(true);

        }

        if (!found) tagB.SetActive(false);
    }
    public void ClearPointA()
    {
        pointA = null;
        PowerManager.Instance.SetSystemA(null);
        tagA.SetActive(false);

    }


    public void ClearPointB()
    {
        pointB = null;
        PowerManager.Instance.SetSystemB(null);
        tagB.SetActive(false);
    }

    private IEnumerator JackMovementRoutine(Transform jack, Vector3 targetPosition, bool isJackA, SwitcherNode node)
    {
        if (isJackA) ClearPointA();
        else ClearPointB();

        Vector3 startPosition = jack.position;
        float elapsed = 0f;

        while (elapsed < jackMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / jackMoveDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            jack.position = Vector3.Lerp(startPosition,targetPosition,t);

            yield return null;
        }

        jack.position = targetPosition;

        if (node != null)
        {
            if (isJackA) SetPointA(node);
            else SetPointB(node);   
        }

        if (isJackA) jackARoutine = null;
        else jackBRoutine = null;
    }

    [Button]
    private void ClearNodes()
    {
        pointA = null;
        pointB = null;

        for (int i = nodeParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(nodeParent.GetChild(i).gameObject);
        }

        nodes.Clear();
        lastState = NodeState.None;
    }


}