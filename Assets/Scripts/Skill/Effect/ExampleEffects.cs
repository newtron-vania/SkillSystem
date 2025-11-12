using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새로운 SkillResult 패턴을 보여주는 예시 Effect 구현들입니다.
/// </summary>

/// <summary>
/// 단일 파라미터를 받는 Effect 예시
/// </summary>
public class HealEffect : Effect
{
    private float _healAmount;

    public HealEffect(float healAmount = 0f)
    {
        _healAmount = healAmount;
        IsInitialized = healAmount > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        var validationResult = EffectErrorHandling.ValidateParameters<float>(objects, out float healAmount);
        if (!validationResult.IsSuccess)
            return validationResult;

        _healAmount = healAmount;
        IsInitialized = true;
        return Success($"HealEffect initialized with heal amount: {_healAmount}");
    }

    protected override SkillResult DoInitialize(params object[] objects)
    {
        throw new NotImplementedException();
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("HealEffect: Not initialized before Apply");
            return false;
        }

        if (target?.stat == null)
        {
            Debug.LogError("HealEffect: Target or target.stat is null");
            return false;
        }

        var previousHealth = target.stat.health;
        target.stat.health = Mathf.Min(target.stat.maxHealth, target.stat.health + _healAmount);
        var actualHeal = target.stat.health - previousHealth;

        Debug.Log($"HealEffect applied: {actualHeal} health restored to {target.name}");
        return true;
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        throw new NotImplementedException();
    }

    public override void Clear()
    {
        // 힐링은 되돌릴 수 없는 효과이므로 Clear에서 특별한 작업 없음
    }

    protected override void DoClear()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 복합 파라미터를 받는 Effect 예시
/// </summary>
public class ShieldEffect : Effect
{
    private float _shieldAmount;
    private float _duration;
    private float _remainingTime;
    private bool _isActive;

    public ShieldEffect(float shieldAmount = 0f, float duration = 0f)
    {
        _shieldAmount = shieldAmount;
        _duration = duration;
        _remainingTime = duration;
        IsInitialized = shieldAmount > 0 && duration > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        var validationResult = EffectErrorHandling.ValidateParameters<float, float>(objects, out float shieldAmount, out float duration);
        if (!validationResult.IsSuccess)
            return validationResult;

        if (shieldAmount <= 0)
            return Failure("Shield amount must be positive");

        if (duration <= 0)
            return Failure("Duration must be positive");

        _shieldAmount = shieldAmount;
        _duration = duration;
        _remainingTime = duration;
        IsInitialized = true;

        return Success($"ShieldEffect initialized with shield: {_shieldAmount}, duration: {_duration}");
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("ShieldEffect: Not initialized before Apply");
            return false;
        }

        if (target?.stat == null)
        {
            Debug.LogError("ShieldEffect: Target or target.stat is null");
            return false;
        }

        if (!_isActive)
        {
            // 처음 적용할 때 실드 부여
            target.stat.shield += _shieldAmount;
            _isActive = true;
            Debug.Log($"ShieldEffect activated: {_shieldAmount} shield applied to {target.name}");
        }

        // 지속 시간 감소
        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0)
        {
            // 실드 제거
            target.stat.shield = Mathf.Max(0, target.stat.shield - _shieldAmount);
            Debug.Log($"ShieldEffect expired: shield removed from {target.name}");
            return true; // 효과 완료
        }

        return false; // 계속 지속
    }

    public override void Clear()
    {
        _isActive = false;
        _remainingTime = _duration;
    }
}

/// <summary>
/// 조건부 실행을 하는 Effect 예시
/// </summary>
public class ConditionalDamageEffect : Effect
{
    private float _baseDamage;
    private float _bonusDamage;
    private Func<Actor, Actor, bool> _condition;

    public ConditionalDamageEffect(float baseDamage = 0f, float bonusDamage = 0f, Func<Actor, Actor, bool> condition = null)
    {
        _baseDamage = baseDamage;
        _bonusDamage = bonusDamage;
        _condition = condition;
        IsInitialized = baseDamage >= 0 && bonusDamage >= 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        var validationResult = EffectErrorHandling.ValidateParameters<float, float, Func<Actor, Actor, bool>>(
            objects, out float baseDamage, out float bonusDamage, out Func<Actor, Actor, bool> condition);

        if (!validationResult.IsSuccess)
            return validationResult;

        _baseDamage = baseDamage;
        _bonusDamage = bonusDamage;
        _condition = condition;
        IsInitialized = true;

        return Success($"ConditionalDamageEffect initialized with base: {_baseDamage}, bonus: {_bonusDamage}");
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("ConditionalDamageEffect: Not initialized before Apply");
            return false;
        }

        if (source?.stat == null || target?.stat == null)
        {
            Debug.LogError("ConditionalDamageEffect: Source or target stat is null");
            return false;
        }

        var damage = _baseDamage;

        // 조건 확인하여 보너스 데미지 추가
        if (_condition?.Invoke(source, target) == true)
        {
            damage += _bonusDamage;
            Debug.Log("ConditionalDamageEffect: Bonus damage condition met");
        }

        var finalDamage = Mathf.Max(0, damage - target.stat.defense);
        target.stat.health = Mathf.Max(0, target.stat.health - finalDamage);

        Debug.Log($"ConditionalDamageEffect applied: {finalDamage} damage to {target.name}");
        return true;
    }

    public override void Clear()
    {
        // 데미지는 되돌릴 수 없는 효과이므로 Clear에서 특별한 작업 없음
    }
}

/// <summary>
/// 복잡한 상태 관리를 하는 Effect 예시
/// </summary>
public class StackingBuffEffect : Effect
{
    private int _stackCount;
    private int _maxStacks;
    private float _stackValue;
    private float _duration;
    private float _remainingTime;
    private List<float> _appliedBuffs = new List<float>();

    public StackingBuffEffect(int maxStacks = 1, float stackValue = 0f, float duration = 0f)
    {
        _maxStacks = maxStacks;
        _stackValue = stackValue;
        _duration = duration;
        _remainingTime = duration;
        IsInitialized = maxStacks > 0 && stackValue != 0 && duration > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        var validationResult = EffectErrorHandling.ValidateParameters<int, float, float>(
            objects, out int maxStacks, out float stackValue, out float duration);

        if (!validationResult.IsSuccess)
            return validationResult;

        if (maxStacks <= 0)
            return Failure("Max stacks must be positive");

        if (duration <= 0)
            return Failure("Duration must be positive");

        _maxStacks = maxStacks;
        _stackValue = stackValue;
        _duration = duration;
        _remainingTime = duration;
        _stackCount = 0;
        _appliedBuffs.Clear();
        IsInitialized = true;

        return Success($"StackingBuffEffect initialized with max stacks: {_maxStacks}, stack value: {_stackValue}, duration: {_duration}");
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("StackingBuffEffect: Not initialized before Apply");
            return false;
        }

        if (target?.stat == null)
        {
            Debug.LogError("StackingBuffEffect: Target or target.stat is null");
            return false;
        }

        // 새로운 스택 추가 (최대 스택 수 제한)
        if (_stackCount < _maxStacks)
        {
            _stackCount++;
            _appliedBuffs.Add(_stackValue);
            target.stat.attackPower += _stackValue;
            _remainingTime = _duration; // 지속시간 갱신

            Debug.Log($"StackingBuffEffect: Stack added. Current stacks: {_stackCount}/{_maxStacks}");
        }
        else
        {
            // 최대 스택 도달 시 지속시간만 갱신
            _remainingTime = _duration;
        }

        // 지속 시간 감소
        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0)
        {
            // 모든 스택 제거
            foreach (var buffValue in _appliedBuffs)
            {
                target.stat.attackPower -= buffValue;
            }
            _appliedBuffs.Clear();
            _stackCount = 0;

            Debug.Log("StackingBuffEffect expired: All stacks removed");
            return true; // 효과 완료
        }

        return false; // 계속 지속
    }

    public override void Clear()
    {
        _stackCount = 0;
        _appliedBuffs.Clear();
        _remainingTime = _duration;
    }
}