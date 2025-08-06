using UnityEngine;

/// <summary>
/// Movement는 Actor의 이동 로직을 정의하기 위한 추상 클래스입니다.
/// Actor를 기반으로 하며, 이동 방식은 파생 클래스에서 구현해야 합니다.
/// 상태 이상(Stun, Airborne, 특정 트리거)일 경우 이동이 제한됩니다.
/// </summary>
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

    private void FixedUpdate()
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