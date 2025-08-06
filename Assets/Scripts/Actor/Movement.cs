using UnityEngine;

[RequireComponent(typeof(Actor))]
public abstract class Movement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    private Actor _actor; // Actor 참조

    private void Start()
    {
        // Actor 컴포넌트를 가져옵니다.
        _actor = GetComponent<Actor>();
    }

    private void Update()
    {
        // Stunned 상태라면 이동을 막는다.
        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated) || _actor.isStunned || _actor.isAirborne) return;
        {
            return; // 이동 불가능
        }

        Move();
    }

    protected abstract void Move();
}