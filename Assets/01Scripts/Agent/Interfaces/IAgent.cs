using UnityEngine;
using Unity.MLAgents;

public interface IAgent
{
    // Getter
    BaseAgent GetAgent();
    AgentType GetAgentType();

    Quaternion GetLocalRot();
    Vector3 GetLocalPos();
    Vector3 GetWorldPos();
    Vector3 GetForward();
    bool HasArrived(Vector3 destination, float threshold = 2.0f);
    bool HasFled(Vector3 destination, float threshold = 3.0f);


    // Movement
    bool TryMoveTo(Vector3 destination, AgentMoveType moveType);
    void RotateTo(Quaternion quaternion);

    //void Dodge(Vector3 direction, float speed, DodgeType type);
    bool TryStrafe(Vector3 centerPos, float radius, float angularSpeed, int direction, out int usedDirection);

    void ResetCooldown(string key, Blackboard blackboard, float duration);

    void ResetMoveCommand();

}
