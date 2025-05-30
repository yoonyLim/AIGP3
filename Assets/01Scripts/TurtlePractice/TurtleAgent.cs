using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class TurtleAgent : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    
    private Renderer _renderer;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    
    public override void Initialize()
    {
        Debug.Log("Initialize");
        
        base.Initialize();
        
        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        
        _currentEpisode++;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects(); // reposition objects
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 015f, 0f);

        // random y-axis direction (angle in degrees)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        
        // random distance
        float randomDistance = Random.Range(1f, 2.5f);
        
        // goal's postion
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;
        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }
    
    // the values to be passed into vector sensor works the bes in range [-1, 1] for machine learning
    // thus the need for normalization
    public override void CollectObservations(VectorSensor sensor)
    {
        // goal's position
        float goalPosNormalizedX = _goal.localPosition.x / 5f;
        float goalPosNormalizedZ = _goal.localPosition.z / 5f;
        
        // turtle's position
        float turtlePosNormalizedX = transform.localPosition.x / 5f;
        float turtlePosNormalizedZ = transform.localPosition.z / 5f;
        
        // turtle's rotation
        float turtleRotationNormalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        
        sensor.AddObservation(goalPosNormalizedX);
        sensor.AddObservation(goalPosNormalizedZ);
        sensor.AddObservation(turtlePosNormalizedX);
        sensor.AddObservation(turtlePosNormalizedZ);
        sensor.AddObservation(turtleRotationNormalized);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions); // move turtle
        
        AddReward(-2f / MaxStep); // penalize for taking actions
        
        _cumulativeReward = GetCumulativeReward(); // get cumulative reward
    }

    public void MoveAgent(ActionSegment<int> action)
    {
        var chosenAction = action[0];

        switch (chosenAction)
        {
            case 1:
                transform.position += transform.forward * _moveSpeed * Time.deltaTime; // move forward
                break;
            case 2:
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f); // rotate left
                break;
            case 3:
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f); // rotate right
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
            GoalReached();
    }

    private void GoalReached()
    {
        AddReward(1f);
        _cumulativeReward = GetCumulativeReward();
        
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f); // penalize for colliding with wall
            
            if (_renderer)
                _renderer.material.color = Color.red;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
            AddReward(-0.01f * Time.fixedDeltaTime); // penalize for the time of colliding with wall
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && _renderer)
            _renderer.material.color = Color.blue;
    }
}
