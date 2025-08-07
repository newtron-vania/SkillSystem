using System.Collections;
using UnityEngine;

/// <summary>
/// GrabEffect는 대상 Actor를 시전자(source) 방향으로 일정 속도로 끌어당기는 효과를 수행하는 클래스입니다.
/// 일정 거리 이내에 도달하면 이동을 멈추며, 잘못된 이동 조건이 감지되면 강제로 위치를 조정해 종료합니다.
/// </summary>
public class GrabEffect : Effect
{
    private float _moveSpeed; // 이동 속도

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float moveSpeed) _moveSpeed = moveSpeed;
    }

    public override bool Apply(Actor source, Actor target)
    {
        Vector3 sourcePos = source.transform.position;
        Vector3 targetPos = target.transform.position;

        float currentDistance = Vector3.Distance(sourcePos, targetPos);

        // 목표 거리 도달: 멈춤
        if (currentDistance <= 1f)
        {
            return true;
        }

        Vector3 direction = (sourcePos - targetPos).normalized;
        Vector3 nextPosition = targetPos + _moveSpeed * Time.deltaTime * direction;

        float nextDistance = Vector3.Distance(sourcePos, nextPosition);

        // 만약 더 멀어졌거나, 너무 멀리 이동하는 경우 → 강제 종료
        if (nextDistance >= currentDistance || nextDistance < 0.01f)
        {
            target.transform.position = sourcePos + direction * 1f;
            return true;
        }

        // 정상 이동
        target.transform.position = nextPosition;

        return false;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}