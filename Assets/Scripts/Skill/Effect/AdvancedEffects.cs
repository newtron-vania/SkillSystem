using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스탯 배수 적용 효과 클래스입니다.
/// </summary>
public class MultiplyStatEffect : Effect
{
    private Stat _targetStat;
    private List<float> _multipliers;
    private float _duration;
    private float _currentTime;
    private List<float> _originalValues;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 3, nameof(MultiplyStatEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _targetStat = SafeCast<Stat>(objects[0], "targetStat");
            _multipliers = SafeCast<List<float>>(objects[1], "multipliers");
            _duration = SafeCast<float>(objects[2], "duration");
            _currentTime = 0f;
            _originalValues = new List<float>();
            return Success($"MultiplyStatEffect initialized for {_duration}s");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize MultiplyStatEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (_currentTime == 0f) // 첫 적용 시 스탯 저장 및 적용
        {
            ApplyMultipliers();
        }

        _currentTime += Time.deltaTime;

        if (_currentTime >= _duration)
        {
            RestoreOriginalStats();
            return true;
        }

        return false; // 지속 시간 동안 대기
    }

    private void ApplyMultipliers()
    {
        _originalValues.Clear();
        _originalValues.Add(_targetStat.health);
        _originalValues.Add(_targetStat.mana);
        _originalValues.Add(_targetStat.ad);
        _originalValues.Add(_targetStat.defense);
        _originalValues.Add(_targetStat.movespeed);
        _originalValues.Add(_targetStat.attackSpeed);

        if (_multipliers.Count > 0 && _multipliers[0] != 1f) _targetStat.health *= _multipliers[0];
        if (_multipliers.Count > 1 && _multipliers[1] != 1f) _targetStat.mana *= _multipliers[1];
        if (_multipliers.Count > 2 && _multipliers[2] != 1f) _targetStat.ad *= _multipliers[2];
        if (_multipliers.Count > 3 && _multipliers[3] != 1f) _targetStat.defense *= _multipliers[3];
        if (_multipliers.Count > 4 && _multipliers[4] != 1f) _targetStat.movespeed *= _multipliers[4];
        if (_multipliers.Count > 5 && _multipliers[5] != 1f) _targetStat.attackSpeed *= _multipliers[5];

        Debug.Log($"Applied stat multipliers for {_duration}s");
    }

    private void RestoreOriginalStats()
    {
        if (_originalValues.Count >= 6)
        {
            _targetStat.health = _originalValues[0];
            _targetStat.mana = _originalValues[1];
            _targetStat.ad = _originalValues[2];
            _targetStat.defense = _originalValues[3];
            _targetStat.movespeed = _originalValues[4];
            _targetStat.attackSpeed = _originalValues[5];
            Debug.Log("Restored original stat values");
        }
    }

    protected override void DoClear()
    {
        if (_originalValues != null && _originalValues.Count >= 6)
        {
            RestoreOriginalStats();
        }
        _targetStat = null;
        _multipliers?.Clear();
        _originalValues?.Clear();
        _duration = 0f;
        _currentTime = 0f;
    }
}

/// <summary>
/// 투사체 발사 효과 클래스입니다.
/// </summary>
public class ShootProjectileEffect : Effect
{
    private float _speed;
    private string _projectilePrefabPath;
    private float _range;
    private Action<Actor, Actor> _onHitAction;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 4, nameof(ShootProjectileEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _speed = SafeCast<float>(objects[0], "speed");
            _projectilePrefabPath = SafeCast<string>(objects[1], "projectilePrefabPath");
            _range = SafeCast<float>(objects[2], "range");
            _onHitAction = SafeCast<Action<Actor, Actor>>(objects[3], "onHitAction");
            return Success($"ShootProjectileEffect initialized: {_projectilePrefabPath}, speed: {_speed}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize ShootProjectileEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        try
        {
            var direction = (source.targetPosition - source.transform.position).normalized;
            var projectileGO = ResourceManager.Instance.Instantiate(_projectilePrefabPath);

            if (projectileGO == null)
            {
                Debug.LogError($"Failed to instantiate projectile: {_projectilePrefabPath}");
                return false;
            }

            projectileGO.transform.position = source.transform.position + Vector3.up;

            var projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Launch(direction, _speed, _range, source);
                if (_onHitAction != null)
                {
                    projectile.AddEvent(_onHitAction);
                }
            }

            Debug.Log($"Launched projectile towards {source.targetPosition}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error launching projectile: {ex.Message}");
            return false;
        }
    }

    protected override void DoClear()
    {
        _speed = 0f;
        _projectilePrefabPath = null;
        _range = 0f;
        _onHitAction = null;
    }
}

/// <summary>
/// 그랩(끌어오기) 효과 클래스입니다.
/// </summary>
public class GrabEffect : Effect
{
    private float _grabSpeed = 10f;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        if (objects.Length > 0)
        {
            try
            {
                _grabSpeed = SafeCast<float>(objects[0], "grabSpeed");
            }
            catch (Exception)
            {
                _grabSpeed = 10f; // 기본값 사용
            }
        }
        return Success($"GrabEffect initialized with speed: {_grabSpeed}");
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (source?.target == null)
        {
            Debug.LogWarning("No target to grab");
            return true; // 타겟이 없으면 완료로 처리
        }

        var targetActor = source.target;
        var direction = (source.transform.position - targetActor.transform.position).normalized;

        // 타겟을 source 쪽으로 이동
        targetActor.transform.position = Vector3.MoveTowards(
            targetActor.transform.position,
            source.transform.position,
            _grabSpeed * Time.deltaTime
        );

        // 충분히 가까워지면 완료
        float distance = Vector3.Distance(source.transform.position, targetActor.transform.position);
        if (distance <= 2f)
        {
            Debug.Log($"Grab completed for {targetActor.championName}");
            return true;
        }

        return false; // 계속 진행 중
    }

    protected override void DoClear()
    {
        _grabSpeed = 10f;
    }
}

/// <summary>
/// 사거리 표시 효과 클래스 (NoneTarget용)
/// </summary>
public class NoneTargetRangeViewEffect : Effect
{
    private float _range;
    private GameObject _rangeIndicator;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(NoneTargetRangeViewEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _range = SafeCast<float>(objects[0], "range");
            return Success($"NoneTargetRangeViewEffect initialized with range: {_range}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize NoneTargetRangeViewEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (_rangeIndicator == null)
        {
            try
            {
                _rangeIndicator = ResourceManager.Instance.Instantiate("RangeCircle");
                if (_rangeIndicator != null)
                {
                    _rangeIndicator.transform.position = source.transform.position;
                    _rangeIndicator.transform.localScale = Vector3.one * _range;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not create range indicator: {ex.Message}");
            }
        }

        return true; // 시각화는 즉시 완료
    }

    protected override void DoClear()
    {
        if (_rangeIndicator != null)
        {
            ResourceManager.Instance.Destroy(_rangeIndicator);
            _rangeIndicator = null;
        }
        _range = 0f;
    }
}

/// <summary>
/// 사거리 표시 효과 클래스 (Target용)
/// </summary>
public class TargetRangeViewEffect : Effect
{
    private float _range;
    private GameObject _rangeIndicator;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(TargetRangeViewEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _range = SafeCast<float>(objects[0], "range");
            return Success($"TargetRangeViewEffect initialized with range: {_range}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize TargetRangeViewEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (_rangeIndicator == null)
        {
            try
            {
                _rangeIndicator = ResourceManager.Instance.Instantiate("RangeCircle");
                if (_rangeIndicator != null)
                {
                    _rangeIndicator.transform.position = source.transform.position;
                    _rangeIndicator.transform.localScale = Vector3.one * _range;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not create range indicator: {ex.Message}");
            }
        }

        return true; // 시각화는 즉시 완료
    }

    protected override void DoClear()
    {
        if (_rangeIndicator != null)
        {
            ResourceManager.Instance.Destroy(_rangeIndicator);
            _rangeIndicator = null;
        }
        _range = 0f;
    }
}

/// <summary>
/// 스킬 시각화 효과 클래스입니다.
/// </summary>
public class SkillViewEffect : Effect
{
    private string _effectPrefabPath;
    private GameObject _effectObject;
    private float _duration;
    private float _currentTime;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 2, nameof(SkillViewEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _effectPrefabPath = SafeCast<string>(objects[0], "effectPrefabPath");
            _duration = SafeCast<float>(objects[1], "duration");
            _currentTime = 0f;
            return Success($"SkillViewEffect initialized: {_effectPrefabPath} for {_duration}s");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize SkillViewEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (_effectObject == null)
        {
            try
            {
                _effectObject = ResourceManager.Instance.Instantiate(_effectPrefabPath);
                if (_effectObject != null && source != null)
                {
                    _effectObject.transform.position = source.transform.position;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not create skill effect: {ex.Message}");
                return true; // 시각 효과 실패해도 스킬은 계속
            }
        }

        _currentTime += Time.deltaTime;
        return _currentTime >= _duration;
    }

    protected override void DoClear()
    {
        if (_effectObject != null)
        {
            ResourceManager.Instance.Destroy(_effectObject);
            _effectObject = null;
        }
        _effectPrefabPath = null;
        _duration = 0f;
        _currentTime = 0f;
    }
}