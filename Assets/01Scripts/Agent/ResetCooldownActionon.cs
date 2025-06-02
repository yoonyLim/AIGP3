using System.Collections;
using UnityEngine;

public class StartCooldownAction : ActionNode
{
    private readonly Blackboard blackboard;
    private readonly string key;
    private readonly float duration;
    private readonly MonoBehaviour runner;

    private bool isStarted = false;

    public StartCooldownAction(MonoBehaviour runner, Blackboard blackboard, string key, float duration) : base(null)
    {
        this.runner = runner;
        this.blackboard = blackboard;
        this.key = key;
        this.duration = duration;
    }

    public override INode.STATE Evaluate()
    {
        if (!isStarted)
        {
            isStarted = true;
            blackboard.Set(key, false);
            runner.StartCoroutine(ResetCooldown());
            return INode.STATE.SUCCESS;
        }

        return INode.STATE.SUCCESS; // Ȥ�� �ʿ��ϴٸ� RUN�� ����
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(duration);
        blackboard.Set(key, true);
        isStarted = false; // ���� ������ ���·� �ʱ�ȭ
    }
}
