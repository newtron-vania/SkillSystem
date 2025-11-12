using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Effect 시스템에서 사용되는 에러 핸들링 패턴과 유틸리티를 제공합니다.
/// </summary>
public static class EffectErrorHandling
{
    /// <summary>
    /// 여러 Effect를 안전하게 초기화하는 헬퍼 메서드입니다.
    /// </summary>
    public static SkillResult InitializeEffects(params (Effect effect, object[] parameters)[] effectInitializations)
    {
        var results = new List<SkillResult>();
        var failedEffects = new List<string>();

        for (int i = 0; i < effectInitializations.Length; i++)
        {
            var (effect, parameters) = effectInitializations[i];

            if (effect == null)
            {
                failedEffects.Add($"Effect {i}: null effect");
                continue;
            }

            try
            {
                var result = effect.Initialize(parameters);
                results.Add(result);

                if (!result.IsSuccess)
                {
                    failedEffects.Add($"Effect {i} ({effect.GetType().Name}): {result.Message}");
                }
            }
            catch (Exception ex)
            {
                failedEffects.Add($"Effect {i} ({effect.GetType().Name}): Exception - {ex.Message}");
            }
        }

        if (failedEffects.Any())
        {
            var errorMessage = string.Join("; ", failedEffects);
            return SkillResult.Failure($"Effect initialization failed: {errorMessage}");
        }

        return SkillResult.Success($"Successfully initialized {effectInitializations.Length} effects");
    }

    /// <summary>
    /// Effect 리스트를 안전하게 초기화하는 헬퍼 메서드입니다.
    /// </summary>
    public static SkillResult InitializeEffectList(List<Effect> effects, List<object[]> parametersList)
    {
        if (effects == null)
            return SkillResult.Failure("Effects list is null");

        if (parametersList == null)
            return SkillResult.Failure("Parameters list is null");

        if (effects.Count != parametersList.Count)
            return SkillResult.Failure($"Mismatch between effects count ({effects.Count}) and parameters count ({parametersList.Count})");

        var results = new List<SkillResult>();
        var failedEffects = new List<string>();

        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            var parameters = parametersList[i];

            if (effect == null)
            {
                failedEffects.Add($"Effect {i}: null effect");
                continue;
            }

            try
            {
                var result = effect.Initialize(parameters);
                results.Add(result);

                if (!result.IsSuccess)
                {
                    failedEffects.Add($"Effect {i} ({effect.GetType().Name}): {result.Message}");
                }
            }
            catch (Exception ex)
            {
                failedEffects.Add($"Effect {i} ({effect.GetType().Name}): Exception - {ex.Message}");
            }
        }

        if (failedEffects.Any())
        {
            var errorMessage = string.Join("; ", failedEffects);
            return SkillResult.Failure($"Effect initialization failed: {errorMessage}");
        }

        return SkillResult.Success($"Successfully initialized {effects.Count} effects");
    }

    /// <summary>
    /// 타입 안전성을 보장하며 파라미터를 검증하는 헬퍼 메서드입니다.
    /// </summary>
    public static SkillResult ValidateParameters<T1>(object[] parameters, out T1 param1)
    {
        param1 = default(T1);

        if (parameters == null || parameters.Length < 1)
            return SkillResult.Failure($"Expected 1 parameter of type {typeof(T1).Name}");

        if (!(parameters[0] is T1))
            return SkillResult.Failure($"Parameter 0: Expected {typeof(T1).Name}, got {parameters[0]?.GetType()?.Name ?? "null"}");

        param1 = (T1)parameters[0];
        return SkillResult.Success();
    }

    /// <summary>
    /// 2개 파라미터에 대한 타입 안전성 검증
    /// </summary>
    public static SkillResult ValidateParameters<T1, T2>(object[] parameters, out T1 param1, out T2 param2)
    {
        param1 = default(T1);
        param2 = default(T2);

        if (parameters == null || parameters.Length < 2)
            return SkillResult.Failure($"Expected 2 parameters of types {typeof(T1).Name}, {typeof(T2).Name}");

        if (!(parameters[0] is T1))
            return SkillResult.Failure($"Parameter 0: Expected {typeof(T1).Name}, got {parameters[0]?.GetType()?.Name ?? "null"}");

        if (!(parameters[1] is T2))
            return SkillResult.Failure($"Parameter 1: Expected {typeof(T2).Name}, got {parameters[1]?.GetType()?.Name ?? "null"}");

        param1 = (T1)parameters[0];
        param2 = (T2)parameters[1];
        return SkillResult.Success();
    }

    /// <summary>
    /// 3개 파라미터에 대한 타입 안전성 검증
    /// </summary>
    public static SkillResult ValidateParameters<T1, T2, T3>(object[] parameters, out T1 param1, out T2 param2, out T3 param3)
    {
        param1 = default(T1);
        param2 = default(T2);
        param3 = default(T3);

        if (parameters == null || parameters.Length < 3)
            return SkillResult.Failure($"Expected 3 parameters of types {typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}");

        if (!(parameters[0] is T1))
            return SkillResult.Failure($"Parameter 0: Expected {typeof(T1).Name}, got {parameters[0]?.GetType()?.Name ?? "null"}");

        if (!(parameters[1] is T2))
            return SkillResult.Failure($"Parameter 1: Expected {typeof(T2).Name}, got {parameters[1]?.GetType()?.Name ?? "null"}");

        if (!(parameters[2] is T3))
            return SkillResult.Failure($"Parameter 2: Expected {typeof(T3).Name}, got {parameters[2]?.GetType()?.Name ?? "null"}");

        param1 = (T1)parameters[0];
        param2 = (T2)parameters[1];
        param3 = (T3)parameters[2];
        return SkillResult.Success();
    }

    /// <summary>
    /// Effect Apply 메서드를 안전하게 실행하는 헬퍼 메서드입니다.
    /// </summary>
    public static bool SafeApplyEffect(Effect effect, Actor source, Actor target, string effectName = null)
    {
        if (effect == null)
        {
            Debug.LogError($"SafeApplyEffect: Effect is null ({effectName ?? "Unknown"})");
            return false;
        }

        if (!effect.IsInitialized)
        {
            Debug.LogError($"SafeApplyEffect: Effect not initialized ({effectName ?? effect.GetType().Name})");
            return false;
        }

        try
        {
            return effect.Apply(source, target);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SafeApplyEffect: Exception in {effectName ?? effect.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Effect Clear 메서드를 안전하게 실행하는 헬퍼 메서드입니다.
    /// </summary>
    public static void SafeClearEffect(Effect effect, string effectName = null)
    {
        if (effect == null)
        {
            Debug.LogWarning($"SafeClearEffect: Effect is null ({effectName ?? "Unknown"})");
            return;
        }

        try
        {
            effect.Clear();
        }
        catch (Exception ex)
        {
            Debug.LogError($"SafeClearEffect: Exception in {effectName ?? effect.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 디버깅을 위한 Effect 상태 로깅
    /// </summary>
    public static void LogEffectState(Effect effect, string context = "")
    {
        if (effect == null)
        {
            Debug.Log($"[{context}] Effect: NULL");
            return;
        }

        Debug.Log($"[{context}] Effect: {effect.GetType().Name}, Initialized: {effect.IsInitialized}");
    }

    /// <summary>
    /// Effect 리스트의 상태를 로깅
    /// </summary>
    public static void LogEffectListState(List<Effect> effects, string context = "")
    {
        if (effects == null)
        {
            Debug.Log($"[{context}] Effect List: NULL");
            return;
        }

        Debug.Log($"[{context}] Effect List: {effects.Count} effects");
        for (int i = 0; i < effects.Count; i++)
        {
            LogEffectState(effects[i], $"{context} [{i}]");
        }
    }
}

/// <summary>
/// Effect 초기화를 위한 빌더 패턴 구현
/// </summary>
public class EffectInitializationBuilder
{
    private readonly List<(Effect effect, object[] parameters)> _effectInitializations = new();

    public EffectInitializationBuilder AddEffect<T>(T effect, params object[] parameters) where T : Effect
    {
        _effectInitializations.Add((effect, parameters));
        return this;
    }

    public SkillResult Initialize()
    {
        return EffectErrorHandling.InitializeEffects(_effectInitializations.ToArray());
    }

    public int Count => _effectInitializations.Count;

    public void Clear()
    {
        _effectInitializations.Clear();
    }
}