using UnityEngine;
using Unity.MLAgents;

public interface IAgent
{
    BaseAgent GetAgent();
    AgentType GetAgentType();

    Quaternion GetLocalRot();
    Vector3 GetLocalPos();
    Vector3 GetWorldPos();
    Vector3 GetForward();

    void MoveTo(Vector3 destination, AgentMoveType moveType);
    void RotateTo(Quaternion quaternion);

    bool HasArrived(Vector3 destination, float threshold = 2.0f);
    bool HasFled(Vector3 destination, float threshold = 3.0f);
    //void Dodge(Vector3 direction, float speed, DodgeType type);
    void Strafe(Vector3 centerPos, float radius = 3f, float angularSpeed = 90f, int direction = 1);

    void ResetCooldown(string key, Blackboard blackboard, float duration);

    void ResetMoveCommand();

    bool WillHitObstacle(Vector3 destination, float distance, LayerMask wallMask);
}
