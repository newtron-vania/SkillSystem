

using System;
using System.Collections.Generic;
using UnityEngine;

public class RocketGrabSkill : NoneTargetSkill
{
    private ToBoolCheck isCatch = new ToBoolCheck();
    public RocketGrabSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new ConsumeManaEffect(Cost));
        AddEffect(new CooldownEffect(Cooltime, SkillIndex));
        AddEffect(new ShootProjectileEffect(Values[3],  "Projectile/DefaultProjectile", Values[2], (Action<Actor, Actor>)((source, target) =>
        {
            var damageEffect = new DamageEffect(Values[0], Values[1], 0);
            damageEffect.Apply(source, target);  // 즉시 데미지 적용
            GetEffect(5).Initialize(Values[3] * 2);
            isCatch._ischeck = true;
            source.target = target;
        })));
        AddEffect(new WaitForCheckEffect(isCatch));
        AddEffect(new GrabEffect());
    }

    public override void Initialize()
    {
        isCatch._ischeck = false;
        GetEffect(1).Initialize(Cost);                       // ConsumeManaEffect
        GetEffect(2).Initialize(Cooltime, SkillIndex);       // CooldownEffect
        GetEffect(3).Initialize(
            Values[3],                                       // projectile speed
            "Projectile/DefaultProjectile",                 // prefab path
            Values[2],                                       // range
            (Action<Actor, Actor>)((source, target) =>
            {
                var damageEffect = new DamageEffect(Values[0], Values[1], 0);
                damageEffect.Apply(source, target);  // 즉시 데미지 적용
                GetEffect(5).Initialize(Values[3] * 2);
                isCatch._ischeck = true;
                source.target = target;
            })
        );                                                   // ShootProjectileEffect
        GetEffect(4).Initialize(isCatch);            // WaitForTimeEffect
    }

    public override bool ApplySkill(Actor source)
    {
        if (!base.ApplySkill(source))
            return false;
        
        if (_currentEffectIndex >= GetEffectCount())
        {
            TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
            _currentEffectIndex = 1; // 다시 초기화 가능하도록
            isCatch._ischeck = false;
            return true;
        }

        var effect = GetEffect(_currentEffectIndex);
        bool success = effect.Apply(source, source.target ?? null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++; // 다음 Effect로
        }

        return false; // 아직 스킬 진행 중
    }
}

public class OverDriveSkill : InstantSkill
{
    public OverDriveSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new WaitForTimeEffect(0.15f));
        AddEffect(new ConsumeManaEffect(Cost));
        AddEffect(new CooldownEffect(Cooltime, SkillIndex));
        AddEffect(new MultiplyStatEffect());
        AddEffect(new MultiplyStatEffect());
    }
    
    public override void Initialize()
    {
        List<float> multiplyValues = new List<float>()
        {
            0,
            0,
            0,
            0,
            0,
            0,
            Values[0]
        };
        GetEffect(1).Initialize(Cost);                       // ConsumeManaEffect
        GetEffect(2).Initialize(Cooltime, SkillIndex);       // CooldownEffect
        GetEffect(3).Initialize(GameManager.Instance.Player, multiplyValues, Values[1]);

        multiplyValues = new List<float>()
        {
            0,
            0,
            0,
            0,
            0,
            Values[2]
        };
        GetEffect(4).Initialize(GameManager.Instance.Player, multiplyValues, Values[3]);
    }

    public override bool ApplySkill(Actor source)
    {
        if (!base.ApplySkill(source))
            return false;
        
        if (_currentEffectIndex >= GetEffectCount())
        {
            _currentEffectIndex = 1; // 다시 초기화 가능하도록
            return true;
        }

        var effect = GetEffect(_currentEffectIndex);
        bool success = effect.Apply(source, source.target ?? null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++; // 다음 Effect로
        }
        return false;
    }
}


public class PowerFistSkill : TargetSkill
{
    public PowerFistSkill(SkillData skillData) : base(skillData)
    {
    }

    public override bool ApplySkill(Actor source)
    {
        bool result = base.ApplySkill(source);
        if (result)
        {
            
        }

        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
        return false;
    }
}