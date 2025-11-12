using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     RocketGrabSkill은 NoneTargetSkill을 상속한 블리츠크랭크의 대표 스킬인 '로켓 손'을 구현한 클래스입니다.
///     이 스킬은 투사체를 발사하여 적중 시 대상을 끌어오고, 피해를 입힌 후 Grab 효과를 적용합니다.
///     구성된 Effect들은 순차적으로 실행되며, 맞추기 전까지 대기 상태를 유지합니다.
/// </summary>
public class RocketGrabSkill : NoneTargetSkill
{
    private readonly ToBoolCheck isCatch = new();

    public RocketGrabSkill(SkillData skillData,
                          IInputService inputService = null,
                          ITriggerService triggerService = null,
                          ITargetingService targetingService = null,
                          ISkillManagerService skillManagerService = null) 
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        // Effect들을 기본 생성자로 생성하고 나중에 Initialize로 설정
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new ActivateTriggerEffect());
        AddEffect(new ShootProjectileEffect());
        AddEffect(new WaitForCheckEffect());
        AddEffect(new DeActivateTriggerEffect());
        AddEffect(new GrabEffect());
    }

    public override SkillResult Initialize()
    {
        try
        {
            isCatch._ischeck = false;
            
            var results = new List<SkillResult>();

            // Effect 0: ConsumeManaEffect
            var effect0Result = GetEffect(0);
            if (effect0Result.IsSuccess)
            {
                var initResult = effect0Result.Value.Initialize(Cost);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ConsumeManaEffect: {effect0Result.Message}");
            }

            // Effect 1: CooldownEffect
            var effect1Result = GetEffect(1);
            if (effect1Result.IsSuccess)
            {
                var initResult = effect1Result.Value.Initialize(Cooltime, SkillIndex);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get CooldownEffect: {effect1Result.Message}");
            }

            // Effect 2: ActivateTriggerEffect
            var effect2Result = GetEffect(2);
            if (effect2Result.IsSuccess)
            {
                var triggers = new List<GameTrigger> { GameTrigger.SkillActivated };
                var initResult = effect2Result.Value.Initialize(triggers);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ActivateTriggerEffect: {effect2Result.Message}");
            }

            // Effect 3: ShootProjectileEffect
            var effect3Result = GetEffect(3);
            if (effect3Result.IsSuccess)
            {
                Action<Actor, Actor> projectileAction = (source, target) =>
                {
                    var damageEffect = new DamageEffect(Values[0], Values[1], 0);
                    damageEffect.Apply(source, target);
                    
                    var grabEffectResult = GetEffect(6);
                    if (grabEffectResult.IsSuccess)
                    {
                        grabEffectResult.Value.Initialize(Values[3] * 2);
                    }
                    
                    isCatch._ischeck = true;
                    source.target = target;
                };
                
                var initResult = effect3Result.Value.Initialize(Values[3], "Projectile/DefaultProjectile", Values[2], projectileAction);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ShootProjectileEffect: {effect3Result.Message}");
            }

            // Effect 4: WaitForCheckEffect
            var effect4Result = GetEffect(4);
            if (effect4Result.IsSuccess)
            {
                var initResult = effect4Result.Value.Initialize(isCatch);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get WaitForCheckEffect: {effect4Result.Message}");
            }

            // Effect 5: DeActivateTriggerEffect
            var effect5Result = GetEffect(5);
            if (effect5Result.IsSuccess)
            {
                var triggers = new List<GameTrigger> { GameTrigger.SkillActivated };
                var initResult = effect5Result.Value.Initialize(triggers);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get DeActivateTriggerEffect: {effect5Result.Message}");
            }

            // Effect 6: GrabEffect (기본값 사용)
            var effect6Result = GetEffect(6);
            if (effect6Result.IsSuccess)
            {
                var initResult = effect6Result.Value.Initialize();
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get GrabEffect: {effect6Result.Message}");
            }

            // 모든 초기화 결과 확인
            var failedResults = results.Where(r => !r.IsSuccess).ToList();
            if (failedResults.Any())
            {
                var errorMessages = string.Join(", ", failedResults.Select(r => r.Message));
                return SkillResult.Failure($"Effect initialization failed: {errorMessages}");
            }

            return base.Initialize();
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("RocketGrabSkill initialization failed", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return SkillResult.Success("RocketGrabSkill completed successfully");
        }

        var effectResult = GetEffect(_currentEffectIndex);
        if (!effectResult.IsSuccess)
        {
            return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
        }

        var success = effectResult.Value.Apply(source, source.target ?? null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++;
        }

        return SkillResult.Success("Effect applied, continuing skill execution");
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            if (!baseResult.IsSuccess)
                return baseResult;
            
            _currentEffectIndex = 1;
            isCatch._ischeck = false;
            
            return SkillResult.Success("RocketGrabSkill cleared successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("RocketGrabSkill clear failed", ex);
        }
    }
}

/// <summary>
///     OverDriveSkill은 InstantSkill을 상속한 블리츠크랭크의 자체 강화 스킬 '폭주'를 구현한 클래스입니다.
///     사용 시 일정 시간 동안 공격 속도 및 이동 속도에 영향을 주는 버프 효과를 부여합니다.
///     두 개의 MultiplyStatEffect로 각각의 스탯을 배수만큼 강화합니다.
///     현재 오류로 인해 미완성 상태입니다.
/// </summary>
public class OverDriveSkill : InstantSkill
{
    private bool _isStart;
    private readonly List<float> _multiplyValues1 = new() { 1, 1, 1, 1, 1, 1 };
    private readonly List<float> _multiplyValues2 = new() { 1, 1, 1, 1, 1, 1 };

    public OverDriveSkill(SkillData skillData,
                         IInputService inputService = null,
                         ITriggerService triggerService = null,
                         ITargetingService targetingService = null,
                         ISkillManagerService skillManagerService = null) 
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        AddEffect(new WaitForTimeEffect());
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new MultiplyStatEffect());
        AddEffect(new MultiplyStatEffect());
    }

    public override SkillResult Initialize()
    {
        try
        {
            var results = new List<SkillResult>();

            // Effect 0: WaitForTimeEffect
            var effect0Result = GetEffect(0);
            if (effect0Result.IsSuccess)
            {
                var initResult = effect0Result.Value.Initialize(0.15f);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get WaitForTimeEffect: {effect0Result.Message}");
            }

            // Effect 1: ConsumeManaEffect
            var effect1Result = GetEffect(1);
            if (effect1Result.IsSuccess)
            {
                var initResult = effect1Result.Value.Initialize(Cost);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ConsumeManaEffect: {effect1Result.Message}");
            }

            // Effect 2: CooldownEffect
            var effect2Result = GetEffect(2);
            if (effect2Result.IsSuccess)
            {
                var initResult = effect2Result.Value.Initialize(Cooltime, SkillIndex);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get CooldownEffect: {effect2Result.Message}");
            }

            // Effect 3: MultiplyStatEffect (첫 번째)
            var effect3Result = GetEffect(3);
            if (effect3Result.IsSuccess)
            {
                _multiplyValues1[5] = Values[0];
                var initResult = effect3Result.Value.Initialize(GameManager.Instance.Player.stat, _multiplyValues1, Values[1]);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get first MultiplyStatEffect: {effect3Result.Message}");
            }

            // Effect 4: MultiplyStatEffect (두 번째)
            var effect4Result = GetEffect(4);
            if (effect4Result.IsSuccess)
            {
                _multiplyValues2[4] = Values[2]; // 이동속도는 인덱스 4
                var initResult = effect4Result.Value.Initialize(GameManager.Instance.Player.stat, _multiplyValues2, Values[3]);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get second MultiplyStatEffect: {effect4Result.Message}");
            }

            // 모든 초기화 결과 확인
            var failedResults = results.Where(r => !r.IsSuccess).ToList();
            if (failedResults.Any())
            {
                var errorMessages = string.Join(", ", failedResults.Select(r => r.Message));
                return SkillResult.Failure($"Effect initialization failed: {errorMessages}");
            }

            return base.Initialize();
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("OverDriveSkill initialization failed", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        if (!_isStart)
        {
            _isStart = true;
            var initResult = Initialize();
            if (!initResult.IsSuccess)
                return initResult;
        }

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return SkillResult.Success("OverDriveSkill completed successfully");
        }

        var effectResult = GetEffect(_currentEffectIndex);
        if (!effectResult.IsSuccess)
        {
            return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
        }

        var success = effectResult.Value.Apply(source, null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++;
        }

        return SkillResult.Success("Effect applied, continuing skill execution");
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            if (!baseResult.IsSuccess)
                return baseResult;

            _currentEffectIndex = 0;
            _isStart = false;
            
            return SkillResult.Success("OverDriveSkill cleared successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("OverDriveSkill clear failed", ex);
        }
    }
}


/// <summary>
///     PowerFistSkill은 TargetSkill을 상속한 블리츠크랭크의 강화 공격 스킬 '강철 주먹'을 구현한 클래스입니다.
///     현재는 스킬 로직이 미완성 상태이며, 추후 대상에게 강화된 기본 공격 또는 에어본 등의 효과를 추가할 예정입니다.
/// </summary>
public class PowerFistSkill : TargetSkill
{
    public PowerFistSkill(SkillData skillData,
                         IInputService inputService = null,
                         ITriggerService triggerService = null,
                         ITargetingService targetingService = null,
                         ISkillManagerService skillManagerService = null) 
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        // 기본적인 스킬 효과들 추가 (추후 확장 예정)
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new ActivateTriggerEffect());
        // TODO: 추후 강화된 기본 공격이나 에어본 효과 추가
        AddEffect(new DeActivateTriggerEffect());
    }

    public override SkillResult Initialize()
    {
        try
        {
            var results = new List<SkillResult>();

            // Effect 0: ConsumeManaEffect
            var effect0Result = GetEffect(0);
            if (effect0Result.IsSuccess)
            {
                var initResult = effect0Result.Value.Initialize(Cost);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ConsumeManaEffect: {effect0Result.Message}");
            }

            // Effect 1: CooldownEffect
            var effect1Result = GetEffect(1);
            if (effect1Result.IsSuccess)
            {
                var initResult = effect1Result.Value.Initialize(Cooltime, SkillIndex);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get CooldownEffect: {effect1Result.Message}");
            }

            // Effect 2: ActivateTriggerEffect
            var effect2Result = GetEffect(2);
            if (effect2Result.IsSuccess)
            {
                var triggers = new List<GameTrigger> { GameTrigger.SkillActivated };
                var initResult = effect2Result.Value.Initialize(triggers);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get ActivateTriggerEffect: {effect2Result.Message}");
            }

            // Effect 3: DeActivateTriggerEffect
            var effect3Result = GetEffect(3);
            if (effect3Result.IsSuccess)
            {
                var triggers = new List<GameTrigger> { GameTrigger.SkillActivated };
                var initResult = effect3Result.Value.Initialize(triggers);
                results.Add(initResult);
            }
            else
            {
                return SkillResult.Failure($"Failed to get DeActivateTriggerEffect: {effect3Result.Message}");
            }

            // 모든 초기화 결과 확인
            var failedResults = results.Where(r => !r.IsSuccess).ToList();
            if (failedResults.Any())
            {
                var errorMessages = string.Join(", ", failedResults.Select(r => r.Message));
                return SkillResult.Failure($"Effect initialization failed: {errorMessages}");
            }

            return base.Initialize();
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("PowerFistSkill initialization failed", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return SkillResult.Success("PowerFistSkill completed successfully");
        }

        var effectResult = GetEffect(_currentEffectIndex);
        if (!effectResult.IsSuccess)
        {
            return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
        }

        var success = effectResult.Value.Apply(source, source.target ?? null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++;
        }

        return SkillResult.Success("Effect applied, continuing skill execution");
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            if (!baseResult.IsSuccess)
                return baseResult;

            // 스킬별 정리 작업
            TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
            
            return SkillResult.Success("PowerFistSkill cleared successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("PowerFistSkill clear failed", ex);
        }
    }
}