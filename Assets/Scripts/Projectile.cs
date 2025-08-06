using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 _direction;
    private float _maxDistance;
    private float _currentDistance;
    private Action<Actor, Actor> _projectileAction;
    private Actor _shooter;
    private float _speed;

    protected virtual void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        _currentDistance += _speed * Time.deltaTime;
        if (_currentDistance >= _maxDistance)
        {
            ResourceManager.Instance.Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // 목표에 닿았을 때
        if (other.gameObject != _shooter.gameObject && other.gameObject.CompareTag("Enemy"))
        {
            Actor targetActor = other.GetComponentInParent<Actor>();
            Debug.Log($"Succesfully hit {other.gameObject.name}");
            _projectileAction?.Invoke(_shooter, targetActor);
            ResourceManager.Instance.Destroy(gameObject); // 투사체 삭제
        }
    }
    public void Launch(Vector3 direction, float speed, float maxDistance, Actor shooter)
    {
        _direction = direction;
        _speed = speed;
        _maxDistance = maxDistance;
        _shooter = shooter;
    }

    public void AddEvent(Action<Actor, Actor> action)
    {
        _projectileAction += action;
    }
}