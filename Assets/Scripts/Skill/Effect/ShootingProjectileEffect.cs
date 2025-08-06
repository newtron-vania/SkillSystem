using System;
using UnityEngine;

/// <summary>
/// ShootProjectileEffect는 지정된 투사체를 생성하여 목표 방향으로 발사하는 효과 클래스입니다.
/// 투사체의 속도, 최대 거리, 프리팹 경로, 충돌 시 실행할 액션을 설정할 수 있습니다.
/// 생성된 투사체는 Projectile 컴포넌트를 통해 이동 및 충돌 처리를 수행합니다.
/// </summary>
public class ShootProjectileEffect : Effect
{
    private float _speed;                     // 투사체 속도
    private string _projectilePath;           // 투사체 프리팹 리소스 경로
    private float _maxDistance;               // 투사체 최대 이동 거리
    Action<Actor, Actor> _projectileAction;   // 충돌 시 실행할 액션 (시전자, 피격자)

    public ShootProjectileEffect(float speed, string projectilePath, float maxDistance,
        Action<Actor, Actor> projectileAction) : base()
    {
        _speed = speed;
        _projectilePath = projectilePath;
        _maxDistance = maxDistance;
        _projectileAction = projectileAction;
    }

    public override void Initialize(params object[] objects)
    {
        if(objects[0] is float speed) _speed = speed;
        if(objects[1] is string projectilePath) _projectilePath = projectilePath;
        if(objects[2] is float maxDistance) _maxDistance = maxDistance;
        if(objects[3] is Action<Actor, Actor> action1) _projectileAction = action1;
    }

    public override bool Apply(Actor source, Actor target)
    {
        Vector3 spawnPosition = source.transform.position + source.transform.forward * 1f + Vector3.up * 1.5f;
        // 목표 지점으로 발사
        GameObject projectile = ResourceManager.Instance.Instantiate(_projectilePath, spawnPosition, Quaternion.identity);
        Vector3 direction = (source.targetPosition - source.transform.position).normalized;
        Debug.Log($"TargetPosition: {source.targetPosition}");
        Debug.Log($"Projectile Direction: {direction}");
        Projectile projectilecomponent = projectile.GetComponent<Projectile>();
        
        projectilecomponent.AddEvent(_projectileAction);
        projectilecomponent.Launch(direction, _speed, _maxDistance, source);
        
        return true;
    }

    public override void Clear()
    {
        throw new NotImplementedException();
    }
}