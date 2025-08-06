using System;
using UnityEngine;

public class GrabProjectile : Projectile
{
    private Vector3 _direction;
    private float _maxDistance;
    private float _currentDistance;
    private Action<Actor, Actor> _projectileAction;
    private Actor _shooter;
    private float _speed;

    protected override void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        _currentDistance += _speed * Time.deltaTime;
        if (_currentDistance >= _maxDistance)
        {
            TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
            ResourceManager.Instance.Destroy(gameObject);
        }
    }
}