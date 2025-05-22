using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TurtleAgent : Agent
{
    [SerializeField] private Transform goal;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 180f;
    
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        
    }
}
