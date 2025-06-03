using UnityEngine;

public interface IAgent
{
    BaseAgent GetAgent();
    AgentType GetAgentType();

    Quaternion GetLocalRot();
    Vector3 GetLocalPos();
    Vector3 GetWorldPos(); // for raycasting
    void MoveTo(Vector3 destination, AgentMoveType moveType);
    bool HasArrived(Vector3 destination, float threshold = 2.0f); 
    void Dodge(Vector3 movement);
    void Strafe(Vector3 centerPos, float radius = 3f, float angularSpeed = 90f, int direction = 1);

    void ResetCooldown(string key, Blackboard blackboard, float duration);
}
