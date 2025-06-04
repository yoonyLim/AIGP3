using System.Collections;
using UnityEditor.UI;
using UnityEngine;

public class DefenssiveBT : MonoBehaviour
{
    private INode _root;
    private readonly Blackboard _blackboard = new Blackboard();

    [SerializeField] private float blockRange = 1f;


    public DefenseAgent selfAgent;
    public AttackAgent targetAgent;
    
    void Start()
    {
        // Block 
        var blockSequence = new SequenceNode();
        blockSequence.Add(new TargetInRangeCondition(selfAgent, targetAgent, blockRange));
        blockSequence.Add(new CanBlockCondition(selfAgent, targetAgent));
        blockSequence.Add(new BlockAction(selfAgent, targetAgent));


        // Root Selector
        var rootSelector = new SelectorNode();
        rootSelector.Add(blockSequence);

        _root = rootSelector;
    }

    void Update()
    {
        _root.Evaluate();
    }
}
