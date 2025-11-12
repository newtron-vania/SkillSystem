using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마나 소모 효과 클래스입니다.
/// </summary>
public class ConsumeManaEffect : Effect
{
    private int _manaCost;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(ConsumeManaEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _manaCost = SafeCast<int>(objects[0], "manaCost");
            return Success($"ConsumeManaEffect initialized with cost: {_manaCost}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize ConsumeManaEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (source?.stat == null)
        {
            Debug.LogError("Source actor or stat is null");
            return false;
        }

        if (source.stat.mana < _manaCost)
        {
            Debug.LogWarning($"Not enough mana. Required: {_manaCost}, Available: {source.stat.mana}");
            return false;
        }

        source.stat.mana -= _manaCost;
        Debug.Log($"Consumed {_manaCost} mana. Remaining: {source.stat.mana}");
        return true;
    }

    protected override void DoClear()
    {
        _manaCost = 0;
    }
}

/// <summary>
/// 스킬 쿨다운 적용 효과 클래스입니다.
/// </summary>
public class CooldownEffect : Effect
{
    private float _cooldownTime;
    private int _skillIndex;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 2, nameof(CooldownEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _cooldownTime = SafeCast<float>(objects[0], "cooldownTime");
            _skillIndex = SafeCast<int>(objects[1], "skillIndex");
            return Success($"CooldownEffect initialized: {_cooldownTime}s for skill {_skillIndex}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize CooldownEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (source?.skillCooldowns == null)
        {
            Debug.LogError("Source actor or skillCooldowns is null");
            return false;
        }

        source.skillCooldowns[_skillIndex] = _cooldownTime;
        Debug.Log($"Applied cooldown: {_cooldownTime}s to skill {_skillIndex}");
        return true;
    }

    protected override void DoClear()
    {
        _cooldownTime = 0f;
        _skillIndex = 0;
    }
}

/// <summary>
/// 데미지 효과 클래스입니다.
/// </summary>
public class DamageEffect : Effect
{
    private float _baseDamage;
    private float _damageMultiplier;
    private float _bonusDamage;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 3, nameof(DamageEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _baseDamage = SafeCast<float>(objects[0], "baseDamage");
            _damageMultiplier = SafeCast<float>(objects[1], "damageMultiplier");
            _bonusDamage = SafeCast<float>(objects[2], "bonusDamage");
            return Success($"DamageEffect initialized: {_baseDamage} + {_damageMultiplier}x + {_bonusDamage}");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize DamageEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (source?.stat == null || target?.stat == null)
        {
            Debug.LogError("Source or target actor/stat is null");
            return false;
        }

        float totalDamage = _baseDamage + (source.stat.attackDamage * _damageMultiplier) + _bonusDamage;
        target.stat.hp -= totalDamage;

        Debug.Log($"Applied {totalDamage} damage to {target.championName}. HP: {target.stat.hp}");
        return true;
    }

    protected override void DoClear()
    {
        _baseDamage = 0f;
        _damageMultiplier = 0f;
        _bonusDamage = 0f;
    }
}

/// <summary>
/// 트리거 활성화 효과 클래스입니다.
/// </summary>
public class ActivateTriggerEffect : Effect
{
    private List<Define.GameTrigger> _triggers;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(ActivateTriggerEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _triggers = SafeCast<List<Define.GameTrigger>>(objects[0], "triggers");
            return Success($"ActivateTriggerEffect initialized with {_triggers.Count} triggers");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize ActivateTriggerEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (TriggerManager.Instance == null)
        {
            Debug.LogError("TriggerManager instance is null");
            return false;
        }

        foreach (var trigger in _triggers)
        {
            TriggerManager.Instance.ActivateTrigger(trigger);
            Debug.Log($"Activated trigger: {trigger}");
        }
        return true;
    }

    protected override void DoClear()
    {
        _triggers?.Clear();
    }
}

/// <summary>
/// 트리거 비활성화 효과 클래스입니다.
/// </summary>
public class DeActivateTriggerEffect : Effect
{
    private List<Define.GameTrigger> _triggers;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(DeActivateTriggerEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _triggers = SafeCast<List<Define.GameTrigger>>(objects[0], "triggers");
            return Success($"DeActivateTriggerEffect initialized with {_triggers.Count} triggers");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize DeActivateTriggerEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (TriggerManager.Instance == null)
        {
            Debug.LogError("TriggerManager instance is null");
            return false;
        }

        foreach (var trigger in _triggers)
        {
            TriggerManager.Instance.DeactivateTrigger(trigger);
            Debug.Log($"Deactivated trigger: {trigger}");
        }
        return true;
    }

    protected override void DoClear()
    {
        _triggers?.Clear();
    }
}

/// <summary>
/// 시간 대기 효과 클래스입니다.
/// </summary>
public class WaitForTimeEffect : Effect
{
    private float _waitTime;
    private float _currentTime;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(WaitForTimeEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _waitTime = SafeCast<float>(objects[0], "waitTime");
            _currentTime = 0f;
            return Success($"WaitForTimeEffect initialized: {_waitTime}s");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize WaitForTimeEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        _currentTime += Time.deltaTime;
        return _currentTime >= _waitTime;
    }

    protected override void DoClear()
    {
        _waitTime = 0f;
        _currentTime = 0f;
    }
}

/// <summary>
/// 조건 대기 효과 클래스입니다.
/// </summary>
public class WaitForCheckEffect : Effect
{
    private ToBoolCheck _checker;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(WaitForCheckEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _checker = SafeCast<ToBoolCheck>(objects[0], "checker");
            return Success("WaitForCheckEffect initialized");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize WaitForCheckEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        return _checker?._ischeck ?? false;
    }

    protected override void DoClear()
    {
        _checker = null;
    }
}