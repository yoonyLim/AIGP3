using UnityEngine;

public interface IAgent
{
    AgentType GetAgentType();

    Vector3 GetPosition();
    void MoveTo(Vector3 destination, AgentMoveType moveType);
    bool HasArrived(Vector3 destination, float threshold = 2.0f); 
}
