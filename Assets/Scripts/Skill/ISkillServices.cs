using System;
using UnityEngine;

/// <summary>
/// 입력 관리 서비스 인터페이스입니다.
/// 스킬이 InputManager에 직접 의존하지 않도록 추상화합니다.
/// </summary>
public interface IInputService
{
    void AddMouseBinding(int button, Action action);
    void RemoveMouseBinding(int button, Action action);
    Ray GetMouseRay();
    Vector3 GetMousePosition();
}

/// <summary>
/// 트리거 관리 서비스 인터페이스입니다.
/// 스킬이 TriggerManager에 직접 의존하지 않도록 추상화합니다.
/// </summary>
public interface ITriggerService
{
    bool IsTriggerActive(Define.GameTrigger trigger);
    void ActivateTrigger(Define.GameTrigger trigger);
    void DeactivateTrigger(Define.GameTrigger trigger);
}

/// <summary>
/// 타겟팅 서비스 인터페이스입니다.
/// 레이캐스팅과 타겟 검증 로직을 추상화합니다.
/// </summary>
public interface ITargetingService
{
    SkillResult TryGetGroundPosition(Ray ray, out Vector3 position);
    SkillResult TryGetActorTarget(Ray ray, Vector3 sourcePosition, float maxRange, out Actor target);
    bool IsInRange(Vector3 source, Vector3 target, float range);
}

/// <summary>
/// 스킬 매니저 서비스 인터페이스입니다.
/// </summary>
public interface ISkillManagerService
{
    void RegisterSkill(Skill skill, Actor actor);
}

/// <summary>
/// Unity의 InputManager를 래핑하는 서비스 구현체입니다.
/// </summary>
public class UnityInputService : IInputService
{
    public void AddMouseBinding(int button, Action action)
    {
        InputManager.Instance?.AddMouseBinding(button, action);
    }

    public void RemoveMouseBinding(int button, Action action)
    {
        InputManager.Instance?.RemoveMouseBinding(button, action);
    }

    public Ray GetMouseRay()
    {
        return Camera.main != null ? Camera.main.ScreenPointToRay(Input.mousePosition) : default;
    }

    public Vector3 GetMousePosition()
    {
        return Input.mousePosition;
    }
}

/// <summary>
/// Unity의 TriggerManager를 래핑하는 서비스 구현체입니다.
/// </summary>
public class UnityTriggerService : ITriggerService
{
    public bool IsTriggerActive(Define.GameTrigger trigger)
    {
        return TriggerManager.Instance?.IsTriggerActive(trigger) ?? false;
    }

    public void ActivateTrigger(Define.GameTrigger trigger)
    {
        TriggerManager.Instance?.ActivateTrigger(trigger);
    }

    public void DeactivateTrigger(Define.GameTrigger trigger)
    {
        TriggerManager.Instance?.DeactivateTrigger(trigger);
    }
}

/// <summary>
/// Unity의 물리 시스템을 사용하는 타겟팅 서비스 구현체입니다.
/// </summary>
public class UnityTargetingService : ITargetingService
{
    private readonly LayerMask _groundLayer;
    private readonly LayerMask _actorLayer;

    public UnityTargetingService()
    {
        _groundLayer = LayerMask.GetMask(SkillConstants.GROUND_LAYER);
        _actorLayer = LayerMask.GetMask(SkillConstants.ACTOR_LAYER);
    }

    public SkillResult TryGetGroundPosition(Ray ray, out Vector3 position)
    {
        position = Vector3.zero;

        try
        {
            if (Physics.Raycast(ray, out RaycastHit hit, SkillConstants.DEFAULT_RAYCAST_DISTANCE, _groundLayer))
            {
                position = new Vector3(hit.point.x, 0, hit.point.z);
                return SkillResult.Success("Ground position found");
            }

            return SkillResult.Failure("No ground found under cursor");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("Error during ground raycast", ex);
        }
    }

    public SkillResult TryGetActorTarget(Ray ray, Vector3 sourcePosition, float maxRange, out Actor target)
    {
        target = null;

        try
        {
            if (Physics.Raycast(ray, out RaycastHit hit, SkillConstants.DEFAULT_RAYCAST_DISTANCE, _actorLayer))
            {
                if (!IsInRange(sourcePosition, hit.transform.position, maxRange))
                {
                    return SkillResult.Failure("Target is out of range");
                }

                target = hit.transform.GetComponent<Actor>();
                if (target == null)
                {
                    return SkillResult.Failure("Hit object is not an Actor");
                }

                return SkillResult.Success("Target found");
            }

            return SkillResult.Failure("No actor found under cursor");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("Error during actor raycast", ex);
        }
    }

    public bool IsInRange(Vector3 source, Vector3 target, float range)
    {
        return Vector3.Distance(source, target) <= range;
    }
}

/// <summary>
/// SkillManager를 래핑하는 서비스 구현체입니다.
/// </summary>
public class UnitySkillManagerService : ISkillManagerService
{
    public void RegisterSkill(Skill skill, Actor actor)
    {
        actor?.SkillManager?.RegisterSkill(skill, actor);
    }
}