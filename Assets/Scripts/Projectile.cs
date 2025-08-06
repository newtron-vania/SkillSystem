using System;
using UnityEngine;

/// <summary>
/// Projectile 클래스는 게임 내 투사체(예: 스킬 발사체)의 이동, 충돌 처리, 
/// 최대 사거리 체크, 명중 시 실행할 행동(Action)을 관리하는 컴포넌트입니다.
/// </summary>
public class Projectile : MonoBehaviour
{
    private Vector3 _direction;                      // 이동 방향
    private float _maxDistance;                      // 최대 이동 거리
    private float _currentDistance;                  // 현재까지 이동한 거리
    private Action<Actor, Actor> _projectileAction;  // 명중 시 실행할 액션 (시전자, 피격자)
    private Actor _shooter;                          // 투사체를 발사한 Actor
    private float _speed;                            // 이동 속도

    /// <summary>
    /// 매 프레임마다 투사체를 이동시키고, 최대 거리 초과 시 파괴합니다.
    /// </summary>
    protected virtual void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        _currentDistance += _speed * Time.deltaTime;

        if (_currentDistance >= _maxDistance)
        {
            ResourceManager.Instance.Destroy(gameObject); // 사거리 초과 시 제거
        }
    }

    
    /// <summary>
    /// 충돌 감지: 적에게 맞았을 경우 액션 실행 후 투사체 파괴
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 시전자 자신이 아닌 적에게만 반응
        if (other.gameObject != _shooter.gameObject && other.gameObject.CompareTag("Enemy"))
        {
            Actor targetActor = other.GetComponentInParent<Actor>();
            Debug.Log($"Successfully hit {other.gameObject.name}");

            _projectileAction?.Invoke(_shooter, targetActor); // 명중 시 효과 실행
            ResourceManager.Instance.Destroy(gameObject);     // 투사체 제거
        }
    }

    /// <summary>
    /// 투사체를 지정된 방향, 속도, 사거리로 발사합니다.
    /// </summary>
    public void Launch(Vector3 direction, float speed, float maxDistance, Actor shooter)
    {
        _direction = direction;
        _speed = speed;
        _maxDistance = maxDistance;
        _shooter = shooter;
    }

    /// <summary>
    /// 투사체 명중 시 실행할 외부 액션을 등록합니다.
    /// 예: 데미지 부여, 상태이상 적용 등
    /// </summary>
    public void AddEvent(Action<Actor, Actor> action)
    {
        _projectileAction += action;
    }
}