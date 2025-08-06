using System;
using UnityEngine;

public class ShootProjectileEffect : Effect
{
    private float _speed;  // 투사체 속도
    private string _projectilePath;  // 투사체 프리팹
    private float _maxDistance;
    Action<Actor, Actor> _projectileAction;

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