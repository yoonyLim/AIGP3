using UnityEngine;

public interface IAgent
{
    BaseAgent GetAgent();
    AgentType GetAgentType();

    Quaternion GetLocalRot();
    Vector3 GetLocalPos();
    void MoveTo(Vector3 destination, AgentMoveType moveType);
    bool HasArrived(Vector3 destination, float threshold = 2.0f); 
    void Dodge(Vector3 movement);

    void ResetCooldown(string key, Blackboard blackboard, float duration);
}
