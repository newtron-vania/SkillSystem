using System;
using UnityEngine;

/// <summary>
/// ShootProjectileEffect는 지정된 투사체를 생성하여 목표 방향으로 발사하는 효과 클래스입니다.
/// 투사체의 속도, 최대 거리, 프리팹 경로, 충돌 시 실행할 액션을 설정할 수 있습니다.
/// 생성된 투사체는 Projectile 컴포넌트를 통해 이동 및 충돌 처리를 수행합니다.
/// </summary>
public class ShootProjectileEffect : Effect
{
    private float _speed;
    private string _projectilePath;
    private float _maxDistance;
    private Action<Actor, Actor> _projectileAction;

    public ShootProjectileEffect(float speed = 0f, string projectilePath = "", float maxDistance = 0f,
        Action<Actor, Actor> projectileAction = null) : base()
    {
        _speed = speed;
        _projectilePath = projectilePath;
        _maxDistance = maxDistance;
        _projectileAction = projectileAction;
        IsInitialized = speed > 0 && !string.IsNullOrEmpty(projectilePath) && maxDistance > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length < 4)
                return Failure("ShootProjectileEffect.Initialize: Expected 4 parameters (float, string, float, Action<Actor, Actor>)");

            if (objects[0] is float speed &&
                objects[1] is string projectilePath &&
                objects[2] is float maxDistance &&
                objects[3] is Action<Actor, Actor> action)
            {
                _speed = speed;
                _projectilePath = projectilePath;
                _maxDistance = maxDistance;
                _projectileAction = action;
                IsInitialized = true;
                return Success($"ShootProjectileEffect initialized with speed: {_speed}, path: {_projectilePath}");
            }

            return Failure("ShootProjectileEffect.Initialize: Invalid parameter types");
        }
        catch (Exception ex)
        {
            return Failure("ShootProjectileEffect.Initialize failed", ex);
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("ShootProjectileEffect: Not initialized before Apply");
            return false;
        }

        if (source == null)
        {
            Debug.LogError("ShootProjectileEffect: Source is null");
            return false;
        }

        try
        {
            Vector3 spawnPosition = source.transform.position + source.transform.forward * 1f + Vector3.up * 1.5f;
            GameObject projectile = ResourceManager.Instance.Instantiate(_projectilePath, spawnPosition, Quaternion.identity);
            
            if (projectile == null)
            {
                Debug.LogError($"ShootProjectileEffect: Failed to instantiate projectile at path: {_projectilePath}");
                return false;
            }

            Vector3 direction = (source.targetPosition - source.transform.position).normalized;
            Debug.Log($"TargetPosition: {source.targetPosition}");
            Debug.Log($"Projectile Direction: {direction}");
            
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent == null)
            {
                Debug.LogError("ShootProjectileEffect: Projectile component not found");
                GameObject.Destroy(projectile);
                return false;
            }
            
            projectileComponent.AddEvent(_projectileAction);
            projectileComponent.Launch(direction, _speed, _maxDistance, source);
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"ShootProjectileEffect.Apply failed: {ex.Message}");
            return false;
        }
    }

    public override void Clear()
    {
        // 투사체는 자체적으로 생명주기를 관리하므로 Clear에서 특별한 작업 없음
    }
}