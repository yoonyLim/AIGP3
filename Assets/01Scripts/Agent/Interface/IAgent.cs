using UnityEngine;

public interface IAgent
{
    BaseAgent GetAgent();
    AgentType GetAgentType();

    Quaternion GetLocalRot();
    Vector3 GetLocalPos();
    Vector3 GetWorldPos();
    Vector3 GetForward();

    bool MoveTo(Vector3 destination, AgentMoveType moveType);
    void RotateTo(Quaternion quaternion);

    bool HasArrived(Vector3 destination, float threshold = 2.0f);
    bool HasFled(Vector3 destination, float threshold = 3.0f);
    //void Dodge(Vector3 direction, float speed, DodgeType type);
    bool TryStrafe(Vector3 centerPos, float radius, float angularSpeed, int direction, out int usedDirection);

    void ResetCooldown(string key, Blackboard blackboard, float duration);

    void ResetMoveCommand();

}
