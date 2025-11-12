using System.Collections;
using UnityEngine;

/// <summary>
/// GrabEffect는 대상 Actor를 시전자(source) 방향으로 일정 속도로 끌어당기는 효과를 수행하는 클래스입니다.
/// 일정 거리 이내에 도달하면 이동을 멈추며, 잘못된 이동 조건이 감지되면 강제로 위치를 조정해 종료합니다.
/// </summary>
public class GrabEffect : Effect
{
    private float _moveSpeed = 10f;

    public GrabEffect(float moveSpeed = 10f)
    {
        _moveSpeed = moveSpeed;
        IsInitialized = true; // GrabEffect can work with default values
    }

    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length > 0 && objects[0] is float moveSpeed)
            {
                _moveSpeed = moveSpeed;
                IsInitialized = true;
                return Success($"GrabEffect initialized with moveSpeed: {_moveSpeed}");
            }
            
            // If no parameters provided, use default speed
            IsInitialized = true;
            return Success($"GrabEffect initialized with default moveSpeed: {_moveSpeed}");
        }
        catch (Exception ex)
        {
            return Failure("GrabEffect.Initialize failed", ex);
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("GrabEffect: Not initialized before Apply");
            return false;
        }

        if (source == null || target == null)
        {
            Debug.LogError("GrabEffect: Source or target is null");
            return true; // End the effect if invalid actors
        }

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
        // GrabEffect는 위치 이동만 하므로 Clear에서 특별한 작업 없음
    }
}