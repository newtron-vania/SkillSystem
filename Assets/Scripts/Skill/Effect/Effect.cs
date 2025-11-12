using System;
using UnityEngine;

public abstract class Effect
{
    /// <summary>
    /// Effect의 초기화 상태를 확인합니다.
    /// </summary>
    public virtual bool IsInitialized { get; protected set; } = false;

    /// <summary>
    /// 효과에 필요한 초기 값을 설정하는 메서드입니다.
    /// 파라미터로 전달된 객체들을 내부 변수에 저장하거나 초기화할 때 사용합니다.
    /// 예: 지속 시간, 데미지 배수, 스킬 인덱스 등.
    /// </summary>
    public virtual SkillResult Initialize(params object[] objects)
    {
        try
        {
            var result = DoInitialize(objects);
            if (result.IsSuccess)
            {
                IsInitialized = true;
            }
            return result;
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error initializing effect {GetType().Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 실제 초기화 로직을 구현하는 메서드입니다.
    /// </summary>
    protected abstract SkillResult DoInitialize(params object[] objects);

    /// <summary>
    /// 실제 효과를 적용하는 메서드입니다.
    /// source(시전자)와 target(피대상) 정보를 기반으로 효과를 실행하며,
    /// 성공적으로 적용되면 true를 반환합니다.
    /// 일부 효과는 조건에 따라 false를 반환해 다음 효과 적용을 대기시킬 수 있습니다.
    /// </summary>
    public virtual bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError($"Effect {GetType().Name} is not initialized. Call Initialize() first.");
            return false;
        }

        try
        {
            return DoApply(source, target);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error applying effect {GetType().Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 실제 효과 적용 로직을 구현하는 메서드입니다.
    /// </summary>
    protected abstract bool DoApply(Actor source, Actor target);

    /// <summary>
    /// 효과 적용 후 정리 작업을 수행하는 메서드입니다.
    /// 예: 생성한 오브젝트 제거, 상태 초기화, 트리거 비활성화 등.
    /// 지속성 효과나 UI 시각화 후 제거가 필요한 경우 반드시 호출됩니다.
    /// </summary>
    public virtual void Clear()
    {
        try
        {
            DoClear();
            IsInitialized = false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error clearing effect {GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 실제 정리 로직을 구현하는 메서드입니다.
    /// </summary>
    protected abstract void DoClear();

    /// <summary>
    /// 기본 SkillResult를 생성하는 헬퍼 메서드입니다.
    /// </summary>
    protected SkillResult Success(string message = "") => SkillResult.Success(message);
    protected SkillResult Failure(string message) => SkillResult.Failure(message);
    protected SkillResult Failure(string message, Exception error) => SkillResult.Failure(message, error);

    /// <summary>
    /// 파라미터 유효성 검사를 수행합니다.
    /// </summary>
    protected SkillResult ValidateParameters(object[] parameters, int expectedCount, string effectName)
    {
        if (parameters == null)
            return Failure($"{effectName}: Parameters cannot be null");

        if (parameters.Length < expectedCount)
            return Failure($"{effectName}: Expected at least {expectedCount} parameters, got {parameters.Length}");

        return Success();
    }

    /// <summary>
    /// 파라미터를 안전하게 특정 타입으로 캐스팅합니다.
    /// </summary>
    protected T SafeCast<T>(object parameter, string parameterName)
    {
        if (parameter is T result)
            return result;

        throw new ArgumentException($"Parameter '{parameterName}' must be of type {typeof(T).Name}, got {parameter?.GetType().Name ?? "null"}");
    }
}